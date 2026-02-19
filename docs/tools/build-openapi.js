#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');
const { Command } = require('commander');

// Initialize commander
const program = new Command();
program
  .name('build-openapi')
  .description('Build the OpenAPI specification from fragments')
  .version('1.0.0')
  .option('-o, --output <file>', 'Output file', 'openapi.yaml')
  .option('-v, --verbose', 'Verbose output', false)
  .parse(process.argv);

const options = program.opts();

// Paths
const ROOT_DIR = path.join(__dirname, '..');
const SPEC_DIR = path.join(ROOT_DIR, 'spec');
const OUTPUT_FILE = path.join(ROOT_DIR, options.output);

// Logging helper
function log(message, level = 'info') {
  if (!options.verbose && level === 'debug') return;
  const prefix = level === 'error' ? '❌ ' : level === 'success' ? '✅ ' : 'ℹ️ ';
  console.log(`${prefix} ${message}`);
}

// Load and parse YAML file
function loadYaml(filePath) {
  try {
    const content = fs.readFileSync(filePath, 'utf8');
    return yaml.load(content);
  } catch (e) {
    log(`Error loading YAML file ${filePath}: ${e.message}`, 'error');
    process.exit(1);
  }
}

// Merge objects deeply
function deepMerge(target, source) {
  const output = { ...target };
  
  if (isObject(target) && isObject(source)) {
    Object.keys(source).forEach(key => {
      if (isObject(source[key])) {
        if (!(key in target)) {
          Object.assign(output, { [key]: source[key] });
        } else {
          output[key] = deepMerge(target[key], source[key]);
        }
      } else if (Array.isArray(source[key])) {
        // For arrays, we might want to merge them based on some ID in the future
        // For now, we'll just concatenate them
        output[key] = [...(target[key] || []), ...source[key]];
      } else {
        Object.assign(output, { [key]: source[key] });
      }
    });
  }
  
  return output;
}

function isObject(item) {
  return item && typeof item === 'object' && !Array.isArray(item);
}

// Main function
async function buildOpenAPI() {
  log('Starting OpenAPI specification build...');
  
  try {
    // Load the main index file
    const indexFile = path.join(SPEC_DIR, 'index.yaml');
    if (!fs.existsSync(indexFile)) {
      throw new Error(`Index file not found at ${indexFile}`);
    }
    
    log(`Loading index file: ${indexFile}`, 'debug');
    let spec = loadYaml(indexFile);
    
    // Process path references
    if (spec.paths && typeof spec.paths === 'object') {
      log('Processing path references...', 'debug');
      const paths = {};
      
      for (const [pathKey, pathItem] of Object.entries(spec.paths)) {
        if (pathItem && typeof pathItem === 'object' && pathItem.$ref) {
          // Handle $ref in path items
          const refParts = pathItem.$ref.split('#');
          const refPath = path.join(SPEC_DIR, refParts[0]);
          const refPointer = refParts[1] || '';
          
          log(`  - Loading path ${pathKey} from: ${refPath}${refPointer ? '#' + refPointer : ''}`, 'debug');
          
          try {
            const refContent = loadYaml(refPath);
            // Extract the referenced content using the JSON Pointer
            const pointerParts = refPointer.split('/').filter(Boolean);
            let content = refContent;
            
            for (const part of pointerParts) {
              if (content && content[part] !== undefined) {
                content = content[part];
              } else {
                throw new Error(`Invalid reference: ${part} not found`);
              }
            }
            
            paths[pathKey] = content;
          } catch (error) {
            log(`  ⚠️  Warning: Could not load path ${pathKey}: ${error.message}`, 'debug');
          }
        } else {
          // Direct path definition
          paths[pathKey] = pathItem;
        }
      }
      
      spec.paths = paths;
    }
    
    // Process component references
    if (spec.components && typeof spec.components === 'object') {
      log('Processing component references...', 'debug');
      const components = {};
      
      for (const [componentType, ref] of Object.entries(spec.components)) {
        if (ref && typeof ref === 'object' && ref.$ref) {
          // Handle $ref in component types (schemas, securitySchemes, etc.)
          const refParts = ref.$ref.split('#');
          const refPath = path.join(SPEC_DIR, refParts[0]);
          const refPointer = refParts[1] || '';
          
          log(`  - Loading ${componentType} from: ${refPath}${refPointer ? '#' + refPointer : ''}`, 'debug');
          
          try {
            const refContent = loadYaml(refPath);
            
            // If no pointer is provided, use the entire document
            if (!refPointer) {
              components[componentType] = refContent;
            } else {
              // Extract the referenced content using the JSON Pointer
              const pointerParts = refPointer.split('/').filter(Boolean);
              let content = refContent;
              
              for (const part of pointerParts) {
                if (content && content[part] !== undefined) {
                  content = content[part];
                } else {
                  throw new Error(`Invalid reference: ${part} not found`);
                }
              }
              
              components[componentType] = content;
            }
          } catch (error) {
            log(`  ⚠️  Warning: Could not load ${componentType}: ${error.message}`, 'debug');
          }
        } else if (ref && typeof ref === 'object') {
          // Direct component definition
          components[componentType] = ref;
        }
      }
      
      // Only replace components if we have valid ones
      if (Object.keys(components).length > 0) {
        spec.components = components;
      } else {
        delete spec.components;
      }
    }
    
    // Process services
    if (spec.services && Array.isArray(spec.services)) {
      log('Processing services...', 'debug');
      
      for (const serviceRef of spec.services) {
        if (typeof serviceRef === 'object' && serviceRef.$ref) {
          const servicePath = path.join(SPEC_DIR, serviceRef.$ref);
          log(`  - Loading service: ${serviceRef.$ref}`, 'debug');
          const serviceContent = loadYaml(servicePath);
          
          // Merge paths
          if (serviceContent.paths) {
            spec.paths = { ...(spec.paths || {}), ...serviceContent.paths };
          }
          
          // Merge components
          if (serviceContent.components) {
            spec.components = deepMerge(spec.components || {}, serviceContent.components);
          }
        }
      }
      
      // Remove the services key as it's not part of the OpenAPI spec
      delete spec.services;
    }
    
    // Clean up any remaining $ref placeholders
    const specString = JSON.stringify(spec);
    const cleanedSpec = JSON.parse(
      specString.replace(/\{\{\s*\$ref:\s*'[^']+'\s*\}\}/g, '{}')
    );
    
    // Write the final OpenAPI spec
    const outputYaml = yaml.dump(cleanedSpec, {
      noRefs: true,
      lineWidth: -1, // No line wrapping
    });
    
    fs.writeFileSync(OUTPUT_FILE, outputYaml, 'utf8');
    
    log(`✅ OpenAPI specification built successfully: ${OUTPUT_FILE}`, 'success');
    log(`   - Paths: ${Object.keys(cleanedSpec.paths || {}).length}`);
    log(`   - Schemas: ${Object.keys(cleanedSpec.components?.schemas || {}).length}`);
    log(`   - Security Schemes: ${Object.keys(cleanedSpec.components?.securitySchemes || {}).length}`);
    
  } catch (error) {
    log(`Failed to build OpenAPI specification: ${error.message}`, 'error');
    process.exit(1);
  }
}

// Export for testing
module.exports = { buildOpenAPI, deepMerge, isObject };

// Run the build only when executed directly
if (require.main === module) {
  buildOpenAPI();
}
