using System.Text.RegularExpressions;
using AgencyLayer.RefactoringAgents.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.RefactoringAgents.Engines;

/// <summary>
/// Pure domain engine that analyzes source code for SOLID and DRY principle violations
/// using heuristic pattern analysis. Follows Hexagonal Architecture as the core business
/// logic layer, independent of any infrastructure concerns.
/// </summary>
public class SolidDryRefactoringEngine : ICodeRefactoringPort
{
    private readonly ILogger<SolidDryRefactoringEngine> _logger;

    // Thresholds for heuristic detection
    private const int SrpMaxMethods = 10;
    private const int SrpMaxFields = 8;
    private const int OcpSwitchCaseThreshold = 5;
    private const int IspMaxInterfaceMethods = 7;
    private const int DryMinDuplicateLineLength = 3;
    private const double DryMinSimilarity = 0.85;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidDryRefactoringEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public SolidDryRefactoringEngine(ILogger<SolidDryRefactoringEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<CodeAnalysisResponse> AnalyzeCodeAsync(
        CodeAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SourceCode))
        {
            return Task.FromResult(new CodeAnalysisResponse
            {
                OverallScore = 1.0,
                Summary = "No source code provided for analysis."
            });
        }

        _logger.LogInformation("Starting {Scope} analysis for {FilePath}",
            request.Scope, string.IsNullOrEmpty(request.FilePath) ? "<inline>" : request.FilePath);

        var response = new CodeAnalysisResponse();
        var lines = request.SourceCode.Split('\n');

