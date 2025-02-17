namespace PHC_HighCounsel_Bot.Common.EmbedStyles;

/// <summary>
/// Represents an abstract class for defining embed styles.
/// </summary>
public abstract class EmbedStyle
{
    /// <summary>
    /// Gets or sets the name of the embed style.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets or sets the icon URL of the embed style.
    /// </summary>
    public abstract string IconUrl { get; }

    /// <summary>
    /// Gets or sets the color of the embed style.
    /// </summary>
    public abstract Color Color { get; }
}
