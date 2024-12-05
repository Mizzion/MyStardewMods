namespace FarmHelper.Framework;

public class PickAxeSettings
{
    /// <summary>Whether to clear debris.</summary>
    public bool ClearDebris { get; set; } = true;

    /// <summary>Whether to clear dead crops.</summary>
    public bool ClearDeadCrops { get; set; } = true;

    /// <summary>Whether to clear tilled dirt.</summary>
    public bool ClearDirt { get; set; } = true;

    /// <summary>Whether to clear placed flooring.</summary>
    public bool ClearFlooring { get; set; } = false;

    /// <summary>Whether to clear boulders and meteorites.</summary>
    public bool ClearBouldersAndMeteorites { get; set; } = true;

    /// <summary>Whether to clear placed objects.</summary>
    public bool ClearObjects { get; set; } = false;

    /// <summary>Whether to break containers in the mine.</summary>
    public bool BreakMineContainers { get; set; } = true;

    /// <summary>Whether to clear weeds.</summary>
    public bool ClearWeeds { get; set; } = true;

    /// <summary>Whether to harvest spawned items in the mines.</summary>
    public bool HarvestMineSpawns { get; set; } = true;
}