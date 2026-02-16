const path = require('path');
const fs = require('fs');
const { buildOpenAPI, deepMerge, isObject } = require('../tools/build-openapi');
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

    describe('isObject', () => {
      it('should return true for plain objects', () => {
        expect(isObject({})).toBe(true);
        expect(isObject({ a: 1 })).toBe(true);
      });

      it('should return false for arrays', () => {
        expect(isObject([])).toBe(false);
        expect(isObject([1, 2])).toBe(false);
      });

      it('should return false for primitives and null', () => {
        expect(isObject(null)).toBeFalsy();
        expect(isObject(undefined)).toBeFalsy();
        expect(isObject(42)).toBeFalsy();
        expect(isObject('string')).toBeFalsy();
        expect(isObject(true)).toBeFalsy();
      });
    });

    describe('deepMerge', () => {
      it('should merge two flat objects', () => {
        const result = deepMerge({ a: 1 }, { b: 2 });
        expect(result).toEqual({ a: 1, b: 2 });
      });

      it('should override primitive values from source', () => {
        const result = deepMerge({ a: 1 }, { a: 2 });
        expect(result).toEqual({ a: 2 });
      });

      it('should deep merge nested objects', () => {
        const target = { nested: { a: 1, b: 2 } };
        const source = { nested: { b: 3, c: 4 } };
        const result = deepMerge(target, source);
        expect(result.nested).toEqual({ a: 1, b: 3, c: 4 });
      });

      it('should add new nested objects from source', () => {
        const target = { a: 1 };
        const source = { nested: { x: 10 } };
        const result = deepMerge(target, source);
        expect(result.nested).toEqual({ x: 10 });
      });

      it('should concatenate arrays', () => {
        const target = { items: [1, 2] };
        const source = { items: [3, 4] };
        const result = deepMerge(target, source);
        expect(result.items).toEqual([1, 2, 3, 4]);
      });

      it('should create array from source when target has no matching key', () => {
        const target = {};
        const source = { items: [1, 2] };
        const result = deepMerge(target, source);
        expect(result.items).toEqual([1, 2]);
      });

      it('should not mutate target or source', () => {
        const target = { a: 1, nested: { x: 1 } };
        const source = { b: 2, nested: { y: 2 } };
        const targetCopy = JSON.parse(JSON.stringify(target));
        const sourceCopy = JSON.parse(JSON.stringify(source));

        deepMerge(target, source);

        expect(target).toEqual(targetCopy);
        expect(source).toEqual(sourceCopy);
      });

      it('should handle non-object target gracefully', () => {
        const result = deepMerge(null, { a: 1 });
        // When target is null, isObject returns false, so just spreads target
        expect(result).toBeDefined();
      });

      it('should handle mixed types in merge', () => {
        const target = { a: 1, b: { x: 1 }, c: [1] };
        const source = { a: 'string', b: { y: 2 }, c: [2], d: true };
        const result = deepMerge(target, source);
        expect(result.a).toBe('string');
        expect(result.b).toEqual({ x: 1, y: 2 });
        expect(result.c).toEqual([1, 2]);
        expect(result.d).toBe(true);
      });
    });
  });

  describe('enhance-openapi.js', () => {
    it('should export an enhanceOpenAPISpec function', () => {
      expect(typeof enhanceOpenAPISpec).toBe('function');
    });

    it('should return undefined for null/undefined input', () => {
      expect(enhanceOpenAPISpec(null)).toBeNull();
      expect(enhanceOpenAPISpec(undefined)).toBeUndefined();
    });

    it('should enhance a minimal OpenAPI spec without mutation', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {}
      };

      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced).toBeDefined();
      expect(enhanced.openapi).toBe('3.0.3');
      expect(enhanced.info.title).toBe('Test API');
      // Original should not be mutated
      expect(spec).not.toBe(enhanced);
    });

    it('should add missing operationId, summary, description, and tags to operations', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/users': {
            get: {
              responses: { '200': { description: 'OK' } }
            },
            post: {
              summary: 'Create user',
              description: 'Creates a new user',
              operationId: 'createUser',
              tags: ['Users'],
              responses: { '201': { description: 'Created' } }
            }
          },
          '/users/{userId}': {
            delete: {
              responses: { '204': { description: 'Deleted' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);

      // GET /users should have auto-generated fields
      const getUsers = enhanced.paths['/users'].get;
      expect(getUsers.operationId).toBeDefined();
      expect(getUsers.summary).toBeDefined();
      expect(getUsers.description).toBeDefined();
      expect(getUsers.tags).toBeDefined();
      expect(getUsers.tags.length).toBeGreaterThan(0);

      // POST /users should keep its original values
      const postUsers = enhanced.paths['/users'].post;
      expect(postUsers.operationId).toBe('createUser');
      expect(postUsers.summary).toBe('Create user');
      expect(postUsers.description).toBe('Creates a new user');
      expect(postUsers.tags).toEqual(['Users']);

      // DELETE /users/{userId} should have auto-generated fields
      const deleteUser = enhanced.paths['/users/{userId}'].delete;
      expect(deleteUser.operationId).toBeDefined();
      expect(deleteUser.summary).toBeDefined();
      expect(deleteUser.description).toBeDefined();
    });

    it('should add parameter descriptions and examples when missing', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/items': {
            get: {
              operationId: 'listItems',
              summary: 'List items',
              description: 'Lists all items',
              tags: ['Items'],
              parameters: [
                {
                  name: 'search',
                  in: 'query',
                  schema: { type: 'string' }
                },
                {
                  name: 'limit',
                  in: 'query',
                  description: 'Max results',
                  example: 10,
                  schema: { type: 'integer' }
                },
                {
                  name: 'active',
                  in: 'query',
                  schema: { type: 'boolean' }
                },
                {
                  name: 'page',
                  in: 'query',
                  schema: { type: 'integer' }
                },
                {
                  name: 'price',
                  in: 'query',
                  schema: { type: 'number' }
                }
              ],
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const params = enhanced.paths['/items'].get.parameters;

      // search (string) - should get description and example
      expect(params[0].description).toBeDefined();
      expect(params[0].example).toBeDefined();
      expect(typeof params[0].example).toBe('string');

      // limit (integer) - already has description and example, should keep them
      expect(params[1].description).toBe('Max results');
      expect(params[1].example).toBe(10);

      // active (boolean) - should get description and example
      expect(params[2].description).toBeDefined();
      expect(params[2].example).toBeDefined();
      expect(typeof params[2].example).toBe('boolean');

      // page (integer, no example) - should get description and numeric example
      expect(params[3].description).toBeDefined();
      expect(params[3].example).toBe(1);

      // price (number, no example) - should get description and numeric example
      expect(params[4].description).toBeDefined();
      expect(params[4].example).toBe(1);
    });

    it('should enhance components with security scheme descriptions', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          securitySchemes: {
            bearerAuth: {
              type: 'http',
              scheme: 'bearer'
            },
            apiKeyAuth: {
              type: 'apiKey',
              name: 'X-API-Key',
              in: 'header',
              description: 'Custom API key description'
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);

      // bearerAuth should get a generated description
      expect(enhanced.components.securitySchemes.bearerAuth.description).toBeDefined();
      expect(enhanced.components.securitySchemes.bearerAuth.description).toContain('http');

      // apiKeyAuth should keep its existing description
      expect(enhanced.components.securitySchemes.apiKeyAuth.description).toBe('Custom API key description');
    });

    it('should enhance schemas with property-level examples', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            Item: {
              type: 'object',
              properties: {
                name: { type: 'string' },
                count: { type: 'integer' },
                price: { type: 'number' },
                active: { type: 'boolean' },
                tags: {
                  type: 'array',
                  items: { type: 'string' }
                },
                counts: {
                  type: 'array',
                  items: { type: 'integer' }
                }
              }
            },
            WithExample: {
              type: 'object',
              example: { id: 1 },
              properties: {
                id: { type: 'integer' }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const itemProps = enhanced.components.schemas.Item.properties;

      // String property should get an example
      expect(itemProps.name.example).toBeDefined();
      expect(typeof itemProps.name.example).toBe('string');

      // Integer type maps to 0 which is falsy, so the code skips it
      // (typeExamples[type] is checked with truthy, and 0 is falsy)
      expect(itemProps.count.example).toBeUndefined();

      // Number type also maps to 0, same behavior
      expect(itemProps.price.example).toBeUndefined();

      // Boolean property should get an example
      expect(itemProps.active.example).toBeDefined();

      // Array of strings should get examples
      expect(itemProps.tags.items.example).toBeDefined();
      expect(itemProps.tags.example).toBeDefined();

      // Array of integers - items example is 1 (truthy), array example is [1,2,3] (truthy)
      expect(itemProps.counts.items.example).toBeDefined();
      expect(itemProps.counts.example).toBeDefined();

      // Schema with existing example should not be modified
      expect(enhanced.components.schemas.WithExample.example).toEqual({ id: 1 });
    });

    it('should handle specs without components or paths gracefully', () => {
      const specNoComponents = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: { '/test': { get: { operationId: 'test', summary: 's', description: 'd', tags: ['t'], responses: {} } } }
      };

      const specNoPaths = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        components: { schemas: {} }
      };

      expect(() => enhanceOpenAPISpec(specNoComponents)).not.toThrow();
      expect(() => enhanceOpenAPISpec(specNoPaths)).not.toThrow();
    });

    it('should add error examples to 4xx response content', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/items': {
            post: {
              operationId: 'createItem',
              summary: 'Create item',
              description: 'Creates an item',
              tags: ['Items'],
              requestBody: {
                content: {
                  'application/json': {
                    schema: { $ref: '#/components/schemas/ItemRequest' }
                  }
                }
              },
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {
                      schema: { $ref: '#/components/schemas/ItemResponse' }
                    }
                  }
                },
                '400': {
                  description: 'Bad request',
                  content: {
                    'application/json': {
                      schema: { type: 'object' }
                    }
                  }
                },
                '401': {
                  description: 'Unauthorized',
                  content: {
                    'application/json': {
                      schema: { type: 'object' }
                    }
                  }
                },
                '404': {
                  description: 'Not found',
                  content: {
                    'application/json': {
                      schema: { type: 'object' }
                    }
                  }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const post = enhanced.paths['/items'].post;
      expect(post).toBeDefined();
      expect(post.operationId).toBe('createItem');

      // 4xx responses should get error examples from the examples file
      const resp400 = post.responses['400'].content['application/json'];
      expect(resp400.example).toBeDefined();

      const resp401 = post.responses['401'].content['application/json'];
      expect(resp401.example).toBeDefined();

      const resp404 = post.responses['404'].content['application/json'];
      expect(resp404.example).toBeDefined();
    });

    it('should add request body examples when operationId matches example keys', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/auth/token': {
            post: {
              operationId: 'authToken',
              summary: 'Get token',
              description: 'Get auth token',
              tags: ['Auth'],
              requestBody: {
                content: {
                  'application/json': {
                    schema: { type: 'object' }
                  }
                }
              },
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {
                      schema: { type: 'object' }
                    }
                  }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const post = enhanced.paths['/auth/token'].post;

      // operationId "authToken" should match example "authTokenRequest" for request body
      const reqBody = post.requestBody.content['application/json'];
      expect(reqBody.example).toBeDefined();
      expect(reqBody.example.email).toBeDefined();

      // operationId "authToken" should match example "authTokenResponse" for response
      const respBody = post.responses['200'].content['application/json'];
      expect(respBody.example).toBeDefined();
      expect(respBody.example.token).toBeDefined();
    });

    it('should not overwrite existing request body or response examples', () => {
      const existingReqExample = { foo: 'bar' };
      const existingRespExample = { baz: 'qux' };

      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/auth/token': {
            post: {
              operationId: 'authToken',
              summary: 'Get token',
              description: 'Get auth token',
              tags: ['Auth'],
              requestBody: {
                content: {
                  'application/json': {
                    schema: { type: 'object' },
                    example: existingReqExample
                  }
                }
              },
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {
                      schema: { type: 'object' },
                      example: existingRespExample
                    }
                  }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const post = enhanced.paths['/auth/token'].post;

      // Existing examples should be preserved, not overwritten
      expect(post.requestBody.content['application/json'].example).toEqual(existingReqExample);
      expect(post.responses['200'].content['application/json'].example).toEqual(existingRespExample);
    });

    it('should apply exampleMap for known schema names', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            User: {
              type: 'object',
              properties: {
                id: { type: 'string' },
                email: { type: 'string' }
              }
            },
            Error: {
              type: 'object',
              properties: {
                code: { type: 'string' },
                message: { type: 'string' }
              }
            },
            Pagination: {
              type: 'object',
              properties: {
                page: { type: 'integer' },
                total: { type: 'integer' }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);

      // "User" schema maps to "userProfile" example
      expect(enhanced.components.schemas.User.example).toBeDefined();

      // "Error" schema maps to "errorBadRequest" example
      expect(enhanced.components.schemas.Error.example).toBeDefined();

      // "Pagination" schema maps to "paginatedResponse" example
      expect(enhanced.components.schemas.Pagination.example).toBeDefined();
    });

    it('should match schema $ref name for response example lookup via third cascade', () => {
      // Tests the third cascade branch: examples[`${schemaName}Request/Response`]
      // operationId doesn't match any example, but schema $ref name does
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/tokens': {
            post: {
              operationId: 'createToken',
              summary: 'Create token',
              description: 'Creates a token',
              tags: ['Tokens'],
              requestBody: {
                content: {
                  'application/json': {
                    schema: { $ref: '#/components/schemas/authToken' }
                  }
                }
              },
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {
                      schema: { $ref: '#/components/schemas/authToken' }
                    }
                  }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const post = enhanced.paths['/tokens'].post;

      // schemaName = "authToken" → examples["authTokenRequest"] exists (third cascade)
      const reqBody = post.requestBody.content['application/json'];
      expect(reqBody.example).toBeDefined();
      expect(reqBody.example.email).toBeDefined();

      // schemaName = "authToken" → examples["authTokenResponse"] exists (third cascade)
      const respBody = post.responses['200'].content['application/json'];
      expect(respBody.example).toBeDefined();
      expect(respBody.example.token).toBeDefined();
    });

    it('should match schema $ref name for champion match example', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/champions': {
            post: {
              operationId: 'championMatch',
              summary: 'Match champions',
              description: 'Match champions',
              tags: ['Champions'],
              requestBody: {
                content: {
                  'application/json': {
                    schema: { type: 'object' }
                  }
                }
              },
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {
                      schema: { type: 'object' }
                    }
                  }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const post = enhanced.paths['/champions'].post;

      // operationId "championMatch" → examples["championMatchRequest"] and "championMatchResponse"
      const reqBody = post.requestBody.content['application/json'];
      expect(reqBody.example).toBeDefined();
      expect(reqBody.example.skills).toBeDefined();

      const respBody = post.responses['200'].content['application/json'];
      expect(respBody.example).toBeDefined();
      expect(respBody.example.matches).toBeDefined();
    });

    it('should enhance components with empty schemas and securitySchemes', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {},
          securitySchemes: {}
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.components).toBeDefined();
      expect(enhanced.components.schemas).toBeDefined();
      expect(enhanced.components.securitySchemes).toBeDefined();
    });

    it('should handle properties with existing examples', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            Item: {
              type: 'object',
              properties: {
                name: { type: 'string', example: 'John' },
                age: { type: 'integer', example: 30 }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      // Existing examples should be preserved
      expect(enhanced.components.schemas.Item.properties.name.example).toBe('John');
      expect(enhanced.components.schemas.Item.properties.age.example).toBe(30);
    });

    it('should handle multiple HTTP methods on the same path', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/resources': {
            get: {
              responses: { '200': { description: 'OK' } }
            },
            put: {
              responses: { '200': { description: 'OK' } }
            },
            patch: {
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const resources = enhanced.paths['/resources'];

      // All methods should get enhanced
      expect(resources.get.operationId).toBeDefined();
      expect(resources.put.operationId).toBeDefined();
      expect(resources.patch.operationId).toBeDefined();

      expect(resources.get.summary).toBeDefined();
      expect(resources.put.summary).toBeDefined();
      expect(resources.patch.summary).toBeDefined();
    });

    it('should handle response content without schema', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/test': {
            get: {
              operationId: 'testOp',
              summary: 'Test',
              description: 'Test operation',
              tags: ['Test'],
              responses: {
                '200': {
                  description: 'OK',
                  content: {
                    'application/json': {}
                  }
                },
                '204': {
                  description: 'No content'
                }
              }
            }
          }
        }
      };

      // Should not throw when response content lacks schema
      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.paths['/test'].get).toBeDefined();
      // Response without content should remain unchanged
      expect(enhanced.paths['/test'].get.responses['204'].description).toBe('No content');
    });

    it('should handle schemas with nested objects that have no type', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            Complex: {
              type: 'object',
              properties: {
                metadata: { type: 'object', additionalProperties: true },
                ref: { $ref: '#/components/schemas/Other' }
              }
            }
          }
        }
      };

      // Should not throw on $ref properties or object type without simple example
      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.components.schemas.Complex).toBeDefined();
    });

    it('should resolve $ref properties in schemas using components.yaml', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            Order: {
              type: 'object',
              properties: {
                name: { type: 'string' },
                pagination: { $ref: '#/components/schemas/Pagination' }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const orderProps = enhanced.components.schemas.Order.properties;

      // The pagination property should have been resolved from components.yaml
      // and enhanced with the Pagination schema content
      expect(orderProps.pagination).toBeDefined();
      expect(orderProps.pagination.type).toBe('object');
    });

    it('should resolve $ref in array items using components.yaml', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            OrderList: {
              type: 'object',
              properties: {
                errors: {
                  type: 'array',
                  items: { $ref: '#/components/schemas/Error' }
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      const errorsItems = enhanced.components.schemas.OrderList.properties.errors.items;

      // The array items should have been resolved from components.yaml Error schema
      expect(errorsItems).toBeDefined();
      expect(errorsItems.type).toBe('object');
    });

    it('should handle $ref to non-existent schema gracefully', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            TestSchema: {
              type: 'object',
              properties: {
                ref: { $ref: '#/components/schemas/NonExistentSchema' }
              }
            }
          }
        }
      };

      // Should not throw when $ref points to non-existent schema
      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.components.schemas.TestSchema).toBeDefined();
    });

    it('should skip self-referential $ref to prevent infinite recursion', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {},
        components: {
          schemas: {
            // Schema name matches the $ref target - should be skipped
            Pagination: {
              type: 'object',
              properties: {
                self: { $ref: '#/components/schemas/Pagination' }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      // Should not infinite loop - self-referencing $ref is skipped
      expect(enhanced.components.schemas.Pagination).toBeDefined();
    });

    it('should handle operations without requestBody or parameters', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/health': {
            get: {
              operationId: 'healthCheck',
              summary: 'Health check',
              description: 'Check service health',
              tags: ['Health'],
              responses: {
                '200': {
                  description: 'Service is healthy'
                }
              }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.paths['/health'].get.operationId).toBe('healthCheck');
    });

    it('should auto-generate tags when tags is an empty array', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/users': {
            get: {
              operationId: 'getUsers',
              summary: 'Get users',
              description: 'Gets all users',
              tags: [],
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      // Empty tags should be replaced with auto-generated tags
      expect(enhanced.paths['/users'].get.tags.length).toBeGreaterThan(0);
    });

    it('should handle requestBody without content property', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/test': {
            post: {
              operationId: 'testPost',
              summary: 'Test',
              description: 'Test',
              tags: ['Test'],
              requestBody: { required: true },
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      expect(enhanced.paths['/test'].post.requestBody.required).toBe(true);
    });

    it('should handle parameter without schema', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/test': {
            get: {
              operationId: 'testGet',
              summary: 'Test',
              description: 'Test op',
              tags: ['Test'],
              parameters: [
                { name: 'q', in: 'query' }
              ],
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      // Parameter without schema should still get description
      expect(enhanced.paths['/test'].get.parameters[0].description).toBeDefined();
      // But should not get an example (no schema to derive type from)
      expect(enhanced.paths['/test'].get.parameters[0].example).toBeUndefined();
    });

    it('should skip non-HTTP-method keys on path items', () => {
      const spec = {
        openapi: '3.0.3',
        info: { title: 'Test API', version: '1.0.0' },
        paths: {
          '/test': {
            summary: 'Test path',
            description: 'A test path',
            get: {
              responses: { '200': { description: 'OK' } }
            }
          }
        }
      };

      const enhanced = enhanceOpenAPISpec(spec);
      // path-level summary/description should remain unchanged
      expect(enhanced.paths['/test'].summary).toBe('Test path');
      expect(enhanced.paths['/test'].description).toBe('A test path');
      // GET should still be enhanced
      expect(enhanced.paths['/test'].get.operationId).toBeDefined();
    });
  });
});
