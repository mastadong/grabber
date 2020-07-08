using Jumble.ExternalCacheManager.Enums;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Managers
{
    public class CacheFileManager
    {
        public static bool LockFiles()
        {
            return true;
        }

        public static bool UnlockFiles()
        {
            return true;
        }
    }
}
