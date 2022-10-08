﻿namespace LabelVoice.Core.Utils;

public static class CodeGenerator
{
    private static readonly HashSet<string> _registered = new();

    /// <summary>
    /// Register a new code.
    /// </summary>
    /// <returns><see langword="true"/> if the given <paramref name="code"/> is successfully registered, or <see langword="false"/> if the code already exists.</returns>
    public static bool Register(string code)
    {
        return _registered.Add(code);
    }

    /// <summary>
    /// Generate and register a new unique hex code with given <paramref name="length"/>.
    /// </summary>
    /// <returns>A new code which has never been registered before.</returns>
    public static string Generate(int length)
    {
        var maxAttempts = 1 << (length * 4 - (length > 2 ? length : 0));

        for (var attempt = 0; attempt < maxAttempts; ++attempt)
        {
            var code = GenerateOnce(length);
            if (_registered.Add(code))
            {
                return code;
            }
        }

        throw new ArgumentException($"Failed to generate a unique code with this length ({length}) after {maxAttempts} attempts.", nameof(length));
    }

    private static string GenerateOnce(int length)
    {
        return ((uint)new Random().Next(int.MinValue, int.MaxValue)).ToString($"x{length}");
    }
}
