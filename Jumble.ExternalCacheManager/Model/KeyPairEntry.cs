using System;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Model
{
    /// <summary>
    /// Key Pair Entry object; all key-value pairs must be of type <string, string>
    /// </summary>
    [Serializable]
    public class KeyPairEntry : CacheObject
    {
        public string key { get; set; }
        public string value { get; set; }

        public KeyPairEntry()
        {
        }
    }
}
