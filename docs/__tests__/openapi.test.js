const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const { validate } = require('@redocly/openapi-core');
const { bundle } = require('@redocly/openapi-core');
const { Oas3Tools } = require('@redocly/openapi-core');

// Helper function to load YAML file
const loadYaml = (filePath) => {
  try {
    return yaml.load(fs.readFileSync(filePath, 'utf8'));
  } catch (e) {
    console.error(`Error loading YAML file: ${filePath}`, e);
    throw e;
  }
};

describe('OpenAPI Specification', () => {
  let openapiSpec;
  
  beforeAll(async () => {
    // Load the bundled OpenAPI spec
    const filePath = path.join(__dirname, '..', 'openapi.yaml');
    openapiSpec = loadYaml(filePath);
  });

  test('should be valid OpenAPI 3.0.x', async () => {
    expect(openapiSpec.openapi).toMatch(/^3\.0\.\d+(-.+)?$/);
  });

  test('should have required info fields', () => {
    const { info } = openapiSpec;
    expect(info).toBeDefined();
    expect(info.title).toBeDefined();
    expect(info.version).toBeDefined();
    expect(info.description).toBeDefined();
    expect(info.contact).toBeDefined();
    expect(info.contact.name).toBeDefined();
    expect(info.contact.email).toBeDefined();
  });

  test('should have at least one server defined', () => {
    expect(Array.isArray(openapiSpec.servers)).toBe(true);
    expect(openapiSpec.servers.length).toBeGreaterThan(0);
  });

  test('should have valid paths', () => {
    expect(openapiSpec.paths).toBeDefined();
    expect(typeof openapiSpec.paths).toBe('object');
    
    Object.entries(openapiSpec.paths).forEach(([path, pathItem]) => {
      expect(path).toMatch(/^\//); // Paths should start with /
      
      // Check each HTTP method in the path
      Object.entries(pathItem).forEach(([method, operation]) => {
        if (['get', 'put', 'post', 'delete', 'options', 'head', 'patch', 'trace'].includes(method)) {
          // Check required operation fields
          expect(operation).toHaveProperty('summary');
          expect(operation).toHaveProperty('description');
          expect(operation).toHaveProperty('operationId');
          expect(operation).toHaveProperty('tags');
          expect(Array.isArray(operation.tags)).toBe(true);
          expect(operation.tags.length).toBeGreaterThan(0);
          
          // Check responses
          expect(operation.responses).toBeDefined();
          expect(operation.responses['200'] || operation.responses['201']).toBeDefined();
          
          // Check parameters if they exist
          if (operation.parameters) {
            expect(Array.isArray(operation.parameters)).toBe(true);
            operation.parameters.forEach(param => {
              expect(param).toHaveProperty('name');
              expect(param).toHaveProperty('in');
              expect(['query', 'header', 'path', 'cookie']).toContain(param.in);
              expect(param).toHaveProperty('description');
              expect(param).toHaveProperty('required');
              expect(typeof param.required).toBe('boolean');
              
              if (param.schema) {
                expect(param.schema).toHaveProperty('type');
              }
            });
          }
        }
      });
    });
  });

  test('should have valid components', () => {
    expect(openapiSpec.components).toBeDefined();
    
    // Check schemas
    if (openapiSpec.components.schemas) {
      Object.entries(openapiSpec.components.schemas).forEach(([name, schema]) => {
        expect(schema).toBeDefined();
        expect(schema.type).toBeDefined();
      });
    }
    
    // Check security schemes
    if (openapiSpec.components.securitySchemes) {
      Object.entries(openapiSpec.components.securitySchemes).forEach(([name, scheme]) => {
        expect(scheme).toBeDefined();
        expect(scheme.type).toBeDefined();
        
        if (scheme.type === 'http') {
          expect(scheme.scheme).toBeDefined();
        } else if (scheme.type === 'apiKey') {
          expect(scheme.name).toBeDefined();
          expect(scheme.in).toBeDefined();
        } else if (scheme.type === 'oauth2' || scheme.type === 'openIdConnect') {
          expect(scheme.flows).toBeDefined();
        }
      });
    }
  });

  test('should pass Redocly validation', async () => {
    const config = await bundle({
      ref: path.join(__dirname, '..', 'openapi.yaml'),
      config: path.join(__dirname, '..', '.redocly.yaml')
    });
    
    const results = await validate({
      ref: path.join(__dirname, '..', 'openapi.yaml'),
      config
    });
    
    // Log any validation errors
    results.forEach(result => {
      if (result.errors && result.errors.length > 0) {
        console.error('Validation errors:', result.errors);
      }
    });
    
    // Check if there are any validation errors
    const hasErrors = results.some(result => result.errors && result.errors.length > 0);
    expect(hasErrors).toBe(false);
  });

  test('should have consistent operationIds', () => {
    const operationIds = new Set();
    const duplicateOperationIds = new Set();
    
    // Collect all operationIds
    Object.values(openapiSpec.paths).forEach(pathItem => {
      Object.entries(pathItem).forEach(([method, operation]) => {
        if (operation.operationId) {
          if (operationIds.has(operation.operationId)) {
            duplicateOperationIds.add(operation.operationId);
          } else {
            operationIds.add(operation.operationId);
          }
        }
      });
    });
    
    // Check for duplicate operationIds
    if (duplicateOperationIds.size > 0) {
      console.error('Duplicate operationIds found:', Array.from(duplicateOperationIds));
    }
    
    expect(duplicateOperationIds.size).toBe(0);
  });

  test('should have security schemes defined for all operations', () => {
    Object.entries(openapiSpec.paths).forEach(([path, pathItem]) => {
      Object.entries(pathItem).forEach(([method, operation]) => {
        if (['get', 'put', 'post', 'delete', 'options', 'head', 'patch', 'trace'].includes(method)) {
          // Check if operation has security requirements
          if (operation.security) {
            operation.security.forEach(securityRequirement => {
              Object.keys(securityRequirement).forEach(schemeName => {
                expect(openapiSpec.components.securitySchemes).toHaveProperty(schemeName);
              });
            });
          }
        }
      });
    });
  });
});
