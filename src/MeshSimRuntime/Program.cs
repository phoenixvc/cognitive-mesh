using System.CommandLine;
using System.Text.Json;
using MetacognitiveLayer.Protocols.Common.Memory;
using MetacognitiveLayer.Protocols.Common.Orchestration;
using MetacognitiveLayer.Protocols.Common.Templates;
using MetacognitiveLayer.Protocols.Common.Tools;
using MetacognitiveLayer.Protocols.Integration;
using MetacognitiveLayer.Protocols.LLM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IAgentOrchestrator = MetacognitiveLayer.Protocols.Common.Orchestration.IAgentOrchestrator;
using IContextTemplateResolver = MetacognitiveLayer.Protocols.Common.Templates.IContextTemplateResolver;
using IMeshMemoryStore = MetacognitiveLayer.Protocols.Common.Memory.IMeshMemoryStore;
using IToolRunner = MetacognitiveLayer.Protocols.Common.Tools.IToolRunner;

namespace CognitiveMesh.MeshSimRuntime
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Create command line interface
            var rootCommand = new RootCommand("MeshSimRuntime - Cognitive Mesh Simulation Runtime");

            // Execute command
            var agentOption = new Option<string>("--agent", "Agent ID to execute");
            var taskOption = new Option<string>("--task", "Task name to execute");
            var paramsOption = new Option<string>("--params", "JSON parameters for the task");
            var outputOption = new Option<string>("--output", "Output file path (optional)");

            var executeCommand = new Command("execute", "Execute an agent task")
            {
                agentOption, taskOption, paramsOption, outputOption
            };
            executeCommand.SetAction(async (parseResult, ct) =>
            {
                var agent = parseResult.GetValue(agentOption) ?? "";
                var task = parseResult.GetValue(taskOption) ?? "";
                var @params = parseResult.GetValue(paramsOption) ?? "";
                var output = parseResult.GetValue(outputOption) ?? "";
                await ExecuteAgentHandler(agent, task, @params, output);
            });
            rootCommand.Add(executeCommand);

            // Register command
            var idOption = new Option<string>("--id", "Agent ID");
            var typeOption = new Option<string>("--type", "Agent type");
            var configOption = new Option<string>("--config", "JSON configuration for the agent");

            var registerCommand = new Command("register", "Register a new agent")
            {
                idOption, typeOption, configOption
            };
            registerCommand.SetAction(async (parseResult, ct) =>
            {
                var id = parseResult.GetValue(idOption) ?? "";
                var type = parseResult.GetValue(typeOption) ?? "";
                var config = parseResult.GetValue(configOption) ?? "";
                await RegisterAgentHandler(id, type, config);
            });
            rootCommand.Add(registerCommand);

            // List command
            var listCommand = new Command("list", "List available agents");
            listCommand.SetAction(async (parseResult, ct) =>
            {
                await ListAgentsHandler();
            });
            rootCommand.Add(listCommand);

            // Parse and execute
            return await rootCommand.Parse(args).InvokeAsync();
        }

        static async Task<int> ExecuteAgentHandler(string agent, string task, string @params, string output)
        {
            try
            {
                Console.WriteLine($"Executing agent: {agent}, task: {task}");

                // Set up services
                var services = ConfigureServices();

                // Get orchestrator
                var orchestrator = services.GetRequiredService<IAgentOrchestrator>();

                // Parse parameters
                var parameters = string.IsNullOrEmpty(@params)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(@params);

                // Create request
                var request = new ACPRequest
                {
                    AgentId = agent,
                    TaskName = task,
                    Parameters = parameters ?? new Dictionary<string, object>()
                };

                // Execute
                var result = await orchestrator.ExecuteAgentAsync(request);

                // Output result
                if (result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Execution successful!");
                    Console.ResetColor();

                    Console.WriteLine($"Execution time: {result.ExecutionTimeMs}ms");
                    Console.WriteLine();
                    Console.WriteLine("Output:");
                    Console.WriteLine(result.Output);

                    // Save to file if requested
                    if (!string.IsNullOrEmpty(output))
                    {
                        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                        Console.WriteLine($"Result saved to {output}");
                    }

                    return 0;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Execution failed!");
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                    Console.ResetColor();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        static async Task<int> RegisterAgentHandler(string id, string type, string config)
        {
            try
            {
                Console.WriteLine($"Registering agent: {id}, type: {type}");

                // Set up services
                var services = ConfigureServices();

                // Get orchestrator
                var orchestrator = services.GetRequiredService<IAgentOrchestrator>();

                // Parse configuration
                var configuration = string.IsNullOrEmpty(config)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(config);

                // Register
                var success = await orchestrator.RegisterAgentAsync(id, type, configuration ?? new Dictionary<string, object>());

                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Agent registered successfully!");
                    Console.ResetColor();
                    return 0;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to register agent!");
                    Console.ResetColor();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        static async Task<int> ListAgentsHandler()
        {
            try
            {
                Console.WriteLine("Listing available agents...");

                // Set up services
                var services = ConfigureServices();

                // Get orchestrator
                var orchestrator = services.GetRequiredService<IAgentOrchestrator>();

                // Get agents
                var agents = await orchestrator.GetAvailableAgentsAsync();

                if (agents.Count == 0)
                {
                    Console.WriteLine("No agents registered.");
                }
                else
                {
                    Console.WriteLine($"Found {agents.Count} agent(s):");
                    foreach (var agent in agents)
                    {
                        Console.WriteLine($"- {agent.Key} (Type: {agent.Value})");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add memory store
            services.AddSingleton<IMeshMemoryStore>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var duckDbLogger = loggerFactory.CreateLogger<DuckDbMemoryStore>();
                var duckDbStore = new DuckDbMemoryStore("data/mesh_memory.duckdb", duckDbLogger);

                // Initialize during startup
                duckDbStore.InitializeAsync().GetAwaiter().GetResult();

                return duckDbStore;
            });

            // Add template resolver
            services.AddSingleton<IContextTemplateResolver>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ContextTemplateResolver>();
                return new ContextTemplateResolver("templates", logger);
            });

            // Add LLM provider (mock for now)
            services.AddSingleton<ILLMProvider>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<MockLLMProvider>();
                return new MockLLMProvider(logger);
            });

            // Add tool runner
            services.AddSingleton<IToolRunner>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<NodeToolRunner>();
                var options = new NodeToolRunnerOptions
                {
                    ToolsDirectory = "tools/node",
                    UseTypeScript = true
                };
                var runner = new NodeToolRunner(options, logger);

                // Initialize during startup
                runner.InitializeAsync().GetAwaiter().GetResult();

                return runner;
            });

            // Add orchestrator
            services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();

            return services.BuildServiceProvider();
        }
    }
}
