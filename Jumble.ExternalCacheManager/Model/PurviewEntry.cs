using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Model
{
    /// <summary>
    /// Holds metadata for mapping an application (purview) to a reserved directory location where cached data files
    /// for the application are stored.
    /// </summary>
    public class PurviewEntry
    {
        public Purview ParentPurview { get; set; }
        public string DestinationDirectory { get; set; }

    }
}
