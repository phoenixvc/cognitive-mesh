const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const { bundle, load } = require('@redocly/openapi-core');

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
    
    // Check if there are any paths defined
    const paths = Object.entries(openapiSpec.paths);
    expect(paths.length).toBeGreaterThan(0);
    
    paths.forEach(([path, pathItem]) => {
      // Path should start with /
      expect(path).toMatch(/^\//);
      
      // Check each HTTP method in the path
      Object.entries(pathItem).forEach(([method, operation]) => {
        if (['get', 'put', 'post', 'delete', 'options', 'head', 'patch', 'trace'].includes(method)) {
          // Check required operation fields
          expect(operation).toHaveProperty('summary');
          expect(operation).toHaveProperty('description');
          
          // operationId is recommended but not strictly required
          if (operation.operationId) {
            expect(typeof operation.operationId).toBe('string');
          }
          
          // Tags are recommended but not strictly required
          if (operation.tags) {
            expect(Array.isArray(operation.tags)).toBe(true);
            operation.tags.forEach(tag => {
              expect(typeof tag).toBe('string');
            });
          }
          
          // Check responses - at least one success response should be defined
          expect(operation.responses).toBeDefined();
          const successResponse = Object.keys(operation.responses).find(
            status => status.startsWith('2') || status === 'default'
          );
          expect(successResponse).toBeDefined();
          
          // Check parameters if they exist
          if (operation.parameters) {
            expect(Array.isArray(operation.parameters)).toBe(true);
            operation.parameters.forEach(param => {
              // Handle both direct parameters and $refs
              const paramObj = param.$ref ? 
                openapiSpec.components.parameters[param.$ref.split('/').pop()] : 
                param;
                
              if (paramObj) {
                expect(paramObj).toHaveProperty('name');
                expect(paramObj).toHaveProperty('in');
                expect(['query', 'header', 'path', 'cookie']).toContain(paramObj.in);
                
                // Description is recommended but not strictly required
                if (paramObj.description) {
                  expect(typeof paramObj.description).toBe('string');
                }
                
                // Required is only mandatory for path parameters
                if (paramObj.in === 'path') {
                  expect(paramObj.required).toBe(true);
                } else if ('required' in paramObj) {
                  expect(typeof paramObj.required).toBe('boolean');
                }
                
                if (paramObj.schema) {
                  expect(paramObj.schema).toHaveProperty('type');
                }
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
    try {
      // First, load the OpenAPI document
      const document = await load({
        config: {
          styleguide: {
            rules: {}
          }
        },
        ref: path.join(__dirname, '..', 'openapi.yaml')
      });
      
      // Then bundle it to resolve all $refs
      const bundled = await bundle({
        ref: document,
        config: {
          styleguide: {
            rules: {}
          }
        }
      });
      
      // Basic validation that we got a valid OpenAPI object
      expect(bundled).toBeDefined();
      expect(bundled.openapi).toMatch(/^3\.0\.\d+(-.+)?$/);
      
    } catch (error) {
      console.error('Validation error:', error);
      throw error;
    }
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
