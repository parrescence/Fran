namespace FactoryAspects.Components;

/// <summary><see cref="FaModal"/>'s max-width. Height always caps at 90vh and scrolls internally regardless of size.</summary>
public enum FaModalSize
{
    Small,
    /// <summary>The default — same 520px max-width FaModal always used before this parameter existed.</summary>
    Medium,
    Large
}
