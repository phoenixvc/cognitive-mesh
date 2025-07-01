import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:3000/v1';
const AUTH_TOKEN = __ENV.AUTH_TOKEN || 'your-test-token';

// Custom metrics
const errorRate = new Rate('errors');
const successRate = new Rate('successes');

// Payload generators
function generateLargePayload(sizeKB) {
  const baseObj = {
    name: 'Test Item',
    description: 'A test item with a large payload',
    items: []
  };
  
  // Calculate how many items we need to reach the desired size
  const itemSize = 100; // bytes per item
  const itemsNeeded = Math.ceil((sizeKB * 1024) / itemSize);
  
  for (let i = 0; i < itemsNeeded; i++) {
    baseObj.items.push({
      id: `item-${i}`,
      value: `Value ${i}`,
      timestamp: new Date().toISOString(),
      metadata: {
        count: i,
        active: i % 2 === 0,
        tags: ['tag1', 'tag2', 'tag3']
      }
    });
  }
  
  return JSON.stringify(baseObj);
}

// Test scenarios
export const options = {
  scenarios: {
    small_payload: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '30s', target: 10 },  // Ramp up to 10 VUs
        { duration: '1m', target: 10 },    // Stay at 10 VUs
        { duration: '30s', target: 0 },    // Ramp down
      ],
      gracefulRampDown: '30s',
      exec: 'testSmallPayload',
    },
    medium_payload: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '30s', target: 5 },
        { duration: '1m', target: 5 },
        { duration: '30s', target: 0 },
      ],
      gracefulRampDown: '30s',
      exec: 'testMediumPayload',
    },
    large_payload: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '30s', target: 3 },
        { duration: '1m', target: 3 },
        { duration: '30s', target: 0 },
      ],
      gracefulRampDown: '30s',
      exec: 'testLargePayload',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.1'], // <10% errors
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    errors: ['rate<0.1'], // <10% errors
  },
};

// Headers for all requests
const headers = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${AUTH_TOKEN}`,
};

// Test functions
export function testSmallPayload() {
  const payload = generateLargePayload(10); // 10KB
  testEndpointWithPayload('/test/endpoint', payload, 'small');
}

export function testMediumPayload() {
  const payload = generateLargePayload(100); // 100KB
  testEndpointWithPayload('/test/endpoint', payload, 'medium');
}

export function testLargePayload() {
  const payload = generateLargePayload(1024); // 1MB
  testEndpointWithPayload('/test/endpoint', payload, 'large');
}

// Helper function to test an endpoint with a payload
function testEndpointWithPayload(endpoint, payload, payloadType) {
  const url = `${BASE_URL}${endpoint}`;
  const params = { headers };
  
  // Add tags for better metrics
  const tags = {
    endpoint,
    payload_type: payloadType,
    payload_size: `${payload.length / 1024} KB`,
  };
  
  // Make the request
  const res = http.post(url, payload, { ...params, tags });
  
  // Check response
  const success = check(res, {
    [`${payloadType} payload status is 200`]: (r) => r.status === 200,
    [`${payloadType} payload has response time < 2s`]: (r) => r.timings.duration < 2000,
  });
  
  // Update metrics
  if (success) {
    successRate.add(1, tags);
  } else {
    errorRate.add(1, tags);
    console.error(`Error with ${payloadType} payload (${payload.length} bytes):`, res.body);
  }
  
  // Add a small delay between requests
  sleep(1);
}

// Setup function that runs once before the test
export function setup() {
  console.log(`Starting performance tests against ${BASE_URL}`);
  console.log('Payload sizes:');
  console.log(`- Small: ${generateLargePayload(10).length / 1024} KB`);
  console.log(`- Medium: ${generateLargePayload(100).length / 1024} KB`);
  console.log(`- Large: ${generateLargePayload(1024).length / 1024} KB`);
  
  return {
    startTime: new Date().toISOString(),
  };
}

// Teardown function that runs after the test
export function teardown(data) {
  console.log(`\nTest completed at ${new Date().toISOString()}`);
  console.log(`Test started at ${data.startTime}`);
  console.log('Performance test completed');
}
