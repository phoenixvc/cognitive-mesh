import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:3000/v1';
const AUTH_TOKEN = __ENV.AUTH_TOKEN || 'your-test-token';
const RATE_LIMIT = parseInt(__ENV.RATE_LIMIT || '100', 10); // Default to 100 req/min

// Custom metrics
const rateLimited = new Rate('rate_limited_requests');
const successful = new Rate('successful_requests');
const errors = new Rate('error_requests');

// Headers for all requests
const headers = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${AUTH_TOKEN}`,
};

// Test scenarios
export const options = {
  scenarios: {
    // Test rate limit enforcement
    rate_limit_test: {
      executor: 'ramping-arrival-rate',
      preAllocatedVUs: 10,
      timeUnit: '1s',
      startRate: RATE_LIMIT / 60, // Start at the rate limit per second
      stages: [
        // Ramp up to 2x the rate limit over 30 seconds
        { target: (RATE_LIMIT * 1.5) / 60, duration: '30s' },
        // Stay at this rate for 1 minute
        { target: (RATE_LIMIT * 1.5) / 60, duration: '1m' },
        // Ramp down
        { target: 0, duration: '30s' },
      ],
    },
    // Test rate limit reset behavior
    rate_limit_reset: {
      executor: 'constant-arrival-rate',
      rate: (RATE_LIMIT * 1.2) / 60, // 20% over the limit
      timeUnit: '1s',
      duration: '2m',
      preAllocatedVUs: 20,
      maxVUs: 50,
      startTime: '3m', // Start after the first test
    },
  },
  thresholds: {
    'rate_limited_requests': [
      // We expect some rate limits when testing above the limit
      { threshold: 'count>0', abortOnFail: false },
    ],
    'http_req_duration': ['p(95)<1000'],
  },
};

// Track rate limit headers
let rateLimitMetrics = {
  limit: 0,
  remaining: 0,
  reset: 0,
};

// Main test function
export default function () {
  const url = `${BASE_URL}/test/endpoint`;
  const res = http.get(url, { headers });
  
  // Extract rate limit headers if present
  if (res.headers['X-RateLimit-Limit']) {
    rateLimitMetrics = {
      limit: parseInt(res.headers['X-RateLimit-Limit'], 10) || rateLimitMetrics.limit,
      remaining: parseInt(res.headers['X-RateLimit-Remaining'], 10) || rateLimitMetrics.remaining,
      reset: parseInt(res.headers['X-RateLimit-Reset'], 10) || rateLimitMetrics.reset,
    };
  }
  
  // Check response status
  const isRateLimited = res.status === 429;
  const isSuccess = res.status >= 200 && res.status < 300;
  
  // Update metrics
  if (isRateLimited) {
    rateLimited.add(1);
    console.log('Rate limited:', res.status, res.body);
  } else if (isSuccess) {
    successful.add(1);
  } else {
    errors.add(1);
    console.log('Error:', res.status, res.body);
  }
  
  // Add assertions
  check(res, {
    'status is 200 or 429': (r) => r.status === 200 || r.status === 429,
    'has rate limit headers': (r) => 
      !!(r.headers['X-RateLimit-Limit'] && 
         r.headers['X-RateLimit-Remaining'] && 
         r.headers['X-RateLimit-Reset']),
  });
  
  // Add a small delay between requests
  sleep(0.1);
}

// Setup function
export function setup() {
  console.log(`Starting rate limit tests against ${BASE_URL}`);
  console.log(`Configured rate limit: ${RATE_LIMIT} requests per minute`);
  
  return {
    startTime: new Date().toISOString(),
    rateLimit: RATE_LIMIT,
  };
}

// Teardown function
export function teardown(data) {
  console.log(`\nTest completed at ${new Date().toISOString()}`);
  console.log(`Test started at ${data.startTime}`);
  console.log(`Configured rate limit: ${data.rateLimit} requests per minute`);
  console.log('Rate limit test completed');
  
  // Log rate limit metrics
  console.log('\nRate Limit Metrics:');
  console.log(`- Limit: ${rateLimitMetrics.limit} requests`);
  console.log(`- Remaining: ${rateLimitMetrics.remaining} requests`);
  console.log(`- Reset: ${new Date(rateLimitMetrics.reset * 1000).toISOString()}`);
}

// Handle test summary
export function handleSummary(data) {
  return {
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    'summary.json': JSON.stringify(data, null, 2),
  };
}

// Test function to verify rate limit headers
export function testRateLimitHeaders() {
  const url = `${BASE_URL}/test/endpoint`;
  const res = http.get(url, { headers });
  
  check(res, {
    'has X-RateLimit-Limit header': (r) => !!r.headers['X-RateLimit-Limit'],
    'has X-RateLimit-Remaining header': (r) => !!r.headers['X-RateLimit-Remaining'],
    'has X-RateLimit-Reset header': (r) => !!r.headers['X-RateLimit-Reset'],
    'remaining is less than or equal to limit': (r) => {
      const limit = parseInt(r.headers['X-RateLimit-Limit'], 10);
      const remaining = parseInt(r.headers['X-RateLimit-Remaining'], 10);
      return remaining <= limit;
    },
  });
}

// Test function to verify rate limit behavior
export function testRateLimitBehavior() {
  const url = `${BASE_URL}/test/endpoint`;
  const responses = [];
  
  // Make requests until we get rate limited
  let rateLimited = false;
  let requestCount = 0;
  
  while (!rateLimited && requestCount < 1000) { // Safety limit
    const res = http.get(url, { headers });
    responses.push(res);
    requestCount++;
    
    if (res.status === 429) {
      rateLimited = true;
      console.log(`Rate limited after ${requestCount} requests`);
      console.log('Rate limit headers:', {
        'X-RateLimit-Limit': res.headers['X-RateLimit-Limit'],
        'X-RateLimit-Remaining': res.headers['X-RateLimit-Remaining'],
        'X-RateLimit-Reset': res.headers['X-RateLimit-Reset'],
      });
    }
    
    // Small delay to avoid overwhelming the server
    sleep(0.01);
  }
  
  // Verify we were rate limited
  check(responses, {
    'eventually got rate limited': (rs) => 
      rs.some(r => r.status === 429),
    'received 200 OK before rate limit': (rs) => 
      rs.some(r => r.status === 200),
  });
  
  return {
    totalRequests: requestCount,
    successfulRequests: responses.filter(r => r.status === 200).length,
    rateLimitedRequests: responses.filter(r => r.status === 429).length,
  };
}
