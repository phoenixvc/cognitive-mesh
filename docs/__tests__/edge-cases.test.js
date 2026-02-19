const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const Ajv = require('ajv');
const addFormats = require('ajv-formats');

// Load the OpenAPI specification
const loadSpec = () => {
  const specPath = path.join(__dirname, '..', 'openapi.yaml');
  return yaml.load(fs.readFileSync(specPath, 'utf8'));
};

// Formats that constrain string values beyond plain text
const STRICT_FORMATS = new Set([
  'uuid', 'date-time', 'date', 'time', 'email', 'uri', 'uri-reference',
  'hostname', 'ipv4', 'ipv6', 'duration'
]);

// Helper function to generate test cases for string fields
const generateStringTestCases = (fieldName, schema) => {
  const testCases = [];
  const hasStrictFormat = STRICT_FORMATS.has(schema.format);
  const hasEnum = Array.isArray(schema.enum);
  const hasPattern = !!schema.pattern;

  // Empty string - invalid for format-constrained, enum, or pattern fields
  testCases.push({
    name: `${fieldName} - empty string`,
    value: "",
    valid: !hasStrictFormat && !hasEnum && !hasPattern &&
      (schema.minLength === undefined || schema.minLength === 0),
    error: schema.minLength ? 'shorter than minimum length' : null
  });

  // Very long string
  if (schema.maxLength) {
    testCases.push({
      name: `${fieldName} - max length (${schema.maxLength} chars)`,
      value: 'a'.repeat(schema.maxLength),
      valid: !hasStrictFormat && !hasEnum && !hasPattern
    });

    testCases.push({
      name: `${fieldName} - exceeds max length (${schema.maxLength + 1} chars)`,
      value: 'a'.repeat(schema.maxLength + 1),
      valid: false,
      error: 'longer than maximum length'
    });
  }

  // Special characters - invalid for format-constrained, enum, or pattern fields
  testCases.push({
    name: `${fieldName} - special characters`,
    value: '!@#$%^&*()_+{}[]|\\:;\'"<>,.?/~`',
    valid: !hasStrictFormat && !hasEnum && !hasPattern
  });

  // Unicode characters - invalid for format-constrained, enum, or pattern fields
  testCases.push({
    name: `${fieldName} - unicode characters`,
    value: '\u3053\u3093\u306b\u3061\u306f\u4e16\u754c',
    valid: !hasStrictFormat && !hasEnum && !hasPattern
  });

  return testCases;
};

// Helper function to generate test cases for number fields
const generateNumberTestCases = (fieldName, schema) => {
  const testCases = [];

  // Minimum value
  if (schema.minimum !== undefined) {
    testCases.push({
      name: `${fieldName} - minimum value (${schema.minimum})`,
      value: schema.minimum,
      valid: true
    });

    testCases.push({
      name: `${fieldName} - below minimum (${schema.minimum - 1})`,
      value: schema.minimum - 1,
      valid: false,
      error: 'must be >= '
    });
  }

  // Maximum value
  if (schema.maximum !== undefined) {
    testCases.push({
      name: `${fieldName} - maximum value (${schema.maximum})`,
      value: schema.maximum,
      valid: true
    });

    testCases.push({
      name: `${fieldName} - above maximum (${schema.maximum + 1})`,
      value: schema.maximum + 1,
      valid: false,
      error: 'must be <='
    });
  }

  // Edge cases
  testCases.push({
    name: `${fieldName} - zero`,
    value: 0,
    valid: schema.minimum === undefined || schema.minimum <= 0
  });

  testCases.push({
    name: `${fieldName} - negative`,
    value: -1,
    valid: schema.minimum === undefined || schema.minimum <= -1
  });

  return testCases;
};

// Load spec synchronously at module scope so it's available during test collection
const openapiSpec = loadSpec();

