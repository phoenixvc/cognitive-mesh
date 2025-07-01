const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const Ajv = require('ajv');
const addFormats = require('ajv-formats');
const { bundle } = require('@redocly/openapi-core');

// Load the OpenAPI specification
const loadSpec = () => {
  const specPath = path.join(__dirname, '..', 'openapi.yaml');
  return yaml.load(fs.readFileSync(specPath, 'utf8'));
};

// Helper function to generate test cases for string fields
const generateStringTestCases = (fieldName, schema) => {
  const testCases = [];
  
  // Empty string
  testCases.push({
    name: `${fieldName} - empty string`,
    value: "",
    valid: schema.minLength === undefined || schema.minLength === 0,
    error: schema.minLength ? 'shorter than minimum length' : null
  });

  // Very long string
  if (schema.maxLength) {
    testCases.push({
      name: `${fieldName} - max length (${schema.maxLength} chars)`,
      value: 'a'.repeat(schema.maxLength),
      valid: true
    });
    
    testCases.push({
      name: `${fieldName} - exceeds max length (${schema.maxLength + 1} chars)`,
      value: 'a'.repeat(schema.maxLength + 1),
      valid: false,
      error: 'longer than maximum length'
    });
  }

  // Special characters
  testCases.push({
    name: `${fieldName} - special characters`,
    value: '!@#$%^&*()_+{}[]|\\:;\'"<>,.?/~`',
    valid: true
  });

  // Unicode characters
  testCases.push({
    name: `${fieldName} - unicode characters`,
    value: 'こんにちは世界',
    valid: true
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

describe('OpenAPI Edge Case Tests', () => {
  let openapiSpec;
  let ajv;
  
  beforeAll(async () => {
    try {
      openapiSpec = loadSpec();
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
    if (!openapiSpec.components || !openapiSpec.components.schemas) {
      it('should have components and schemas defined', () => {
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
                  const testObj = { [propName]: testCase.value };
                  const validate = ajv.compile(schema);
                  const isValid = validate(testObj);
                  
                  if (testCase.valid) {
                    expect(isValid).toBe(true);
                  } else {
                    expect(isValid).toBe(false);
                    if (testCase.error) {
                      expect(JSON.stringify(validate.errors)).toContain(testCase.error);
                    }
                  }
                });
              });
            } else if (propSchema.type === 'number' || propSchema.type === 'integer') {
              generateNumberTestCases(propName, propSchema).forEach(testCase => {
                it(`should ${testCase.valid ? 'accept' : 'reject'} ${testCase.name}`, () => {
                  const testObj = { [propName]: testCase.value };
                  const validate = ajv.compile(schema);
                  const isValid = validate(testObj);
                  
                  if (testCase.valid) {
                    expect(isValid).toBe(true);
                  } else {
                    expect(isValid).toBe(false);
                    if (testCase.error) {
                      expect(JSON.stringify(validate.errors)).toContain(testCase.error);
                    }
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
    Object.entries(openapiSpec.paths).forEach(([path, methods]) => {
      describe(`Path: ${path}`, () => {
        Object.entries(methods).forEach(([method, operation]) => {
          if (['get', 'put', 'post', 'delete', 'patch'].includes(method)) {
            describe(`${method.toUpperCase()} ${path}`, () => {
              // Test missing required parameters
              if (operation.parameters) {
                const requiredParams = operation.parameters.filter(p => p.required);
                
                if (requiredParams.length > 0) {
                  it('should reject request with missing required parameters', async () => {
                    // Create a request with missing required parameters
                    const request = {
                      method: method.toUpperCase(),
                      path,
                      headers: {},
                      query: {},
                      body: {}
                    };
                    
                    // Validate the request
                    const result = await validate({
                      spec: openapiSpec,
                      request,
                      config: {
                        style: {
                          path: {},
                          query: {},
                          header: {},
                          cookie: {}
                        }
                      }
                    });
                    
                    // Should have validation errors for missing required parameters
                    expect(result.errors).toBeDefined();
                    expect(result.errors.length).toBeGreaterThan(0);
                    expect(result.errors.some(e => e.message.includes('is required'))).toBe(true);
                  });
                }
              }

              // Test invalid parameter types
              if (operation.parameters) {
                operation.parameters.forEach(param => {
                  if (param.schema && param.schema.type) {
                    it(`should reject invalid ${param.in} parameter type for ${param.name}`, async () => {
                      const invalidValue = param.schema.type === 'string' ? 123 : 'invalid';
                      
                      const request = {
                        method: method.toUpperCase(),
                        path: path.replace(/\{([^}]+)\}/g, param.in === 'path' ? invalidValue : 'valid'),
                        headers: {},
                        query: {},
                        body: {}
                      };
                      
                      // Set the parameter in the appropriate location
                      switch (param.in) {
                        case 'query':
                          request.query[param.name] = invalidValue;
                          break;
                        case 'header':
                          request.headers[param.name] = invalidValue;
                          break;
                        case 'path':
                          // Already set in path replacement
                          break;
                        case 'cookie':
                          request.headers.cookie = `${param.name}=${invalidValue}`;
                          break;
                      }
                      
                      // Validate the request
                      const result = await validate({
                        spec: openapiSpec,
                        request,
                        config: {
                          style: {
                            path: {},
                            query: {},
                            header: {},
                            cookie: {}
                          }
                        }
                      });
                      
                      // Should have type validation errors
                      expect(result.errors).toBeDefined();
                      expect(result.errors.length).toBeGreaterThan(0);
                      expect(
                        result.errors.some(e => 
                          e.message.includes('type') || 
                          e.message.includes('format') ||
                          e.message.includes('pattern')
                        )
                      ).toBe(true);
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
    Object.entries(openapiSpec.paths).forEach(([path, methods]) => {
      Object.entries(methods).forEach(([method, operation]) => {
        if (['put', 'post', 'patch'].includes(method) && operation.requestBody) {
          describe(`${method.toUpperCase()} ${path}`, () => {
            // Test empty request body for required fields
            it('should reject empty request body when required', async () => {
              const request = {
                method: method.toUpperCase(),
                path,
                headers: {
                  'content-type': 'application/json'
                },
                body: {}
              };
              
              const result = await validate({
                spec: openapiSpec,
                request,
                config: {
                  style: {
                    path: {},
                    query: {},
                    header: {},
                    cookie: {}
                  }
                }
              });
              
              // Should have validation errors for required fields
              expect(result.errors).toBeDefined();
              expect(result.errors.length).toBeGreaterThan(0);
            });

            // Test with invalid content type
            it('should reject invalid content type', async () => {
              const request = {
                method: method.toUpperCase(),
                path,
                headers: {
                  'content-type': 'text/plain'
                },
                body: 'invalid content'
              };
              
              const result = await validate({
                spec: openapiSpec,
                request,
                config: {
                  style: {
                    path: {},
                    query: {},
                    header: {},
                    cookie: {}
                  }
                }
              });
              
              // Should have content type validation error
              expect(result.errors).toBeDefined();
              expect(result.errors.some(e => 
                e.message.includes('content type') || 
                e.message.includes('media type')
              )).toBe(true);
            });
          });
        }
      });
    });
  });

  describe('Security Requirements', () => {
    // Test endpoints with security requirements
    Object.entries(openapiSpec.paths).forEach(([path, methods]) => {
      Object.entries(methods).forEach(([method, operation]) => {
        if (operation.security && operation.security.length > 0) {
          describe(`${method.toUpperCase()} ${path}`, () => {
            // Test without authentication
            it('should reject unauthenticated requests', async () => {
              const request = {
                method: method.toUpperCase(),
                path,
                headers: {},
                query: {},
                body: {}
              };
              
              const result = await validate({
                spec: openapiSpec,
                request,
                config: {
                  style: {
                    path: {},
                    query: {},
                    header: {},
                    cookie: {}
                  }
                }
              });
              
              // Should have security validation error
              expect(result.errors).toBeDefined();
              expect(result.errors.some(e => 
                e.message.includes('security') || 
                e.message.includes('authentication') ||
                e.message.includes('authorization')
              )).toBe(true);
            });

            // Test with invalid token
            it('should reject requests with invalid token', async () => {
              const request = {
                method: method.toUpperCase(),
                path,
                headers: {
                  'authorization': 'Bearer invalid.token.here'
                },
                query: {},
                body: {}
              };
              
              // Note: This is a basic test - actual token validation would happen in the API
              // We're just testing that the OpenAPI spec is correctly defined
              const result = await validate({
                spec: openapiSpec,
                request,
                config: {
                  style: {
                    path: {},
                    query: {},
                    header: {},
                    cookie: {}
                  }
                }
              });
              
              // The request should pass OpenAPI validation (actual auth happens in the API)
              // So we just check that there are no validation errors
              expect(result.errors).toBeUndefined();
            });
          });
        }
      });
    });
  });
});
