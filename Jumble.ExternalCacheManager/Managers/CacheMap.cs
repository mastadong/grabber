using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jumble.ExternalCacheManager.Managers
{
    public class CacheMap
    {
        public string _cacheRootDirectory { get; private set; }
        public DataFileMap DataFileMap { get; set; }
        public PurviewMap PurviewMap { get; set; }

        public static CacheMap Load()
        {
            CacheMap cacheMap = new CacheMap();
            cacheMap._cacheRootDirectory = GetCacheRootDirectory();
            if(cacheMap._cacheRootDirectory == null)
            {
                return null;
            }
            else
            {
                cacheMap.DataFileMap = new DataFileMap() { DataFiles = GetDataFiles(cacheMap._cacheRootDirectory) };
                cacheMap.PurviewMap = new PurviewMap() { PurviewEntries = GetPurviewEntries(cacheMap._cacheRootDirectory) };

                return cacheMap;
            }
        }

        /// <summary>
        /// Returns the path of the cache root directory which is stored in the local application file.  The path is necessary for locating 
        /// the filemap and purviewmap files which are used to load the cache map.
        /// </summary>
        /// <returns></returns>
        public static string GetCacheRootDirectory()
        {
            try
            {
                BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                KeyPairEntry keyPair = binarySerializingService.DeserializeObject("cacherootdirectory.dat");
                string directory = keyPair.value;
                if(Directory.Exists(directory))
                {
                    return directory;
                }
                else
                {
                    Console.WriteLine("The cache root directory " + directory + " cannot be found.\nIf the directory was moved, then the change was not recorded with the system.\nTo fix the problem, the 'cacherootdirectory.dat' file must be reserialized");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Returns the list of DataFile entries in the filemap file.
        /// </summary>
        /// <param name="cacheRootDirectory"></param>
        /// <returns></returns>
        public static List<DataFile> GetDataFiles(string cacheRootDirectory)
        {
            string dataFileMap = "filemap.xml";
            if (File.Exists(cacheRootDirectory + dataFileMap))
            { 
                XMLSerializingService<List<DataFile>> xmlSerializingService = new XMLSerializingService<List<DataFile>>();
                return xmlSerializingService.DeserializeObject(cacheRootDirectory + dataFileMap);
            }
            else
            {
                Console.WriteLine("'filemap.xml' was not found in the directory: " + cacheRootDirectory + ".\n If the name was changed, try renaming the file and trying again.");
                return null;
            }
        }


        /// <summary>
        /// Returns the list of PurviewEntries stored in the purviewmap.xml file.
        /// </summary>
        /// <param name="cacheRootDirectory"></param>
        /// <returns></returns>
        public static List<PurviewEntry> GetPurviewEntries(string cacheRootDirectory)
        {
            string purviewMap = "purviewmap.xml";
            if (File.Exists(cacheRootDirectory + purviewMap))
            {
                XMLSerializingService<List<PurviewEntry>> xmlSerializingService = new XMLSerializingService<List<PurviewEntry>>();
                return xmlSerializingService.DeserializeObject(cacheRootDirectory + purviewMap); 
            }
            else
            {
                Console.WriteLine("'filemap.xml' was not found in the directory: " + cacheRootDirectory + ".\n If the name was changed, try renaming the file and trying again.");
                return null;
            }
        }

        public static string GetPurviewDirectoryPath(Purview purview, CacheMap cache)
        {
            foreach (PurviewEntry p in cache.PurviewMap.PurviewEntries)
            {
                if (p.ParentPurview == purview)
                {
                   return p.DestinationDirectory + @"\";
                }
            }
            return null;
        }

        //public static bool VerifySOMType(string somType)
        //{

        //}


    }
}
