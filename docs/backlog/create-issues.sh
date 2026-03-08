#!/bin/bash
# Create GitHub issues for agentic pattern implementations
# Run with: ./create-issues.sh
# Requires: gh auth login

set -e

REPO="${REPO:-JustAGhosT/cognitive-mesh}"
DELAY="${DELAY:-2}"  # Seconds between issues to avoid rate limiting

echo "Creating issues in $REPO..."
echo "Press Ctrl+C to stop at any time."


echo "Creating issue: AgentConfigAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentConfigAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for team-shared agent configuration as code.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentConfigAdapter` implementing `IAgentConfigPort`.

**Source:** `src/AgencyLayer/Agents/Ports/IAgentConfigPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentPoolAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentPoolAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for agent pooling and lifecycle management.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentPoolAdapter` implementing `IAgentPoolPort`.

**Source:** `src/AgencyLayer/Agents/Ports/IAgentPoolPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AsyncCodingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AsyncCodingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for asynchronous coding agent pipeline.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AsyncCodingAdapter` implementing `IAsyncCodingPort`.

**Source:** `src/AgencyLayer/Agents/Ports/IAsyncCodingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodeReviewAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodeReviewAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
code changes for bugs, security issues, style violations, and

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodeReviewAdapter` implementing `ICodeReviewPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ICodeReviewPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodeSkillModelAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodeSkillModelAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for merged code + language skill model.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodeSkillModelAdapter` implementing `ICodeSkillModelPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ICodeSkillModelPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodebaseOptimizationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodebaseOptimizationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for codebase optimization for agents.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodebaseOptimizationAdapter` implementing `ICodebaseOptimizationPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ICodebaseOptimizationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodebaseQAAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodebaseQAAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
answering capabilities, helping developers understand large codebases

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodebaseQAAdapter` implementing `ICodebaseQAPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ICodebaseQAPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CriticReviewAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CriticReviewAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for CriticGPT-style code review.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CriticReviewAdapter` implementing `ICriticReviewPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ICriticReviewPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SpecDrivenAgentAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SpecDrivenAgentAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for specification-driven agent development.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SpecDrivenAgentAdapter` implementing `ISpecDrivenAgentPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ISpecDrivenAgentPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SpecializedAgentAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SpecializedAgentAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for specialized agent management (CrewAI-style).

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SpecializedAgentAdapter` implementing `ISpecializedAgentPort`.

**Source:** `src/AgencyLayer/Agents/Ports/ISpecializedAgentPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolSelectionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolSelectionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for intelligent tool selection.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolSelectionAdapter` implementing `IToolSelectionPort`.

