using System.Reflection;

namespace SoundMate.API;

/// <summary>
/// What this build calls itself, read once from the assembly attributes the SDK stamps from
/// <c>Directory.Build.props</c>.
/// <para>
/// Reading it rather than repeating it is the point: the version exists in exactly one place, and
/// everything that shows it — the OpenAPI document, <c>GET /api/version</c> — reads the same
/// value. A hardcoded string here would drift the first time somebody bumped the props file.
/// </para>
/// </summary>
internal static class BuildInfo
{
    /// <summary>e.g. "0.2.0". Build metadata is stripped; see <see cref="ReadVersion"/>.</summary>
    public static string Version { get; } = ReadVersion();

    /// <summary>The <c>Product</c> property from Directory.Build.props.</summary>
    public static string Product { get; } =
        Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "SoundMate";

    private static Assembly Assembly => typeof(BuildInfo).Assembly;

    private static string ReadVersion()
    {
        var informational = Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
            return Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

        // The SDK appends "+<commit sha>" when source link is on. That is build metadata, not part
        // of the version anybody asks about, so it is trimmed.
        var buildMetadata = informational.IndexOf('+');

        return buildMetadata < 0 ? informational : informational[..buildMetadata];
    }
}
