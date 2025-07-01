import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { SharedArray } from 'k6/data';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:3000/v1';
const AUTH_TOKEN = __ENV.AUTH_TOKEN || 'your-test-token';

// Custom metrics
const errorRate = new Rate('errors');
const successRate = new Rate('successes');

// Test data - using SharedArray for memory efficiency with large datasets
const testUsers = new SharedArray('test_users', function () {
  return JSON.parse(open('./test-data/users.json'));
});

// Test scenarios
export const options = {
  scenarios: {
    low_concurrency: {
      executor: 'constant-vus',
      vus: 10,  // 10 concurrent users
      duration: '2m',
      exec: 'testConcurrentRequests',
      tags: { scenario: 'low_concurrency' },
    },
    medium_concurrency: {
      executor: 'ramping-vus',
      startVUs: 10,
      stages: [
        { duration: '30s', target: 50 },  // Ramp up to 50 VUs
        { duration: '1m', target: 50 },   // Stay at 50 VUs
        { duration: '30s', target: 0 },   // Ramp down
      ],
      gracefulRampDown: '30s',
      exec: 'testConcurrentRequests',
      tags: { scenario: 'medium_concurrency' },
      startTime: '1m', // Start after low_concurrency
    },
    high_concurrency: {
      executor: 'ramping-vus',
      startVUs: 10,
      stages: [
        { duration: '30s', target: 100 },  // Ramp up to 100 VUs
        { duration: '1m', target: 100 },   // Stay at 100 VUs
        { duration: '30s', target: 0 },    // Ramp down
      ],
      gracefulRampDown: '30s',
      exec: 'testConcurrentRequests',
      tags: { scenario: 'high_concurrency' },
      startTime: '4m', // Start after medium_concurrency
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.1'], // <10% errors
    http_req_duration: ['p(95)<1000'], // 95% of requests should be below 1s
    errors: ['rate<0.1'], // <10% errors
  },
};

// Headers for all requests
const headers = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${AUTH_TOKEN}`,
};

// Test function for concurrent requests
export function testConcurrentRequests() {
  // Get a random user from the test data
  const user = testUsers[Math.floor(Math.random() * testUsers.length)];
  
  // Define the endpoints to test
  const endpoints = [
    { method: 'GET', path: `/users/${user.id}` },
    { method: 'GET', path: '/users/me' },
    { method: 'GET', path: '/resources' },
    { method: 'POST', path: '/search', body: JSON.stringify({ query: 'test' }) },
  ];
  
  // Select a random endpoint
  const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
  
  // Make the request
  const url = `${BASE_URL}${endpoint.path}`;
  const params = { 
    headers,
    tags: { endpoint: endpoint.path },
  };
  
  let res;
  if (endpoint.method === 'GET') {
    res = http.get(url, params);
  } else if (endpoint.method === 'POST') {
    res = http.post(url, endpoint.body, params);
  }
  
  // Check response
  const success = check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 2s': (r) => r.timings.duration < 2000,
  });
  
  // Update metrics
  if (success) {
    successRate.add(1, { endpoint: endpoint.path });
  } else {
    errorRate.add(1, { 
      endpoint: endpoint.path,
      status: res.status,
      error: res.error || 'Unknown error',
    });
  }
  
  // Add a small random delay between requests
  sleep(Math.random() * 2);
}

// Setup function to prepare test data
export function setup() {
  // Create test users if they don't exist
  const users = [];
  for (let i = 0; i < 100; i++) {
    users.push({
      id: `user-${i}`,
      name: `Test User ${i}`,
      email: `user${i}@example.com`,
    });
  }
  
  // Save test users to a file
  const fs = require('k6/x/file');
  fs.writeStringToFile('./test-data/users.json', JSON.stringify(users));
  
  console.log(`Starting concurrency tests against ${BASE_URL}`);
  return {
    startTime: new Date().toISOString(),
    testUsers: users.length,
  };
}

// Teardown function
export function teardown(data) {
  console.log(`\nTest completed at ${new Date().toISOString()}`);
  console.log(`Test started at ${data.startTime}`);
  console.log(`Tested with ${data.testUsers} test users`);
  console.log('Concurrency test completed');
}

// Test for race conditions
export function testRaceConditions() {
  // This function would be called by a separate scenario
  // to test for race conditions in concurrent operations
  const userId = `user-${Math.floor(Math.random() * 100)}`;
  const url = `${BASE_URL}/users/${userId}`;
  
  // Simulate concurrent updates to the same resource
  const update1 = http.patch(
    url, 
    JSON.stringify({ name: 'Updated Name 1' }), 
    { headers }
  );
  
  const update2 = http.patch(
    url, 
    JSON.stringify({ name: 'Updated Name 2' }), 
    { headers }
  );
  
  // Check that at least one update was successful
  check([update1, update2], {
    'at least one update succeeded': (responses) => 
      responses.some(r => r.status === 200),
    'no conflicting updates': (responses) => {
      const successful = responses.filter(r => r.status === 200);
      return successful.length <= 1; // Only one update should succeed
    },
  });
}
