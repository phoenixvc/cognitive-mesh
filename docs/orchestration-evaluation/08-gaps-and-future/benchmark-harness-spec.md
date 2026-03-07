# Standard Benchmark Harness Specification

Three standard workflows designed to be implemented across all evaluated systems, enabling apples-to-apples comparison.

## Benchmark 1: Interactive Coordination Latency

**Purpose**: Measure p95 coordination latency — time from request arrival to first meaningful action.

### Workflow Definition

```text
Input: { taskId, agentCount: 3, payload: "classify" }

Steps:
1. Receive request (start timer)
2. Parse and validate input
3. Route to appropriate agent(s)
4. Agent acknowledges receipt (stop timer)

Measurements:
- p50, p95, p99 of steps 1→4
- Breakdown: parse time, routing time, dispatch time
```

### Success Criteria

| Rating | p95 Latency |
|--------|-------------|
| Excellent (5.0) | < 10ms |
| Good (4.0) | < 50ms |
| Acceptable (3.0) | < 200ms |
| Poor (2.0) | < 1000ms |
| Failing (1.0) | > 1000ms |

### Implementation Notes

- Run 1000 iterations with warm-up (100 iterations discarded)
- Measure with monotonic clock (not wall clock)
- Report: min, p50, p95, p99, max, stddev
- Record CPU/memory during benchmark for Efficiency metric

## Benchmark 2: Parallel Fan-Out Throughput

**Purpose**: Measure throughput under parallel execution and validate backpressure behavior.

### Workflow Definition

```text
Input: { batchSize: N, taskTemplate: { type: "transform", payload: "..." } }

Steps:
1. Receive batch of N tasks
2. Fan-out: dispatch all N tasks in parallel
3. Each task performs a simulated 100ms operation
4. Fan-in: collect all N results
5. Aggregate and return

Vary N: 1, 5, 10, 25, 50, 100, 250, 500

Measurements:
- Wall-clock time for complete batch (steps 1→5)
- Tasks/second at each N
- Memory high-water mark at each N
- Point where throughput degrades (saturation point)
- Backpressure behavior at 2× saturation point
```

### Success Criteria

| Rating | Behavior at N=100 |
|--------|-------------------|
| Excellent (5.0) | Linear scaling; graceful backpressure; < 15s wall clock |
| Good (4.0) | Near-linear; some backpressure; < 20s |
| Acceptable (3.0) | Sub-linear but completes; basic queueing; < 30s |
| Poor (2.0) | Significant degradation; no backpressure; < 60s |
| Failing (1.0) | Crashes, hangs, or OOM; > 60s |

### Implementation Notes

- Simulated task should be CPU-light (sleep/await, not compute)
- Record task completion order to verify parallelism (not sequential)
- Test with and without concurrency limits enabled

## Benchmark 3: Failure Injection and Recovery

**Purpose**: Validate retry behavior, circuit breaker activation, and error propagation.

### Workflow Definition

```text
Input: { taskCount: 10, failureRate: 0.3, failureType: "transient" }

Steps:
1. Dispatch 10 tasks
2. 3 tasks fail on first attempt (transient error)
3. Retry failed tasks (verify retry behavior)
4. 1 task fails permanently (non-transient)
5. Verify: 9 tasks succeed, 1 reports permanent failure
6. Verify: all outcomes are logged/auditable

Failure types to test:
- Transient: succeeds on retry
- Permanent: always fails
- Timeout: never responds
- Cascade: failure causes dependent task failure

Measurements:
- Time to first retry
- Total retries consumed
- Permanent failure detection time
- Timeout detection time
- Correct final state (9 success, 1 failure)
- Audit trail completeness
```

### Success Criteria

| Rating | Behavior |
|--------|----------|
| Excellent (5.0) | All failure types handled correctly; circuit breaker activates; full audit trail; idempotent retries |
| Good (4.0) | Transient + permanent handled; timeout detected; partial audit trail |
| Acceptable (3.0) | Basic retry works; permanent failure detected; some logging |
| Poor (2.0) | Retry works but no timeout handling; incomplete error reporting |
| Failing (1.0) | Failures crash the system; no retry; no audit trail |

## Reporting Template

```markdown
# Benchmark Report: [System Name]

## Environment
- Hardware: [CPU, RAM, Disk]
- Runtime: [Language version, framework version]
- Configuration: [relevant config overrides]

## Results

### Benchmark 1: Interactive Coordination Latency
| Metric | Value |
|--------|-------|
| p50 | Xms |
| p95 | Xms |
| p99 | Xms |
| max | Xms |
| CPU (avg) | X% |
| Memory (peak) | X MB |

### Benchmark 2: Fan-Out Throughput
| N | Wall Clock | Tasks/sec | Memory Peak | Backpressure? |
|---|-----------|-----------|-------------|---------------|
| 1 | | | | |
| 10 | | | | |
| 100 | | | | |
| 500 | | | | |

### Benchmark 3: Failure Recovery
| Scenario | Result | Time to Detect | Retries Used | Audit Complete? |
|----------|--------|---------------|--------------|-----------------|
| Transient | | | | |
| Permanent | | | | |
| Timeout | | | | |
| Cascade | | | | |
```

## Implementation Priority

1. **Benchmark 1** first — easiest to implement, highest impact on Latency scoring confidence
2. **Benchmark 3** second — validates Fault Tolerance scores
3. **Benchmark 2** last — requires more infrastructure, validates Throughput/Scalability
