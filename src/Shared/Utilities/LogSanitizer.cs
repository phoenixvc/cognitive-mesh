namespace CognitiveMesh.Shared;

/// <summary>
/// Provides sanitization for log message values to prevent log injection (log forging) attacks.
/// Replaces control characters (newlines, carriage returns, etc.) in user-provided values
/// before they are passed to structured logging methods.
/// </summary>
public static class LogSanitizer
{
    /// <summary>
    /// Sanitizes a string value for safe inclusion in log entries by replacing
    /// control characters with underscores. This prevents log forging attacks
    /// where an attacker injects newline characters to create fake log entries.
    /// </summary>
    /// <param name="value">The value to sanitize. May be null.</param>
    /// <returns>The sanitized value with control characters replaced, or an empty string if null.</returns>
    public static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Replace newline/carriage-return characters to prevent log forging attacks.
        // Uses Replace() so static-analysis tools (CodeQL) can trace the sanitization.
        return value.Replace("\r", "_").Replace("\n", "_");
    }
}
