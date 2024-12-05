﻿namespace FarmHelper.Framework;

public class WeaponSettings
{
    /// <summary>Whether to attack monsters.</summary>
    public bool AttackMonsters { get; set; } = false;

    /// <summary>Whether to clear dead crops.</summary>
    public bool ClearDeadCrops { get; set; } = true;

    /// <summary>Whether to break containers in the mine.</summary>
    public bool BreakMineContainers { get; set; } = true;

    /// <summary>Whether to cut tall grass.</summary>
    public bool HarvestGrass { get; set; } = false;
}