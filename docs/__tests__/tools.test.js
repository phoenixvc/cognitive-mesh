const path = require('path');
const fs = require('fs');
const { buildOpenAPI } = require('../tools/build-openapi');
const { enhanceOpenAPISpec } = require('../tools/enhance-openapi');

describe('Build Tools', () => {
  describe('build-openapi.js', () => {
    it('should export a buildOpenAPI function', () => {
      expect(typeof buildOpenAPI).toBe('function');
    });

    it('should generate a valid OpenAPI spec', async () => {
      const outputPath = path.join(__dirname, '..', 'openapi.yaml');
      await buildOpenAPI();
      
      // Verify the file was created
      expect(fs.existsSync(outputPath)).toBe(true);
      
      // Read the generated file
      const content = fs.readFileSync(outputPath, 'utf8');
      expect(content).toContain('openapi: 3.0.3');
      expect(content).toContain('title: Cognitive Mesh API');
    });
  });

  describe('enhance-openapi.js', () => {
    it('should export an enhanceOpenAPISpec function', () => {
      expect(typeof enhanceOpenAPISpec).toBe('function');
    });

    it('should enhance an OpenAPI spec with examples', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {}
      };
      
      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced).toBeDefined();
      expect(enhanced.openapi).toBe('3.0.3');
      expect(enhanced.info.title).toBe('Test API');
    });
  });
});
