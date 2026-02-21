using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Tools
{
    /// <summary>
    /// Configuration options for the Node tool runner.
    /// </summary>
    public class NodeToolRunnerOptions
    {
        /// <summary>
        /// Base directory for Node.js tools
        /// </summary>
        public string ToolsDirectory { get; set; } = "tools/node";

        /// <summary>
        /// Path to Node.js executable
        /// </summary>
        public string NodePath { get; set; } = "node";

        /// <summary>
        /// Maximum execution time in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Whether to use TypeScript directly (requires ts-node)
        /// </summary>
        public bool UseTypeScript { get; set; } = false;
    }

    /// <summary>
    /// Tool runner implementation for executing JavaScript/TypeScript tools using Node.js.
    /// </summary>
    public class NodeToolRunner : IToolRunner
    {
        private readonly ILogger<NodeToolRunner> _logger;
        private readonly NodeToolRunnerOptions _options;
        private readonly Dictionary<string, string> _toolCache = new Dictionary<string, string>();
        private bool _initialized = false;

        public NodeToolRunner(NodeToolRunnerOptions options, ILogger<NodeToolRunner> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the tool runner by scanning for available tools.
        /// </summary>
        public Task InitializeAsync()
        {
            if (_initialized)
                return Task.CompletedTask;

            _logger.LogInformation("Initializing Node tool runner");

            try
            {
                // Ensure tools directory exists
                if (!Directory.Exists(_options.ToolsDirectory))
                {
                    Directory.CreateDirectory(_options.ToolsDirectory);
                }

                // Scan for JavaScript and TypeScript tools
                var toolFiles = Directory.GetFiles(_options.ToolsDirectory, "*.js", SearchOption.AllDirectories);
                if (_options.UseTypeScript)
                {
                    var tsFiles = Directory.GetFiles(_options.ToolsDirectory, "*.ts", SearchOption.AllDirectories);
                    toolFiles = toolFiles.Concat(tsFiles).ToArray();
                }

                // Cache tool paths
                foreach (var toolFile in toolFiles)
                {
                    var toolId = Path.GetFileNameWithoutExtension(toolFile);
                    _toolCache[toolId] = toolFile;
                }

                _logger.LogInformation("Node tool runner initialized with {Count} tools", _toolCache.Count);
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Node tool runner");
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes a Node.js tool with the provided inputs.
        /// </summary>
        public async Task<object> Execute(string toolId, Dictionary<string, object> input, ToolContext context)
        {
            if (!_initialized)
                await InitializeAsync();

            _logger.LogInformation("Executing Node tool: {ToolId}", toolId);

            try
            {
                // Check if tool exists
                if (!_toolCache.TryGetValue(toolId, out var toolPath) && !await ValidateToolAsync(toolId))
                {
                    throw new KeyNotFoundException($"Tool not found: {toolId}");
                }

                // Use cached path or find it
                toolPath ??= Path.Combine(_options.ToolsDirectory, $"{toolId}.{(_options.UseTypeScript ? "ts" : "js")}");

                // Prepare the execution environment
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var nodeExecutable = _options.NodePath;
                
                // Use ts-node for TypeScript files if enabled
                string command;
                var args = new List<string>();
                
                if (_options.UseTypeScript && toolPath.EndsWith(".ts"))
                {
                    command = "npx";
                    args.Add("ts-node");
                    args.Add(toolPath);
                }
                else
                {
                    command = nodeExecutable;
                    args.Add(toolPath);
                }

                // Create JSON input for the tool
                var toolInput = new Dictionary<string, object>(input)
                {
                    ["context"] = new
                    {
                        sessionId = context.SessionId,
                        userId = context.UserId,
                        additionalContext = context.AdditionalContext
                    }
                };

                // Convert input to JSON and prepare for stdin
                var inputJson = JsonSerializer.Serialize(toolInput);
                
                // Start the process
                using (var process = new Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = string.Join(" ", args);
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data != null) outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (e.Data != null) errorBuilder.AppendLine(e.Data);
                    };

                    _logger.LogDebug("Starting Node process for tool: {ToolId}", toolId);
                    
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Write input to stdin
                    await process.StandardInput.WriteLineAsync(inputJson);
                    process.StandardInput.Close();

                    // Wait for process with timeout
                    if (!process.WaitForExit(_options.TimeoutMs))
                    {
                        process.Kill();
                        throw new TimeoutException($"Tool execution timed out after {_options.TimeoutMs}ms: {toolId}");
                    }

                    var output = outputBuilder.ToString().Trim();
                    var error = errorBuilder.ToString().Trim();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Tool execution failed: {ToolId}, Exit code: {ExitCode}, Error: {Error}",
                            toolId, process.ExitCode, error);
                        throw new InvalidOperationException($"Tool execution failed with exit code {process.ExitCode}: {error}");
                    }

                    if (string.IsNullOrEmpty(output))
                    {
                        _logger.LogWarning("Tool returned empty output: {ToolId}", toolId);
                        return new { success = true, message = "Tool executed successfully but returned no output" };
                    }

                    try
                    {
                        // Parse the output as JSON
                        var result = JsonSerializer.Deserialize<object>(output);
                        return result;
                    }
                    catch (JsonException)
                    {
                        // Return raw output if it's not valid JSON
                        return new { output = output, raw = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Node tool: {ToolId}", toolId);
                throw;
            }
        }

        /// <summary>
        /// Gets the available tools managed by this runner.
        /// </summary>
        public async Task<Dictionary<string, object>> GetAvailableToolsAsync()
        {
            if (!_initialized)
                await InitializeAsync();
                
            var tools = new Dictionary<string, object>();
            
            foreach (var toolEntry in _toolCache)
            {
                try
                {
                    // Get tool metadata by invoking with --info flag
                    var toolInfo = await GetToolMetadataAsync(toolEntry.Key);
                    tools[toolEntry.Key] = toolInfo;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting metadata for tool: {ToolId}", toolEntry.Key);
                    tools[toolEntry.Key] = new { 
                        id = toolEntry.Key,
                        path = toolEntry.Value,
                        error = "Failed to get tool metadata"
                    };
                }
            }

            return tools;
        }

        /// <summary>
        /// Validates if a tool exists and can be executed.
        /// </summary>
        public async Task<bool> ValidateToolAsync(string toolId)
        {
            if (!_initialized)
                await InitializeAsync();
                
            try
            {
                // Check cache first
                if (_toolCache.ContainsKey(toolId))
                {
                    return true;
                }

                // Check if tool file exists
                var jsPath = Path.Combine(_options.ToolsDirectory, $"{toolId}.js");
                var tsPath = Path.Combine(_options.ToolsDirectory, $"{toolId}.ts");

                bool exists = File.Exists(jsPath) || (_options.UseTypeScript && File.Exists(tsPath));
                
                if (exists)
                {
                    // Add to cache
                    _toolCache[toolId] = File.Exists(jsPath) ? jsPath : tsPath;
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tool: {ToolId}", toolId);
                return false;
            }
        }

        /// <summary>
        /// Gets metadata for a tool by invoking it with the --info flag.
        /// </summary>
        private Task<object> GetToolMetadataAsync(string toolId)
        {
            try
            {
                if (!_toolCache.TryGetValue(toolId, out var toolPath))
                {
                    throw new KeyNotFoundException($"Tool not found: {toolId}");
                }

                // Prepare the process
                var nodeExecutable = _options.NodePath;

                string command;
                var args = new List<string>();

                if (_options.UseTypeScript && toolPath.EndsWith(".ts"))
                {
                    command = "npx";
                    args.Add("ts-node");
                    args.Add(toolPath);
                }
                else
                {
                    command = nodeExecutable;
                    args.Add(toolPath);
                }

                args.Add("--info");

                using (var process = new Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = string.Join(" ", args);
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data != null) outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (e.Data != null) errorBuilder.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(_options.TimeoutMs))
                    {
                        process.Kill();
                        throw new TimeoutException($"Tool info request timed out: {toolId}");
                    }

                    var output = outputBuilder.ToString().Trim();

                    if (process.ExitCode != 0 || string.IsNullOrEmpty(output))
                    {
                        // Default metadata if not available
                        object defaultMeta = new
                        {
                            id = toolId,
                            path = toolPath,
                            type = toolPath.EndsWith(".ts") ? "typescript" : "javascript",
                            description = "No metadata available"
                        };
                        return Task.FromResult(defaultMeta);
                    }

                    try
                    {
                        // Try to parse the output as JSON
                        object? parsed = JsonSerializer.Deserialize<object>(output);
                        return Task.FromResult(parsed ?? (object)new { id = toolId, description = output });
                    }
                    catch
                    {
                        // If not valid JSON, return as description
                        object fallbackMeta = new
                        {
                            id = toolId,
                            path = toolPath,
                            type = toolPath.EndsWith(".ts") ? "typescript" : "javascript",
                            description = output
                        };
                        return Task.FromResult(fallbackMeta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tool metadata: {ToolId}", toolId);
                throw;
            }
        }
    }

    /// <summary>
    /// Factory for creating tool runners.
    /// </summary>
    public static class ToolRunnerFactory
    {
        /// <summary>
        /// Registers tool runner services with the dependency injection container.
        /// </summary>
        public static IServiceCollection AddNodeToolRunner(this IServiceCollection services, Action<NodeToolRunnerOptions>? configureOptions = null)
        {
            // Register options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<NodeToolRunnerOptions>(options => { });
            }

            // Register NodeToolRunner as a singleton
            services.AddSingleton<NodeToolRunner>();

            // Register as IToolRunner if it's the default
            services.AddSingleton<IToolRunner>(provider => provider.GetRequiredService<NodeToolRunner>());

            return services;
        }
    }
}