using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Model
{
    public class ConfigurationStore
    {



        public string GetDirectoryPath(Purview methodPurview)
        {
            switch (methodPurview)

            {
                case Purview.Budget:
                    return "";
                case Purview.Document:
                    return "";
                case Purview.Aire:
                    return "";
                case Purview.Silo:
                    return "";
                default:
                    return "";
            }
        }

    }
}
