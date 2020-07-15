using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Services
{
    public class FileLocatorService
    {
        /// <summary>
        /// Performs the cache file lookup using the provided filename, then returns the full file path if it can be 
        /// found.  This function also handles searching for Rename events when they exist. 
        /// </summary>
        /// <returns></returns>
        public string GetCacheFilePath(string fileName)
        {
            bool FileFound = false;
            CacheMap cache = CacheMap.Load();
            foreach(DataFile d in cache.DataFileMap.DataFiles)
            {
                if(d.FileName == fileName)
                {
                    FileFound = true;
                    return d.FullFilePath;
                }
            }

            //If the file was unable to be retrieved, perform the check for a Rename operation.
            if (FileFound == false)
            {
                return "File Not Found";
            }

            return "How did I get here?";
        }

    }
}
