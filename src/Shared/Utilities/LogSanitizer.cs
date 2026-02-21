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

        // Fast path: check if sanitization is actually needed
        bool needsSanitization = false;
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsControl(value[i]))
            {
                needsSanitization = true;
                break;
            }
        }

        if (!needsSanitization)
        {
            return value;
        }

        return string.Create(value.Length, value, static (span, src) =>
        {
            for (int i = 0; i < src.Length; i++)
            {
                span[i] = char.IsControl(src[i]) ? '_' : src[i];
            }
        });
    }
}