**Source:** `src/AgencyLayer/Agents/Ports/IToolSelectionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AbstractedCodeReviewAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AbstractedCodeReviewAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for AbstractedCodeReview functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AbstractedCodeReviewAdapter` implementing `IAbstractedCodeReviewPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IAbstractedCodeReviewPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentPersonalityAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentPersonalityAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for AgentPersonality functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentPersonalityAdapter` implementing `IAgentPersonalityPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IAgentPersonalityPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentRFTAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentRFTAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for AgentRFT functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentRFTAdapter` implementing `IAgentRFTPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IAgentRFTPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AntiRewardHackingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AntiRewardHackingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for AntiRewardHacking functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AntiRewardHackingAdapter` implementing `IAntiRewardHackingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IAntiRewardHackingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: BashExecutionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement BashExecutionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for BashExecution functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `BashExecutionAdapter` implementing `IBashExecutionPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IBashExecutionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: BurnTheBoatsAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement BurnTheBoatsAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for BurnTheBoats functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `BurnTheBoatsAdapter` implementing `IBurnTheBoatsPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IBurnTheBoatsPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CLIOrchestrationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CLIOrchestrationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for CLIOrchestration functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CLIOrchestrationAdapter` implementing `ICLIOrchestrationPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ICLIOrchestrationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CLISkillAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CLISkillAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for CLISkill functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CLISkillAdapter` implementing `ICLISkillPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ICLISkillPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodeOverAPIAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodeOverAPIAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for CodeOverAPI functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodeOverAPIAdapter` implementing `ICodeOverAPIPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ICodeOverAPIPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodeThenExecuteAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodeThenExecuteAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for CodeThenExecute functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodeThenExecuteAdapter` implementing `ICodeThenExecutePort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ICodeThenExecutePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ContextAnxietyAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ContextAnxietyAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for ContextAnxiety functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ContextAnxietyAdapter` implementing `IContextAnxietyPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IContextAnxietyPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CustomSandboxAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CustomSandboxAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for CustomSandbox functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CustomSandboxAdapter` implementing `ICustomSandboxPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ICustomSandboxPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DevToolingResetAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DevToolingResetAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for DevToolingReset functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DevToolingResetAdapter` implementing `IDevToolingResetPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IDevToolingResetPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: FilesystemStateAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement FilesystemStateAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for FilesystemState functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `FilesystemStateAdapter` implementing `IFilesystemStatePort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IFilesystemStatePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: FrontierDevelopmentAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement FrontierDevelopmentAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for FrontierDevelopment functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `FrontierDevelopmentAdapter` implementing `IFrontierDevelopmentPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IFrontierDevelopmentPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: InferenceHealedRewardAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement InferenceHealedRewardAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for InferenceHealedReward functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `InferenceHealedRewardAdapter` implementing `IInferenceHealedRewardPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IInferenceHealedRewardPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: IsolatedVMAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement IsolatedVMAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for IsolatedVM functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `IsolatedVMAdapter` implementing `IIsolatedVMPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IIsolatedVMPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: LaneQueueingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement LaneQueueingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for LaneQueueing functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `LaneQueueingAdapter` implementing `ILaneQueueingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ILaneQueueingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MemRLAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MemRLAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for MemRL functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MemRLAdapter` implementing `IMemRLPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IMemRLPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MilestoneEscrowAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MilestoneEscrowAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for MilestoneEscrow functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MilestoneEscrowAdapter` implementing `IMilestoneEscrowPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IMilestoneEscrowPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: NonCustodialSpendingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement NonCustodialSpendingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for NonCustodialSpending functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `NonCustodialSpendingAdapter` implementing `INonCustodialSpendingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/INonCustodialSpendingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PerceptionArchitectureAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PerceptionArchitectureAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for PerceptionArchitecture functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PerceptionArchitectureAdapter` implementing `IPerceptionArchitecturePort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IPerceptionArchitecturePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PosteriorSamplingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PosteriorSamplingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for PosteriorSampling functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PosteriorSamplingAdapter` implementing `IPosteriorSamplingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IPosteriorSamplingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PromptCachingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PromptCachingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for PromptCaching functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PromptCachingAdapter` implementing `IPromptCachingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IPromptCachingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: RLAIFAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement RLAIFAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for RLAIF functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `RLAIFAdapter` implementing `IRLAIFPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IRLAIFPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: RLSampleSelectionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement RLSampleSelectionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for RLSampleSelection functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `RLSampleSelectionAdapter` implementing `IRLSampleSelectionPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IRLSampleSelectionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: RewardShapingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement RewardShapingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for RewardShaping functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `RewardShapingAdapter` implementing `IRewardShapingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IRewardShapingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ScaffoldingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ScaffoldingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for Scaffolding functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ScaffoldingAdapter` implementing `IScaffoldingPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IScaffoldingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ShellContextAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ShellContextAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for ShellContext functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ShellContextAdapter` implementing `IShellContextPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IShellContextPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SoulboundIdentityAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SoulboundIdentityAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for SoulboundIdentity functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SoulboundIdentityAdapter` implementing `ISoulboundIdentityPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ISoulboundIdentityPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SubjectHygieneAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SubjectHygieneAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for SubjectHygiene functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SubjectHygieneAdapter` implementing `ISubjectHygienePort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ISubjectHygienePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolSelectionGuideAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolSelectionGuideAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for ToolSelectionGuide functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolSelectionGuideAdapter` implementing `IToolSelectionGuidePort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IToolSelectionGuidePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: TriggerVocabularyAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement TriggerVocabularyAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for TriggerVocabulary functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `TriggerVocabularyAdapter` implementing `ITriggerVocabularyPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/ITriggerVocabularyPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: VMOperatorAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement VMOperatorAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for VMOperator functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `VMOperatorAdapter` implementing `IVMOperatorPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IVMOperatorPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: WorkspaceOrchestrationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement WorkspaceOrchestrationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Implement adapter for WorkspaceOrchestration functionality

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `WorkspaceOrchestrationAdapter` implementing `IWorkspaceOrchestrationPort`.

