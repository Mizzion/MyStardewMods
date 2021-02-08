using System.Collections.Generic;

namespace OneSprinklerOneScarecrow.Framework
{
    public interface IJsonAssetsApi
    {
        IDictionary<string, int> GetAllObjectIds();
        IDictionary<string, int> GetAllBigCraftableIds();
    }
}