## 4. Google (ADK / Vertex AI)

### 4.1 Agent Development Kit (ADK) — Default Identity Prompt

Google's ADK auto-injects a minimal identity prompt for every agent. This is the **only** default system instruction:

```python
# Source: adk-python/src/google/adk/flows/llm_flows/identity.py
si = f'You are an agent. Your internal name is "{agent.name}".'
if agent.description:
    si += f' The description about you is "{agent.description}".'
```

The developer-provided `instruction` field is appended after this identity block:

```python
from google.adk.agents import Agent

root_agent = Agent(
    name="basic_search_agent",
    model="gemini-2.5-flash",
    description="Agent to answer questions using Google Search.",
    instruction="You are an expert researcher. You always stick
    to the facts.",
    tools=[google_search],
)
```

#### Agent Transfer Instructions (Exact Source Code)

When sub-agents are present, ADK injects transfer instructions:

```python
# Source: adk-python/src/google/adk/flows/llm_flows/agent_transfer.py
f"""
You have a list of other agents to transfer to:

{agent_descriptions}

If you are the best to answer the question according to your
description, you can answer it.

If another agent is better for answering the question according
to its description, call `{tool_name}` function to transfer the
question to that agent. When transferring, do not generate any
text other than the function call.

**NOTE**: the only available agents for `{tool_name}` function
are {formatted_agent_names}.
"""
```

If parent transfer is enabled:
```python
f"""
If neither you nor the other agents are best for the question,
transfer to your parent agent {agent.parent_agent.name}.
"""
```

*Source: [google/adk-python identity.py](https://github.com/google/adk-python/blob/main/src/google/adk/flows/llm_flows/identity.py), [agent_transfer.py](https://github.com/google/adk-python/blob/main/src/google/adk/flows/llm_flows/agent_transfer.py)*

#### YAML-Based Configuration (No-Code)

```yaml
name: assistant_agent
model: gemini-2.5-flash
description: A helper agent that can answer users' questions.
instruction: You are an agent to help answer users' various questions.
```

### 4.2 Multi-Agent Orchestration (AutoFlow)

ADK uses **AutoFlow** for automatic agent delegation. When sub-agents are present, the root agent's LLM considers each sub-agent's `description` and generates `transfer_to_agent(agent_name='target')` calls when appropriate.

```python
# Source: adk-python/src/google/adk/flows/llm_flows/auto_flow.py
class AutoFlow(SingleFlow):
    """AutoFlow is SingleFlow with agent transfer capability.

    Agent transfer is allowed in the following direction:
    1. from parent to sub-agent;
    2. from sub-agent to parent;
    3. from sub-agent to its peer agents;

    For peer-agent transfers, it's only enabled when all below
    conditions are met:
    - The parent agent is also an LlmAgent.
    - `disallow_transfer_to_peers` option of this agent is
      False (default).
    """
```

#### How Transfer Works

1. The root agent's LLM reads its own `instruction` plus all sub-agents' `description` fields
2. If a query matches a sub-agent's capability, the LLM generates: `transfer_to_agent(agent_name='target_agent_name')`
3. AutoFlow intercepts this, calls `root_agent.find_agent()`, and switches `InvocationContext`
4. The target agent takes over with its own instruction/tools

```python
root_agent = Agent(
    name="coordinator",
    model="gemini-2.5-pro",
    description="Coordinates between research and analysis agents.",
    instruction="""You coordinate a team of specialists.
    When the user needs factual research, delegate to the
    research_agent. When they need data analysis, delegate
    to the analysis_agent. Always synthesize results before
    responding to the user.""",
    sub_agents=[research_agent, analysis_agent],
)
```

**Known issue**: The LLM sometimes hallucinates extra arguments for `transfer_to_agent()` (e.g., `prompt=`, `user_request=`, `message=`), causing runtime errors.

*Source: [Google ADK Multi-Agent Systems](https://google.github.io/adk-docs/agents/multi-agents/), [ADK Python Source](https://github.com/google/adk-python)*

### 4.3 Agent Cards (A2A Protocol)

For inter-agent communication via the A2A protocol, agents publish discovery metadata:

```json
{
  "name": "research-agent",
  "description": "Expert research agent that can search and
   synthesize information from multiple sources.",
  "url": "https://agent.example.com/a2a",
  "capabilities": {
    "input": ["text/plain", "application/json"],
    "output": ["text/plain", "application/json"],
    "streaming": true
  },
  "authentication": {
    "schemes": ["bearer"]
  },
  "version": "1.0.0"
}
```

Agent Cards are served at `/.well-known/agent.json` and enable runtime discovery. The orchestrator reads these cards to understand what each agent can do before delegating tasks.

*Source: [Google ADK Documentation](https://docs.cloud.google.com/agent-builder/agent-engine/develop/adk), [ADK Agent Config](https://google.github.io/adk-docs/agents/config/)*

---

