namespace FarmHelper.Framework;

public class HoeSettings
{
    /// <summary>Whether to till empty dirt.</summary>
    public bool TillDirt { get; set; } = true;

    /// <summary>Whether to clear weeds.</summary>
    public bool ClearWeeds { get; set; } = true;

    /// <summary>Whether to dig artifact spots.</summary>
    public bool DigArtifactSpots { get; set; } = true;

    /// <summary>Whether to dig seed spots.</summary>
    public bool DigSeedSpots { get; set; } = true;

    /// <summary>Whether to harvest spawned ginger.</summary>
    public bool HarvestGinger { get; set; } = true;
}