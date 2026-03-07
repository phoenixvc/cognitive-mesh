# Agent Platform Prompts, System Instructions, and Implementation Details

Research compiled: 2026-03-07

This document catalogs the actual prompts, system instructions, and implementation details used by major AI agent platforms. Where possible, exact prompt text is provided. Where full text is unavailable, the documented structure and key excerpts are included.

---

## 1. Anthropic Claude Agent/Tool Use

### 1.1 Claude Tool Use System Prompt Template

When the Claude API receives a `tools` parameter, it automatically constructs a system prompt from a template. The template structure (from [Anthropic's official docs](https://platform.claude.com/docs/en/agents-and-tools/tool-use/implement-tool-use)):

```
In this environment you have access to a set of tools you can use to answer the user's question.
{{ FORMATTING INSTRUCTIONS }}
String and scalar parameters should be specified as is, while lists and objects should use JSON format.
Note that spaces for string values are not stripped.
The output is not expected to be valid XML and is parsed with regular expressions.
Here are the functions available in JSONSchema format:
{{ TOOL DEFINITIONS IN JSON SCHEMA }}
{{ USER SYSTEM PROMPT }}
{{ TOOL CONFIGURATION }}
```

The tool invocation format presented to Claude uses XML-like blocks:

```xml
<function_calls>
<invoke name="$FUNCTION_NAME">
<parameter name="$PARAMETER_NAME">$PARAMETER_VALUE