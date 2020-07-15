using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;

namespace Jumble.ExternalCacheManager.Managers
{
    public class CacheMap
    {
        public string ConnectionStrFileName { get; } = "connectionstr.xml";
        public string ConfigFileName { get; } = "config.dat";
        //The 'cacherootdirectory.dat' file lives in the application's local folder and is required to load the CacheMap correctly.
        public string ApplicationCacheRootDirectoryFileName { get; }  = "cacherootdirectory.dat";
        public string _cacheRootDirectory { get; set; }
        public DataFileMap DataFileMap { get; set; }
        public PurviewMap PurviewMap { get; set; }
        public string connStrFilePath { get; set; }
        public static CacheMap Load()
        {
            CacheMap cacheMap = new CacheMap();
            cacheMap._cacheRootDirectory = GetCacheRootDirectory(cacheMap);
            if(cacheMap._cacheRootDirectory == null)
            {
                return null;
            }
            else
            {
                cacheMap.DataFileMap = new DataFileMap() { DataFiles = GetDataFiles(cacheMap._cacheRootDirectory) };
                cacheMap.PurviewMap = new PurviewMap() { PurviewEntries = GetPurviewEntries(cacheMap._cacheRootDirectory) };
                cacheMap.connStrFilePath = GetConnectionStringFilePath(cacheMap);
                return cacheMap;
            }
        }

        /// <summary>
        /// Returns a CacheMap object that populates the filename constants only.  Used to restore the cache configuration, including the 
        /// application local files necessary to fully load the CacheMap without error.
        /// </summary>
        /// <returns></returns>
        public static CacheMap Safe()
        {
            CacheMap cache = new CacheMap();
            return cache;
        }
        /// <summary>
        /// Returns the path of the cache root directory which is stored in the local application file.  The path is necessary for locating 
        /// the filemap and purviewmap files which are used to load the cache map.
        /// </summary>
        /// <returns></returns>
        public static string GetCacheRootDirectory(CacheMap cache)
        {
            try
            {
                BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                //Deserialize the cache root directory path from the DAT file stored in the local application folder.
                KeyPairEntry keyPair = binarySerializingService.DeserializeObject(cache.ApplicationCacheRootDirectoryFileName);
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
        /// Returns a list of the datafile names for the provided purview.
        /// </summary>
        /// <param name="purviewType"></param>
        /// <returns></returns>
        public static List<string> GetDataFileNamesByPurview(CacheMap cache, Purview purviewType)
        {
            List<string> dataFileNames = new List<string>();
            foreach(DataFile d in cache.DataFileMap.DataFiles)
            {
                if(d.Purview == purviewType)
                {
                    dataFileNames.Add(d.FileName.ToString());
                }
            }

            return dataFileNames;
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

        /// <summary>
        /// Returns the file path of the connection string source file.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static string GetConnectionStringFilePath(CacheMap cache)
        {
            return cache._cacheRootDirectory + cache.ConnectionStrFileName;
        }

        /// <summary>
        /// Returns the full file path for the provided filename, if it can be found.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDataFilePathByName(string fileName)
        {
            bool FileLocated = false;
            CacheMap cache = CacheMap.Load();
            foreach(DataFile d in cache.DataFileMap.DataFiles)
            {
                if(d.FileName == fileName)
                {
                    FileLocated = true;
                    return d.FullFilePath;
                }
            }

            if(FileLocated == false)
            {
                Console.WriteLine("File : " + fileName + " could not be matched to a file in the CacheMap.");
                return "";
            }

            return "How did I get here? : " + fileName;
        }



    /// <summary>
    /// Serializes changes to the in-memory copy of the DataFileMap 
    /// </summary>
    /// <param name="cacheMap"></param>
    /// <returns></returns>
       public static bool SerializeFileMap(CacheMap cacheMap)
       {
            try
            {
                XMLSerializingService<List<DataFile>> xmlSerializingService = new XMLSerializingService<List<DataFile>>();
                xmlSerializingService.SerializeObject(cacheMap.DataFileMap.DataFiles, cacheMap._cacheRootDirectory + "filemap.xml");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Serialization error when trying to update DataFileMap changes.  The output filepath was:\n" + cacheMap._cacheRootDirectory + "filemap.xml");
                Console.WriteLine("ERROR: " + e.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// Serializs changes to the in-memory copy of the PurviewFileMap
        /// </summary>
        /// <param name="cacheMap"></param>
        /// <returns></returns>
        public static bool SerializePurviewMap(CacheMap cacheMap)
        {
            try
            {
                XMLSerializingService<List<PurviewEntry>> xmlSerializingService = new XMLSerializingService<List<PurviewEntry>>();
                xmlSerializingService.SerializeObject(cacheMap.PurviewMap.PurviewEntries, cacheMap._cacheRootDirectory + "purviewmap.xml");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Serialization error when trying to update PurviewFileMap changes.  The output filepath was:\n" + cacheMap._cacheRootDirectory + "purviewmap.xml, and the error reads: " + e.Message.ToString());
                return false;
            }
        }

    }
}
