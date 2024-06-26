﻿using System.Collections.Generic;
using System.Security;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Increased_Artifact_Spots
{
    public class ModConfig
    {
        //Roughly how many artifact spots to spawn
        public int AverageArtifactSpots { get; set; } = 50;

        //public bool ForceAverageArtifacts { get; set; } = false;

        //Whether or not to show the amount of artifacts spots spawned in the game.
        public bool ShowSpawnedNumArtifactSpots { get; set; } = true;

        public bool SpawnArtifactsOnFarm { get; set; } = false;

        public ArtifactSpots ValidDefaultSpots { get; set; } = new ArtifactSpots();
    }

    public class ArtifactSpots
    {
        public Dictionary<string, List<Vector2>> Locations { get; set; } = new();

    }
}