**Source:** `src/AgencyLayer/LowPriority/Ports/IWorkspaceOrchestrationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P3 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ConversationPatternAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ConversationPatternAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
patterns including two-agent, sequential, group chat, and hierarchical

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ConversationPatternAdapter` implementing `IConversationPatternPort`.

**Source:** `src/AgencyLayer/MultiAgentOrchestration/Ports/IConversationPatternPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ActionCachingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ActionCachingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
for debugging, testing, and performance optimization. Cached actions

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ActionCachingAdapter` implementing `IActionCachingPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IActionCachingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentCommunicationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentCommunicationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for agent-to-agent communication.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentCommunicationAdapter` implementing `IAgentCommunicationPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IAgentCommunicationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentStateAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentStateAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for agent state management.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentStateAdapter` implementing `IAgentStatePort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IAgentStatePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AutonomousLoopAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AutonomousLoopAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
on iterations, time, budget, and errors, with progress tracking

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AutonomousLoopAdapter` implementing `IAutonomousLoopPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IAutonomousLoopPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: BestOfNAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement BestOfNAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for recursive best-of-N delegation.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `BestOfNAdapter` implementing `IBestOfNPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IBestOfNPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: BudgetRoutingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement BudgetRoutingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
within the remaining budget and blocking requests that would exceed

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `BudgetRoutingAdapter` implementing `IBudgetRoutingPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IBudgetRoutingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CanaryRolloutAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CanaryRolloutAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
by routing a small percentage of traffic to the new version and monitoring

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CanaryRolloutAdapter` implementing `ICanaryRolloutPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ICanaryRolloutPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CoherenceSessionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CoherenceSessionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
context consolidation, checkpointing, and progress tracking

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CoherenceSessionAdapter` implementing `ICoherenceSessionPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ICoherenceSessionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ComplexityEscalationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ComplexityEscalationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for progressive complexity escalation.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ComplexityEscalationAdapter` implementing `IComplexityEscalationPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IComplexityEscalationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DistributedExecutionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DistributedExecutionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for distributed execution with cloud workers.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DistributedExecutionAdapter` implementing `IDistributedExecutionPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IDistributedExecutionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DualAgentAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DualAgentAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
complex setup tasks and a maintainer for ongoing operations,

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DualAgentAdapter` implementing `IDualAgentPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IDualAgentPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DualLLMAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DualLLMAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
for complex reasoning and planning, and a faster/cheaper model for

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DualLLMAdapter` implementing `IDualLLMPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IDualLLMPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: FeatureContractAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement FeatureContractAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for feature list as immutable contract.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `FeatureContractAdapter` implementing `IFeatureContractPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IFeatureContractPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: GraphOfThoughtsAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement GraphOfThoughtsAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
that can have multiple connections (support, contradict, refine),

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `GraphOfThoughtsAdapter` implementing `IGraphOfThoughtsPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IGraphOfThoughtsPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: HandoffAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement HandoffAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for seamless background-to-foreground handoff.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `HandoffAdapter` implementing `IHandoffPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IHandoffPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: HumanInTheLoopAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement HumanInTheLoopAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for human-in-the-loop operations.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `HumanInTheLoopAdapter` implementing `IHumanInTheLoopPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IHumanInTheLoopPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: HybridWorkflowAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement HybridWorkflowAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for hybrid LLM/Code workflow coordination.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `HybridWorkflowAdapter` implementing `IHybridWorkflowPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IHybridWorkflowPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MapReduceAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MapReduceAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for LLM Map-Reduce pattern.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MapReduceAdapter` implementing `IMapReducePort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IMapReducePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ModelFallbackAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ModelFallbackAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
when the primary model is unavailable or degraded. It maintains health

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ModelFallbackAdapter` implementing `IModelFallbackPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IModelFallbackPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MultiModelEditAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MultiModelEditAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for multi-model orchestration for complex edits.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MultiModelEditAdapter` implementing `IMultiModelEditPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IMultiModelEditPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PlannerWorkerAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PlannerWorkerAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
planner agent and multiple worker agents for parallel execution,

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PlannerWorkerAdapter` implementing `IPlannerWorkerPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IPlannerWorkerPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ReliabilityMapAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ReliabilityMapAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
and mitigating reliability problems in agentic systems, based on

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ReliabilityMapAdapter` implementing `IReliabilityMapPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IReliabilityMapPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SandboxFanOutAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SandboxFanOutAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for adaptive sandbox fan-out controller.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SandboxFanOutAdapter` implementing `ISandboxFanOutPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ISandboxFanOutPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SchemaValidationRetryAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SchemaValidationRetryAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
validation failures to improve corrections across steps and

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SchemaValidationRetryAdapter` implementing `ISchemaValidationRetryPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ISchemaValidationRetryPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SwarmMigrationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SwarmMigrationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for swarm migration pattern.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SwarmMigrationAdapter` implementing `ISwarmMigrationPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ISwarmMigrationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: TreeSearchAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement TreeSearchAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
branching reasoning paths, backtracking, and systematic search for

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `TreeSearchAdapter` implementing `ITreeSearchPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/ITreeSearchPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: WorkflowEvalsAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement WorkflowEvalsAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
responses, allowing deterministic testing of agent decision-making

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `WorkflowEvalsAdapter` implementing `IWorkflowEvalsPort`.

**Source:** `src/AgencyLayer/Orchestration/Ports/IWorkflowEvalsPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DualUseToolAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DualUseToolAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for dual-use tool design.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DualUseToolAdapter` implementing `IDualUseToolPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IDualUseToolPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DynamicToolGenerationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DynamicToolGenerationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
specifications, allowing the system to extend its capabilities

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DynamicToolGenerationAdapter` implementing `IDynamicToolGenerationPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IDynamicToolGenerationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MCPToolAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MCPToolAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
enabling discovery, invocation, and management of tools

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MCPToolAdapter` implementing `IMCPToolPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IMCPToolPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MultiPlatformCommunicationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MultiPlatformCommunicationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
messages across multiple communication platforms (Slack, Teams,

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MultiPlatformCommunicationAdapter` implementing `IMultiPlatformCommunicationPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IMultiPlatformCommunicationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MultimodalAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MultimodalAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for visual AI multimodal integration.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MultimodalAdapter` implementing `IMultimodalPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IMultimodalPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PatchSteeringAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PatchSteeringAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for patch steering via prompted tool selection.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PatchSteeringAdapter` implementing `IPatchSteeringPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IPatchSteeringPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SemanticRoutingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SemanticRoutingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for semantic intent routing.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SemanticRoutingAdapter` implementing `ISemanticRoutingPort`.

**Source:** `src/AgencyLayer/Tools/Ports/ISemanticRoutingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SubagentValidationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SubagentValidationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
ensuring code compiles, JSON is valid, and outputs meet expected

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SubagentValidationAdapter` implementing `ISubagentValidationPort`.

**Source:** `src/AgencyLayer/Tools/Ports/ISubagentValidationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolCreationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolCreationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for democratization of tooling via agents.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolCreationAdapter` implementing `IToolCreationPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IToolCreationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolDiscoveryAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolDiscoveryAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for progressive tool discovery.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolDiscoveryAdapter` implementing `IToolDiscoveryPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IToolDiscoveryPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolRetryAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolRetryAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
with configurable backoff strategies, error classification,

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolRetryAdapter` implementing `IToolRetryPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IToolRetryPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ToolSteeringAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ToolSteeringAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for tool use steering via prompting.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ToolSteeringAdapter` implementing `IToolSteeringPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IToolSteeringPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: WebSearchAgentAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement WebSearchAgentAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for AI web search agent loop.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `WebSearchAgentAdapter` implementing `IWebSearchAgentPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IWebSearchAgentPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: WebhookTriggerAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement WebhookTriggerAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for multi-platform webhook triggers.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `WebhookTriggerAdapter` implementing `IWebhookTriggerPort`.

**Source:** `src/AgencyLayer/Tools/Ports/IWebhookTriggerPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AuditTrailAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AuditTrailAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for comprehensive audit trail.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AuditTrailAdapter` implementing `IAuditTrailPort`.

**Source:** `src/BusinessApplications/Compliance/Ports/IAuditTrailPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** product | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: LatentDemandAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement LatentDemandAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for latent demand product discovery.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `LatentDemandAdapter` implementing `ILatentDemandPort`.

**Source:** `src/BusinessApplications/CustomerIntelligence/Ports/ILatentDemandPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** product | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: VectorStoreAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement VectorStoreAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for vector store operations.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `VectorStoreAdapter` implementing `IVectorStorePort`.

**Source:** `src/FoundationLayer/Persistence/Ports/IVectorStorePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CredentialSyncAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CredentialSyncAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
(Azure Key Vault, HashiCorp Vault, AWS Secrets Manager) for secure

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CredentialSyncAdapter` implementing `ICredentialSyncPort`.

**Source:** `src/FoundationLayer/Security/Ports/ICredentialSyncPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: EgressControlAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement EgressControlAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
by agents and tools. All outbound traffic must be explicitly allowed

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `EgressControlAdapter` implementing `IEgressControlPort`.

**Source:** `src/FoundationLayer/Security/Ports/IEgressControlPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: PIITokenizationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement PIITokenizationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
personally identifiable information in text content. Tokens are deterministic

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `PIITokenizationAdapter` implementing `IPIITokenizationPort`.

**Source:** `src/FoundationLayer/Security/Ports/IPIITokenizationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SandboxAuthorizationAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SandboxAuthorizationAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
authorization for tool execution and providing resource-limited sandbox

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SandboxAuthorizationAdapter` implementing `ISandboxAuthorizationPort`.

**Source:** `src/FoundationLayer/Security/Ports/ISandboxAuthorizationPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SecurityScanningAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SecurityScanningAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
into CI/CD pipelines for continuous security validation. All scans are

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SecurityScanningAdapter` implementing `ISecurityScanningPort`.

**Source:** `src/FoundationLayer/Security/Ports/ISecurityScanningPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ThreatModelAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ThreatModelAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
This port analyzes agent configurations and raises alerts when approaching

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ThreatModelAdapter` implementing `IThreatModelPort`.

**Source:** `src/FoundationLayer/Security/Ports/IThreatModelPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** infra | **Priority:** P1 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CIFeedbackAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CIFeedbackAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for background agent with CI feedback.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CIFeedbackAdapter` implementing `ICIFeedbackPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/ICIFeedbackPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CompoundingEngineeringAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CompoundingEngineeringAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for compounding engineering pattern.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CompoundingEngineeringAdapter` implementing `ICompoundingEngineeringPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/ICompoundingEngineeringPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: DogfoodingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement DogfoodingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for dogfooding with rapid iteration.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `DogfoodingAdapter` implementing `IDogfoodingPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/IDogfoodingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: FeedbackLoopAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement FeedbackLoopAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for continuous feedback loops.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `FeedbackLoopAdapter` implementing `IFeedbackLoopPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/IFeedbackLoopPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: IncidentEvalAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement IncidentEvalAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
creating a feedback loop that prevents regression and captures

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `IncidentEvalAdapter` implementing `IIncidentEvalPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/IIncidentEvalPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ResearchShippingAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ResearchShippingAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for shipping as research pattern.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ResearchShippingAdapter` implementing `IResearchShippingPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/IResearchShippingPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SkillEvolutionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SkillEvolutionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for skill library evolution.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SkillEvolutionAdapter` implementing `ISkillEvolutionPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/ISkillEvolutionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SkillRefinementAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SkillRefinementAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
execution outcomes and user feedback, automatically proposing

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SkillRefinementAdapter` implementing `ISkillRefinementPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/ISkillRefinementPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SpecTestFeedbackAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SpecTestFeedbackAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
into executable tests, creating a feedback loop that validates

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SpecTestFeedbackAdapter` implementing `ISpecTestFeedbackPort`.

**Source:** `src/MetacognitiveLayer/ContinuousLearning/Ports/ISpecTestFeedbackPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ReasoningMonitorAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ReasoningMonitorAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for chain-of-thought monitoring and interruption.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ReasoningMonitorAdapter` implementing `IReasoningMonitorPort`.

**Source:** `src/MetacognitiveLayer/PerformanceMonitoring/Ports/IReasoningMonitorPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: StreamingProgressAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement StreamingProgressAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
enabling UIs and other consumers to display live updates

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `StreamingProgressAdapter` implementing `IStreamingProgressPort`.

**Source:** `src/MetacognitiveLayer/PerformanceMonitoring/Ports/IStreamingProgressPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: AgentIdentityAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement AgentIdentityAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for self-identity accumulation.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `AgentIdentityAdapter` implementing `IAgentIdentityPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IAgentIdentityPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: CodeContextAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement CodeContextAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for curated code context window.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `CodeContextAdapter` implementing `ICodeContextPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/ICodeContextPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ContextCompactionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ContextCompactionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
retention to keep context within token limits while preserving

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ContextCompactionAdapter` implementing `IContextCompactionPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IContextCompactionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: EpisodicMemoryAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement EpisodicMemoryAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for episodic memory.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `EpisodicMemoryAdapter` implementing `IEpisodicMemoryPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IEpisodicMemoryPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: FileContextAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement FileContextAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for curated file context window.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `FileContextAdapter` implementing `IFileContextPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IFileContextPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: MemorySynthesisAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement MemorySynthesisAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
learnings that can improve agent performance over time. It identifies

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `MemorySynthesisAdapter` implementing `IMemorySynthesisPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IMemorySynthesisPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ProgressiveDisclosureAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ProgressiveDisclosureAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for progressive disclosure of large files.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ProgressiveDisclosureAdapter` implementing `IProgressiveDisclosurePort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IProgressiveDisclosurePort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SemanticFilteringAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SemanticFilteringAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
relevant content based on query similarity, enabling efficient context

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SemanticFilteringAdapter` implementing `ISemanticFilteringPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/ISemanticFilteringPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: WorkingMemoryAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement WorkingMemoryAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
tasks, notes, and context during complex multi-step operations.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `WorkingMemoryAdapter` implementing `IWorkingMemoryPort`.

**Source:** `src/MetacognitiveLayer/Protocols/Common/Memory/Ports/IWorkingMemoryPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ObservabilityAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ObservabilityAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
including distributed tracing, metrics collection, and structured

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ObservabilityAdapter` implementing `IObservabilityPort`.

**Source:** `src/MetacognitiveLayer/Telemetry/Ports/IObservabilityPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ChainOfThoughtAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ChainOfThoughtAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
including zero-shot, few-shot, and self-consistency approaches

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ChainOfThoughtAdapter` implementing `IChainOfThoughtPort`.

**Source:** `src/ReasoningLayer/StructuredReasoning/Ports/IChainOfThoughtPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: RAGAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement RAGAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for RAG (Retrieval-Augmented Generation).

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `RAGAdapter` implementing `IRAGPort`.

**Source:** `src/ReasoningLayer/StructuredReasoning/Ports/IRAGPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ReActAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ReActAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
between thinking (reasoning) and acting (tool use), with observations

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ReActAdapter` implementing `IReActPort`.

**Source:** `src/ReasoningLayer/StructuredReasoning/Ports/IReActPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: ReflexionAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement ReflexionAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
through self-reflection on past episodes, storing insights in

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `ReflexionAdapter` implementing `IReflexionPort`.

**Source:** `src/ReasoningLayer/StructuredReasoning/Ports/IReflexionPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Creating issue: SelfDiscoverAdapter"
gh issue create --repo "$REPO" \
  --title "[FEATURE] Implement SelfDiscoverAdapter" \
  --label "enhancement,triage" \
  --body "$(cat <<'EOF'
## Summary
Port for Self-Discover LLM self-composed reasoning.

## Motivation
Port interface from agentic patterns analysis. Adapter needed for the port contract.

## Proposed Solution
Create `SelfDiscoverAdapter` implementing `ISelfDiscoverPort`.

**Source:** `src/ReasoningLayer/StructuredReasoning/Ports/ISelfDiscoverPort.cs`

## Acceptance Criteria
- [ ] Adapter implements interface methods
- [ ] Unit tests >80% coverage
- [ ] XML docs on public members

**Area:** backend | **Priority:** P2 | **Phase:** active
EOF
)"
sleep $DELAY


echo "Done! All issues created."
