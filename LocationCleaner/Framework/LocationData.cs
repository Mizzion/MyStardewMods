using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LocationCleaner.Framework
{
    internal class LocationData
    {
        public GameLocation location { get; set; }
        public IList<SObject> locObj { get; set; }
        public IList<TerrainFeature> locTerrain { get; set; }
        public IList<ResourceClump> locResource { get; set; }

        public LocationData(string locationName)
        {
            location = Game1.getLocationFromName(locationName);
            locObj = location.objects.Values.ToList();
            locTerrain = location.terrainFeatures.Values.ToList();

            var resourceClumps =
                (location as Farm)?.resourceClumps
                ?? (IList<ResourceClump>)(location as Woods)?.stumps
                ?? new List<ResourceClump>();
            locResource = resourceClumps;
        }
    }
}
