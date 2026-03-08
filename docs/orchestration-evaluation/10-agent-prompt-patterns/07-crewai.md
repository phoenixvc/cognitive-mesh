## 7. CrewAI

### 7.1 Agent Definition Structure & Assembled Prompt

CrewAI agents are defined with `role`, `goal`, and `backstory` — a character-driven approach. These are assembled into the system prompt:

```python
agent = Agent(
    role="Senior Research Analyst",
    goal="Uncover cutting-edge developments in AI",
    backstory="""You work at a leading tech think tank.
    Your expertise lies in identifying emerging trends.
    You have a knack for dissecting complex data and
    presenting actionable insights.""",
    allow_delegation=False,  # Default is False as of 2025
    tools=[search_tool, scrape_tool],
)
```

#### Assembled System Prompt (Exact Format)

```
You are {Agent Role}. {Agent backstory}. Your personal goal
is: {Agent goal}

To give my best complete final answer to the task respond
using the exact following format:

Thought: I now can give a great answer
Final Answer: Your final answer must be the great and the
most complete as possible, it must be outcome described.

I MUST use these formats, my job depends on it!
```

#### Task Execution User Prompt

```
{Task description}

This is the expected criteria for your final answer:
{Task expected_output}
you MUST return the actual complete content as the final
answer, not a summary.

Begin! This is VERY important to you, use the tools available
and give your best Final Answer, your job depends on it!

Thought:
```

**Notable**: CrewAI uses psychological urgency ("your job depends on it!") as a prompt engineering technique to increase format compliance. This is unique among frameworks.

### 7.2 Manager Agent (Hierarchical Process)

When using `Process.hierarchical`, CrewAI creates a default manager or accepts a custom one:

```python
manager = Agent(
    role="Project Manager",
    goal="Efficiently manage the crew and ensure high-quality
    task completion",
    backstory="You're an experienced project manager, skilled
    in overseeing complex projects and guiding teams to success.
    Your role is to coordinate the efforts of the crew members,
    ensuring that each task is completed on time and to the
    highest standard.",
    allow_delegation=True,
)

crew = Crew(
    agents=[researcher, writer],
    tasks=[task],
    manager_agent=manager,
    process=Process.hierarchical,
)
```

### 7.3 Custom Prompt Templates

CrewAI supports overriding the prompt template structure per agent:

```python
agent = Agent(
    role="{topic} specialist",
    system_template="""<|start_header_id|>system<|end_header_id|>
    {{ .System }}<|eot_id|>""",
    prompt_template="""<|start_header_id|>user<|end_header_id|>
    {{ .Prompt }}<|eot_id|>""",
    response_template="""<|start_header_id|>assistant<|end_header_id|>
    {{ .Response }}<|eot_id|>""",
)
```

### 7.4 Delegation Control

```python
# Controlled hierarchical delegation (proposed/merged feature)
agent = Agent(
    role="Executive Director",
    allow_delegation=True,
    allowed_agents=["Communications Manager", "Research Manager"],
)
```

### 7.5 Best Practices from CrewAI Documentation

> "Specialists over generalists: agents perform better with specialized roles rather than general ones."

> "The 80/20 rule: 80% of your effort should go into designing tasks, and only 20% into defining agents."

*Source: [CrewAI Agent docs](https://docs.crewai.com/en/concepts/agents), [CrewAI Custom Manager](https://docs.crewai.com/how-to/custom-manager-agent), [CrewAI GitHub](https://github.com/crewAIInc/crewAI)*

---

