const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const { globSync } = require('glob');
const { execSync } = require('child_process');

// Configuration
const ROOT_DIR = path.join(__dirname, '..');
const SPEC_DIR = path.join(ROOT_DIR, 'spec');
const OUTPUT_FILE = path.join(ROOT_DIR, 'openapi.yaml');
const EXAMPLES_FILE = path.join(SPEC_DIR, 'common', 'examples.yaml');

// Load examples
const examples = yaml.load(fs.readFileSync(EXAMPLES_FILE, 'utf8')).examples;

// Helper function to load YAML file
const loadYaml = (filePath) => {
  try {
    return yaml.load(fs.readFileSync(filePath, 'utf8'));
  } catch (e) {
    console.error(`Error loading YAML file: ${filePath}`, e);
    throw e;
  }
};

// Helper function to save YAML file
const saveYaml = (filePath, data) => {
  try {
    fs.writeFileSync(filePath, yaml.dump(data, { lineWidth: -1, noRefs: true }), 'utf8');
    console.log(`Saved: ${path.relative(process.cwd(), filePath)}`);
  } catch (e) {
    console.error(`Error saving YAML file: ${filePath}`, e);
    throw e;
  }
};

// Function to enhance a single schema with examples
const enhanceSchemaWithExamples = (schema, schemaName) => {
  if (!schema) return schema;
  
  // Skip if already has an example
  if (schema.example) return schema;
  
  // Map schema names to example names
  const exampleMap = {
    'User': 'userProfile',
    'AuthToken': 'authTokenResponse',
    'Error': 'errorBadRequest',
    'Pagination': 'paginatedResponse',
    'Timestamp': 'timestampExample'
  };
  
  const exampleName = exampleMap[schemaName];
  if (exampleName && examples[exampleName]) {
    schema.example = examples[exampleName].value;
  }
  
  // Handle nested properties
  if (schema.properties) {
    Object.entries(schema.properties).forEach(([propName, propSchema]) => {
      if (propSchema.$ref) {
        // Handle nested schemas via $ref
        const refName = propSchema.$ref.split('/').pop();
        if (refName && refName !== schemaName) { // Prevent infinite recursion
          const refSchema = loadYaml(path.join(SPEC_DIR, 'common', 'components.yaml'));
          if (refSchema.components && refSchema.components.schemas && refSchema.components.schemas[refName]) {
            schema.properties[propName] = enhanceSchemaWithExamples(
              { ...propSchema, ...refSchema.components.schemas[refName] },
              refName
            );
          }
        }
      } else if (propSchema.type === 'array' && propSchema.items) {
        // Handle arrays
        if (propSchema.items.$ref) {
          const refName = propSchema.items.$ref.split('/').pop();
          if (refName) {
            const refSchema = loadYaml(path.join(SPEC_DIR, 'common', 'components.yaml'));
            if (refSchema.components && refSchema.components.schemas && refSchema.components.schemas[refName]) {
              schema.properties[propName].items = enhanceSchemaWithExamples(
                { ...propSchema.items, ...refSchema.components.schemas[refName] },
                refName
              );
            }
          }
        } else if (!propSchema.items.example && propSchema.items.type) {
          // Add simple array examples based on type
          const typeExamples = {
            'string': ['item1', 'item2'],
            'number': [1, 2, 3],
            'boolean': [true, false],
            'integer': [1, 2, 3]
          };
          
          if (typeExamples[propSchema.items.type]) {
            schema.properties[propName].items.example = typeExamples[propSchema.items.type][0];
            schema.properties[propName].example = typeExamples[propSchema.items.type];
          }
        }
      } else if (!propSchema.example && propSchema.type) {
        // Add simple examples based on type
        const typeExamples = {
          'string': 'example',
          'number': 0,
          'boolean': true,
          'integer': 0,
          'date-time': new Date().toISOString()
        };
        
        if (typeExamples[propSchema.type]) {
          schema.properties[propName].example = typeExamples[propSchema.type];
        }
      }
    });
  }
  
  return schema;
};

// Function to enhance all schemas in components
const enhanceComponents = (components) => {
  if (!components) return components;
  
  // Enhance schemas
  if (components.schemas) {
    Object.entries(components.schemas).forEach(([schemaName, schema]) => {
      components.schemas[schemaName] = enhanceSchemaWithExamples(schema, schemaName);
    });
  }
  
  // Enhance security schemes
  if (components.securitySchemes) {
    Object.values(components.securitySchemes).forEach(scheme => {
      if (!scheme.description) {
        scheme.description = `Security scheme using ${scheme.type} authentication`;
      }
    });
  }
  
  return components;
};