describe('OpenAPI Edge Case Tests', () => {
  let ajv;

  beforeAll(async () => {
    try {
      if (!openapiSpec) {
        throw new Error('Failed to load OpenAPI spec');
      }

      ajv = new Ajv({
        strict: false,
        allErrors: true,
        validateFormats: true
      });
      addFormats(ajv);
    } catch (error) {
      console.error('Error in beforeAll:', error);
      throw error;
    }
  });

  describe('Schema Validation', () => {
    if (!openapiSpec || !openapiSpec.components || !openapiSpec.components.schemas) {
      it('should have components and schemas defined', () => {
        expect(openapiSpec).toBeDefined();
        expect(openapiSpec.components).toBeDefined();
        expect(openapiSpec.components.schemas).toBeDefined();
      });
      return;
    }

    // Test all schemas for edge cases
    Object.entries(openapiSpec.components.schemas).forEach(([schemaName, schema]) => {
      describe(`Schema: ${schemaName}`, () => {
        if (schema.properties) {
          Object.entries(schema.properties).forEach(([propName, propSchema]) => {
            if (propSchema.type === 'string') {
              generateStringTestCases(propName, propSchema).forEach(testCase => {
                it(`should ${testCase.valid ? 'accept' : 'reject'} ${testCase.name}`, () => {
                  // Compile only the individual property schema to avoid required-field errors
                  const propertySchema = { type: 'object', properties: { [propName]: propSchema } };
                  const validate = ajv.compile(propertySchema);
                  const isValid = validate({ [propName]: testCase.value });

                  if (testCase.valid) {
                    expect(isValid).toBe(true);
                  } else {
                    expect(isValid).toBe(false);
                  }
                });
              });
            } else if (propSchema.type === 'number' || propSchema.type === 'integer') {
              generateNumberTestCases(propName, propSchema).forEach(testCase => {
                it(`should ${testCase.valid ? 'accept' : 'reject'} ${testCase.name}`, () => {
                  // Compile only the individual property schema to avoid required-field errors
                  const propertySchema = { type: 'object', properties: { [propName]: propSchema } };
                  const validate = ajv.compile(propertySchema);
                  const isValid = validate({ [propName]: testCase.value });

                  if (testCase.valid) {
                    expect(isValid).toBe(true);
                  } else {
                    expect(isValid).toBe(false);
                  }
                });
              });
            }
          });
        }
      });
    });
  });

  describe('Path Parameter Validation', () => {
    Object.entries(openapiSpec.paths).forEach(([pathStr, methods]) => {
      describe(`Path: ${pathStr}`, () => {
        Object.entries(methods).forEach(([method, operation]) => {
          if (['get', 'put', 'post', 'delete', 'patch'].includes(method)) {
            describe(`${method.toUpperCase()} ${pathStr}`, () => {
              if (operation.parameters) {
                const requiredParams = operation.parameters.filter(p => p.required);

                if (requiredParams.length > 0) {
                  it('should define required parameters with schemas', () => {
                    requiredParams.forEach(param => {
                      expect(param.name).toBeDefined();
                      expect(param.in).toBeDefined();
                      expect(param.required).toBe(true);
                      expect(param.schema).toBeDefined();
                    });
                  });
                }
              }

              if (operation.parameters) {
                operation.parameters.forEach(param => {
                  if (param.schema && param.schema.type) {
                    it(`should define schema type for ${param.in} parameter ${param.name}`, () => {
                      expect(param.schema.type).toBeDefined();
                      expect(['string', 'integer', 'number', 'boolean', 'array', 'object'])
                        .toContain(param.schema.type);

                      if (param.in === 'path') {
                        expect(param.required).toBe(true);
                      }
                    });
                  }
                });
              }
            });
          }
        });
      });
    });
  });

  describe('Request Body Validation', () => {
    Object.entries(openapiSpec.paths).forEach(([pathStr, methods]) => {
      Object.entries(methods).forEach(([method, operation]) => {
        if (['put', 'post', 'patch'].includes(method) && operation.requestBody) {
          describe(`${method.toUpperCase()} ${pathStr}`, () => {
            it('should define request body with content type', () => {
              expect(operation.requestBody.content).toBeDefined();
              const contentTypes = Object.keys(operation.requestBody.content);
              expect(contentTypes.length).toBeGreaterThan(0);
              expect(contentTypes).toContain('application/json');
            });

            it('should define request body schema', () => {
              const jsonContent = operation.requestBody.content['application/json'];
              expect(jsonContent).toBeDefined();
              expect(jsonContent.schema).toBeDefined();
            });

            if (operation.requestBody.required) {
              it('should mark request body as required', () => {
                expect(operation.requestBody.required).toBe(true);
              });
            }
          });
        }
      });
    });
  });

  describe('Security Requirements', () => {
    it('should have global security defined', () => {
      expect(openapiSpec.security).toBeDefined();
      expect(Array.isArray(openapiSpec.security)).toBe(true);
      expect(openapiSpec.security.length).toBeGreaterThan(0);
    });

    it('should reference valid security schemes', () => {
      const definedSchemes = Object.keys(openapiSpec.components.securitySchemes || {});
      openapiSpec.security.forEach(securityRequirement => {
        Object.keys(securityRequirement).forEach(schemeName => {
          expect(definedSchemes).toContain(schemeName);
        });
      });
    });

    // Test endpoints with operation-level security
    Object.entries(openapiSpec.paths).forEach(([pathStr, methods]) => {
      Object.entries(methods).forEach(([method, operation]) => {
        if (operation.security && operation.security.length > 0) {
          describe(`${method.toUpperCase()} ${pathStr}`, () => {
            it('should reference valid security schemes', () => {
              const definedSchemes = Object.keys(openapiSpec.components.securitySchemes || {});
              operation.security.forEach(securityRequirement => {
                Object.keys(securityRequirement).forEach(schemeName => {
                  expect(definedSchemes).toContain(schemeName);
                });
              });
            });
          });
        }
      });
    });
  });
});