        if (request.Scope is AnalysisScope.Solid or AnalysisScope.Both)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DetectSrpViolations(lines, request.FilePath, response);
            DetectOcpViolations(lines, request.FilePath, response);
            DetectLspViolations(lines, request.FilePath, response);
            DetectIspViolations(lines, request.FilePath, response);
            DetectDipViolations(lines, request.FilePath, response);
        }

        if (request.Scope is AnalysisScope.Dry or AnalysisScope.Both)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DetectDryViolations(lines, request.FilePath, response);
        }

        response.OverallScore = CalculateOverallScore(response);
        response.Summary = BuildSummary(response);

        _logger.LogInformation("Analysis complete: {SolidCount} SOLID violations, {DryCount} DRY violations, score {Score:F2}",
            response.SolidViolations.Count, response.DryViolations.Count, response.OverallScore);

        return Task.FromResult(response);
    }

    private void DetectSrpViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        var classPattern = new Regex(@"^\s*(?:public|internal|private|protected)?\s*(?:static|abstract|sealed|partial)?\s*class\s+(\w+)");
        var methodPattern = new Regex(@"^\s*(?:public|private|protected|internal)?\s*(?:static|virtual|override|abstract|async)?\s*(?:\w+(?:<[^>]+>)?(?:\[\])?)\s+(\w+)\s*\(");
        var fieldPattern = new Regex(@"^\s*(?:private|protected|internal)?\s*(?:readonly|static)?\s*\w+(?:<[^>]+>)?\s+_?\w+\s*[;=]");

        string? currentClass = null;
        int classStartLine = 0;
        int methodCount = 0;
        int fieldCount = 0;
        var methodNames = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var classMatch = classPattern.Match(lines[i]);
            if (classMatch.Success)
            {
                // Emit violation for previous class if needed
                if (currentClass != null)
                {
                    EmitSrpIfViolated(currentClass, classStartLine, methodCount, fieldCount, methodNames, filePath, response);
                }

                currentClass = classMatch.Groups[1].Value;
                classStartLine = i + 1;
                methodCount = 0;
                fieldCount = 0;
                methodNames.Clear();
            }

            if (currentClass != null)
            {
                if (methodPattern.IsMatch(lines[i]))
                {
                    var methodMatch = methodPattern.Match(lines[i]);
                    methodCount++;
                    methodNames.Add(methodMatch.Groups[1].Value);
                }

                if (fieldPattern.IsMatch(lines[i]))
                {
                    fieldCount++;
                }
            }
        }

        // Check the last class
        if (currentClass != null)
        {
            EmitSrpIfViolated(currentClass, classStartLine, methodCount, fieldCount, methodNames, filePath, response);
        }
    }

    private void EmitSrpIfViolated(string className, int line, int methodCount, int fieldCount,
        List<string> methodNames, string filePath, CodeAnalysisResponse response)
    {
        if (methodCount > SrpMaxMethods)
        {
            response.SolidViolations.Add(new SolidViolation
            {
                Principle = SolidPrinciple.SRP,
                Severity = methodCount > SrpMaxMethods * 2 ? IssueSeverity.Error : IssueSeverity.Warning,
                Location = new CodeLocation { FilePath = filePath, Line = line, MemberName = className },
                Description = $"Class '{className}' has {methodCount} methods (threshold: {SrpMaxMethods}), suggesting multiple responsibilities.",
                SuggestedFix = $"Consider splitting '{className}' into smaller classes, each with a single responsibility."
            });

            response.Suggestions.Add(new RefactoringSuggestion
            {
                RefactoringType = "ExtractClass",
                Description = $"Split '{className}' into cohesive classes based on method grouping.",
                Impact = 0.8
            });
        }

        if (fieldCount > SrpMaxFields)
        {
            response.SolidViolations.Add(new SolidViolation
            {
                Principle = SolidPrinciple.SRP,
                Severity = IssueSeverity.Warning,
                Location = new CodeLocation { FilePath = filePath, Line = line, MemberName = className },
                Description = $"Class '{className}' has {fieldCount} fields (threshold: {SrpMaxFields}), suggesting it manages too much state.",
                SuggestedFix = "Group related fields into separate data objects or extract collaborating classes."
            });
        }
    }

    private void DetectOcpViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        var switchPattern = new Regex(@"^\s*switch\s*\(");
        var casePattern = new Regex(@"^\s*case\s+");
        var ifElseChainPattern = new Regex(@"^\s*else\s+if\s*\(");

        int consecutiveIfElse = 0;
        int switchStartLine = -1;
        int caseCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            // Detect long switch statements
            if (switchPattern.IsMatch(lines[i]))
            {
                switchStartLine = i + 1;
                caseCount = 0;
            }

            if (switchStartLine >= 0 && casePattern.IsMatch(lines[i]))
            {
                caseCount++;
            }

            if (switchStartLine >= 0 && lines[i].Trim() == "}")
            {
                if (caseCount >= OcpSwitchCaseThreshold)
                {
                    response.SolidViolations.Add(new SolidViolation
                    {
                        Principle = SolidPrinciple.OCP,
                        Severity = IssueSeverity.Warning,
                        Location = new CodeLocation { FilePath = filePath, Line = switchStartLine },
                        Description = $"Switch statement with {caseCount} cases (threshold: {OcpSwitchCaseThreshold}). Adding new cases requires modifying existing code.",
                        SuggestedFix = "Replace with a strategy pattern or polymorphic dispatch using a dictionary of handlers."
                    });

                    response.Suggestions.Add(new RefactoringSuggestion
                    {
                        RefactoringType = "ReplaceConditionalWithPolymorphism",
                        Description = $"Replace {caseCount}-case switch with strategy pattern.",
                        Impact = 0.7
                    });
                }
                switchStartLine = -1;
            }

            // Detect long if/else-if chains
            if (ifElseChainPattern.IsMatch(lines[i]))
            {
                consecutiveIfElse++;
            }
            else if (!lines[i].Trim().StartsWith("else") && !lines[i].Trim().StartsWith("{") && !lines[i].Trim().StartsWith("}") && lines[i].Trim().Length > 0)
            {
                if (consecutiveIfElse >= OcpSwitchCaseThreshold)
                {
                    response.SolidViolations.Add(new SolidViolation
                    {
                        Principle = SolidPrinciple.OCP,
                        Severity = IssueSeverity.Warning,
                        Location = new CodeLocation { FilePath = filePath, Line = i + 1 - consecutiveIfElse },
                        Description = $"If/else-if chain with {consecutiveIfElse + 1} branches. Adding new conditions requires modifying existing code.",
                        SuggestedFix = "Replace with a chain of responsibility or strategy pattern."
                    });
                }
                consecutiveIfElse = 0;
            }
        }
    }

    private void DetectLspViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        var notImplPattern = new Regex(@"throw\s+new\s+NotImplementedException\s*\(");
        var notSupportedPattern = new Regex(@"throw\s+new\s+NotSupportedException\s*\(");
        var emptyOverridePattern = new Regex(@"^\s*(?:public|protected)\s+override\s+\w+\s+(\w+)\s*\([^)]*\)\s*\{\s*\}\s*$");

        for (int i = 0; i < lines.Length; i++)
        {
            if (notImplPattern.IsMatch(lines[i]))
            {
                response.SolidViolations.Add(new SolidViolation
                {
                    Principle = SolidPrinciple.LSP,
                    Severity = IssueSeverity.Error,
                    Location = new CodeLocation { FilePath = filePath, Line = i + 1 },
                    Description = "Method throws NotImplementedException, indicating the subtype does not fully support the base type contract.",
                    SuggestedFix = "Implement the method or redesign the type hierarchy so this method is not required."
                });
            }

            if (notSupportedPattern.IsMatch(lines[i]))
            {
                response.SolidViolations.Add(new SolidViolation
                {
                    Principle = SolidPrinciple.LSP,
                    Severity = IssueSeverity.Warning,
                    Location = new CodeLocation { FilePath = filePath, Line = i + 1 },
                    Description = "Method throws NotSupportedException, suggesting the type cannot fully substitute for its base type.",
                    SuggestedFix = "Consider splitting the interface so this type only implements the operations it supports."
                });
            }

            if (emptyOverridePattern.IsMatch(lines[i]))
            {
                var match = emptyOverridePattern.Match(lines[i]);
                response.SolidViolations.Add(new SolidViolation
                {
                    Principle = SolidPrinciple.LSP,
                    Severity = IssueSeverity.Info,
                    Location = new CodeLocation { FilePath = filePath, Line = i + 1, MemberName = match.Groups[1].Value },
                    Description = $"Empty override of '{match.Groups[1].Value}' silently changes base class behavior.",
                    SuggestedFix = "Ensure the empty override is intentional. Document why the base behavior is suppressed."
                });
            }
        }
    }

    private void DetectIspViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        var interfacePattern = new Regex(@"^\s*(?:public|internal)?\s*interface\s+(\w+)");
        var interfaceMethodPattern = new Regex(@"^\s*(?:Task|void|bool|int|string|float|double|decimal|IEnumerable|IReadOnlyList|IList)\s*(?:<[^>]+>)?\s+\w+\s*\(");

        string? currentInterface = null;
        int interfaceStartLine = 0;
        int methodCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var ifaceMatch = interfacePattern.Match(lines[i]);
            if (ifaceMatch.Success)
            {
                if (currentInterface != null && methodCount > IspMaxInterfaceMethods)
                {
                    response.SolidViolations.Add(new SolidViolation
                    {
                        Principle = SolidPrinciple.ISP,
                        Severity = IssueSeverity.Warning,
                        Location = new CodeLocation { FilePath = filePath, Line = interfaceStartLine, MemberName = currentInterface },
                        Description = $"Interface '{currentInterface}' has {methodCount} methods (threshold: {IspMaxInterfaceMethods}). Clients may be forced to depend on methods they don't use.",
                        SuggestedFix = $"Split '{currentInterface}' into smaller, role-specific interfaces."
                    });

                    response.Suggestions.Add(new RefactoringSuggestion
                    {
                        RefactoringType = "ExtractInterface",
                        Description = $"Split '{currentInterface}' into cohesive sub-interfaces.",
                        Impact = 0.6
                    });
                }

                currentInterface = ifaceMatch.Groups[1].Value;
                interfaceStartLine = i + 1;
                methodCount = 0;
            }

            if (currentInterface != null && interfaceMethodPattern.IsMatch(lines[i]))
            {
                methodCount++;
            }
        }

        // Check the last interface
        if (currentInterface != null && methodCount > IspMaxInterfaceMethods)
        {
            response.SolidViolations.Add(new SolidViolation
            {
                Principle = SolidPrinciple.ISP,
                Severity = IssueSeverity.Warning,
                Location = new CodeLocation { FilePath = filePath, Line = interfaceStartLine, MemberName = currentInterface },
                Description = $"Interface '{currentInterface}' has {methodCount} methods (threshold: {IspMaxInterfaceMethods}). Clients may be forced to depend on methods they don't use.",
                SuggestedFix = $"Split '{currentInterface}' into smaller, role-specific interfaces."
            });
        }
    }

    private void DetectDipViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        // Detect direct instantiation of concrete types outside of constructors/factories
        var newConcretePattern = new Regex(@"=\s*new\s+([A-Z]\w+(?:Client|Service|Repository|Manager|Handler|Provider|Engine|Adapter))\s*\(");
        var constructorPattern = new Regex(@"^\s*(?:public|private|protected|internal)\s+\w+\s*\(");
        var factoryMethodPattern = new Regex(@"^\s*(?:public|private|protected|internal)\s+static\s+");

        bool inConstructorOrFactory = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (constructorPattern.IsMatch(lines[i]) || factoryMethodPattern.IsMatch(lines[i]))
            {
                inConstructorOrFactory = true;
            }

            if (inConstructorOrFactory && lines[i].Trim() == "}")
            {
                inConstructorOrFactory = false;
            }

            if (!inConstructorOrFactory)
            {
                var match = newConcretePattern.Match(lines[i]);
                if (match.Success)
                {
                    var typeName = match.Groups[1].Value;
                    response.SolidViolations.Add(new SolidViolation
                    {
                        Principle = SolidPrinciple.DIP,
                        Severity = IssueSeverity.Warning,
                        Location = new CodeLocation { FilePath = filePath, Line = i + 1 },
                        Description = $"Direct instantiation of concrete type '{typeName}'. High-level modules should depend on abstractions.",
                        SuggestedFix = $"Inject '{typeName}' through the constructor using its interface (e.g., 'I{typeName}')."
                    });

                    response.Suggestions.Add(new RefactoringSuggestion
                    {
                        RefactoringType = "InjectDependency",
                        Description = $"Replace 'new {typeName}(...)' with constructor injection of 'I{typeName}'.",
                        Impact = 0.7
                    });
                }
            }
        }
    }

    private void DetectDryViolations(string[] lines, string filePath, CodeAnalysisResponse response)
    {
        // Find duplicate blocks of code (sequences of N+ similar lines)
        var normalizedLines = lines.Select(NormalizeLine).ToArray();
        var duplicates = new HashSet<string>();

        for (int blockSize = DryMinDuplicateLineLength; blockSize <= Math.Min(20, lines.Length / 2); blockSize++)
        {
            for (int i = 0; i <= normalizedLines.Length - blockSize; i++)
            {
                var block = string.Join("\n", normalizedLines.Skip(i).Take(blockSize));
                if (string.IsNullOrWhiteSpace(block) || block.Length < 30)
                    continue;

                for (int j = i + blockSize; j <= normalizedLines.Length - blockSize; j++)
                {
                    var candidate = string.Join("\n", normalizedLines.Skip(j).Take(blockSize));
                    if (string.IsNullOrWhiteSpace(candidate))
                        continue;

                    var similarity = CalculateSimilarity(block, candidate);
                    if (similarity >= DryMinSimilarity)
                    {
                        var key = $"{i}:{j}:{blockSize}";
                        if (duplicates.Add(key))
                        {
                            var snippet = string.Join("\n", lines.Skip(i).Take(Math.Min(blockSize, 5)));
                            response.DryViolations.Add(new DryViolation
                            {
                                Severity = blockSize >= 10 ? IssueSeverity.Error : IssueSeverity.Warning,
                                Locations = new List<CodeLocation>
                                {
                                    new() { FilePath = filePath, Line = i + 1 },
                                    new() { FilePath = filePath, Line = j + 1 }
                                },
                                DuplicatedPattern = snippet.Length > 200 ? snippet[..200] + "..." : snippet,
                                Description = $"Duplicated code block ({blockSize} lines, {similarity:P0} similar) found at lines {i + 1} and {j + 1}.",
                                SuggestedAbstraction = "Extract the duplicated logic into a shared method or base class."
                            });

                            response.Suggestions.Add(new RefactoringSuggestion
                            {
                                RefactoringType = "ExtractMethod",
                                Description = $"Extract duplicated {blockSize}-line block into a shared method.",
                                Impact = 0.6 + (blockSize * 0.02)
                            });
                        }
                    }
                }
            }
        }
    }

    private static string NormalizeLine(string line)
    {
        // Normalize whitespace and remove comments for comparison
        var trimmed = line.Trim();
        if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
            return string.Empty;
        return Regex.Replace(trimmed, @"\s+", " ");
    }

    private static double CalculateSimilarity(string a, string b)
    {
        if (a == b) return 1.0;
        if (a.Length == 0 || b.Length == 0) return 0.0;

        // Use Jaccard similarity on character trigrams for efficiency
        var trigramsA = GetTrigrams(a);
        var trigramsB = GetTrigrams(b);

        var intersection = trigramsA.Intersect(trigramsB).Count();
        var union = trigramsA.Union(trigramsB).Count();

        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static HashSet<string> GetTrigrams(string text)
    {
        var trigrams = new HashSet<string>();
        for (int i = 0; i <= text.Length - 3; i++)
        {
            trigrams.Add(text.Substring(i, 3));
        }
        return trigrams;
    }

    private static double CalculateOverallScore(CodeAnalysisResponse response)
    {
        double score = 1.0;

        foreach (var v in response.SolidViolations)
        {
            score -= v.Severity switch
            {
                IssueSeverity.Error => 0.15,
                IssueSeverity.Warning => 0.08,
                IssueSeverity.Info => 0.03,
                _ => 0.0
            };
        }

        foreach (var v in response.DryViolations)
        {
            score -= v.Severity switch
            {
                IssueSeverity.Error => 0.12,
                IssueSeverity.Warning => 0.06,
                _ => 0.02
            };
        }

        return Math.Max(0.0, Math.Min(1.0, score));
    }

    private static string BuildSummary(CodeAnalysisResponse response)
    {
        var solidCount = response.SolidViolations.Count;
        var dryCount = response.DryViolations.Count;
        var totalIssues = solidCount + dryCount;

        if (totalIssues == 0)
            return "No SOLID or DRY violations detected. The code follows good design principles.";

        var parts = new List<string>();
        if (solidCount > 0)
        {
            var principles = response.SolidViolations
                .Select(v => v.Principle)
                .Distinct()
                .OrderBy(p => p);
            parts.Add($"{solidCount} SOLID violation(s) ({string.Join(", ", principles)})");
        }

        if (dryCount > 0)
        {
            parts.Add($"{dryCount} DRY violation(s)");
        }

        var scoreLabel = response.OverallScore switch
        {
            >= 0.9 => "excellent",
            >= 0.7 => "good",
            >= 0.5 => "needs improvement",
            _ => "poor"
        };

        return $"Found {string.Join(" and ", parts)}. Overall quality: {scoreLabel} ({response.OverallScore:P0}). {response.Suggestions.Count} refactoring suggestion(s) available.";
    }
}