// Function to enhance paths with examples and descriptions
const enhancePaths = (paths) => {
  if (!paths) return paths;
  
  Object.entries(paths).forEach(([path, methods]) => {
    Object.entries(methods).forEach(([method, operation]) => {
      if (['get', 'put', 'post', 'delete', 'patch'].includes(method)) {
        // Add operationId if missing
        if (!operation.operationId) {
          const pathParts = path.split('/').filter(Boolean);
          const resource = pathParts[pathParts.length - 1].replace(/[{}]/g, '');
          operation.operationId = `${method}${resource.charAt(0).toUpperCase() + resource.slice(1)}`;
        }
        
        // Add summary if missing
        if (!operation.summary) {
          operation.summary = `${method.toUpperCase()} ${path}`;
        }
        
        // Add description if missing
        if (!operation.description) {
          operation.description = `Performs ${method} operation on ${path}`;
        }
        
        // Add tags if missing
        if (!operation.tags || !operation.tags.length) {
          const tag = path.split('/').filter(Boolean)[0] || 'default';
          operation.tags = [tag];
        }
        
        // Add parameters example if missing
        if (operation.parameters) {
          operation.parameters.forEach(param => {
            if (!param.description) {
              param.description = `The ${param.name} parameter`;
            }
            if (!param.example && param.schema) {
              if (param.schema.type === 'string') {
                param.example = `example-${param.name}`;
              } else if (param.schema.type === 'integer' || param.schema.type === 'number') {
                param.example = 1;
              } else if (param.schema.type === 'boolean') {
                param.example = true;
              }
            }
          });
        }
        
        // Add request body example if missing
        if (operation.requestBody && operation.requestBody.content) {
          Object.values(operation.requestBody.content).forEach(content => {
            if (content.schema && !content.example) {
              const schemaName = content.schema.$ref ? 
                content.schema.$ref.split('/').pop() : 
                'Request';
              
              if (examples[`${operation.operationId}Request`]) {
                content.example = examples[`${operation.operationId}Request`].value;
              } else if (examples[`${method}${schemaName}`]) {
                content.example = examples[`${method}${schemaName}`].value;
              } else if (examples[`${schemaName}Request`]) {
                content.example = examples[`${schemaName}Request`].value;
              }
            }
          });
        }
        
        // Add response examples if missing
        if (operation.responses) {
          Object.entries(operation.responses).forEach(([statusCode, response]) => {
            if (response.content) {
              Object.values(response.content).forEach(content => {
                if (content.schema && !content.example) {
                  const schemaName = content.schema.$ref ? 
                    content.schema.$ref.split('/').pop() : 
                    'Response';
                  
                  if (examples[`${operation.operationId}Response`]) {
                    content.example = examples[`${operation.operationId}Response`].value;
                  } else if (examples[`${method}${schemaName}`]) {
                    content.example = examples[`${method}${schemaName}`].value;
                  } else if (examples[`${schemaName}Response`]) {
                    content.example = examples[`${schemaName}Response`].value;
                  } else if (statusCode.startsWith('4') && examples.errorBadRequest) {
                    content.example = examples.errorBadRequest.value;
                  } else if (statusCode === '401' && examples.errorUnauthorized) {
                    content.example = examples.errorUnauthorized.value;
                  } else if (statusCode === '404' && examples.errorNotFound) {
                    content.example = examples.errorNotFound.value;
                  }
                }
              });
            }
          });
        }
      }
    });
  });
  
  return paths;
};

// Main function to enhance the OpenAPI spec
const enhanceOpenAPI = async () => {
  try {
    console.log('Enhancing OpenAPI specification...');
    
    // Load the root spec
    const rootSpec = loadYaml(path.join(SPEC_DIR, 'index.yaml'));
    
    // Enhance components
    if (rootSpec.components) {
      console.log('Enhancing components...');
      rootSpec.components = enhanceComponents(rootSpec.components);
    }
    
    // Enhance paths
    if (rootSpec.paths) {
      console.log('Enhancing paths...');
      rootSpec.paths = enhancePaths(rootSpec.paths);
    }
    
    // Save the enhanced spec
    saveYaml(OUTPUT_FILE, rootSpec);
    
    console.log('OpenAPI specification enhanced successfully!');
    
    // Validate the enhanced spec
    console.log('\nValidating enhanced OpenAPI specification...');
    try {
      execSync('npx @redocly/cli lint openapi.yaml', { 
        cwd: ROOT_DIR, 
        stdio: 'inherit' 
      });
      console.log('\n✅ OpenAPI specification is valid!');
    } catch (error) {
      console.error('\n❌ OpenAPI specification validation failed:', error.message);
      process.exit(1);
    }
    
  } catch (error) {
    console.error('Error enhancing OpenAPI specification:', error);
    process.exit(1);
  }
};

// Run the enhancement
enhanceOpenAPI();
