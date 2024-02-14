using System;
using System.Collections.Generic;
using System.Text;

namespace Mizzion.Stardew.Common.Integrations
{
    internal interface IModIntegration
    {
        /*********
         ** Accessors
         *********/
        /// <summary>A human-readable name for the mod.</summary>
        string Label { get; }

        /// <summary>Whether the mod is available.</summary>
        bool IsLoaded { get; }
    }
}
