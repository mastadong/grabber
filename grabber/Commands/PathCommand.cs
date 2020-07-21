using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace grabber.Commands
{
    public class PathCommand
    {
        /// <summary>
        /// Prints the directory structure for the cache root directory configuration.
        /// </summary>
        public void DisplayDirectoryStructure()
        {
            CacheMap cache = CacheMap.RawLoad();
            Console.WriteLine("    ");
            Console.WriteLine("CACHE ROOT DIRECTORY: " + cache._cacheRootDirectory.ToString());
            Console.WriteLine("    SUB DIRECTORIES: ");
            Console.WriteLine("    ");

            foreach(PurviewEntry p in cache.PurviewMap.PurviewEntries)
            {
                Console.WriteLine("    " + p.DestinationDirectory + "\n    |");
                Console.WriteLine("        FILES: ");
                DirectoryInfo d = new DirectoryInfo(p.DestinationDirectory);
                foreach (FileInfo file in d.GetFiles())
                {
                    Console.WriteLine("            " + file.Name);
                }
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Updates the cache root directory, copies all existing files to the new location, then deletes all files and folders in the 
        /// old root directory.
        /// </summary>
        /// <param name="newCacheRootDirectory"></param>
        /// <returns></returns>
        public bool ChangeCacheRootDirectory(string newCacheRootDirectory)
        {
            try
            {
                //copy all the existing folders and files to the new location. 
                CacheMap cache = CacheMap.RawLoad();
                string oldCacheRootDirectory = cache._cacheRootDirectory;
                //copy the folders
                foreach (PurviewEntry p in cache.PurviewMap.PurviewEntries)
                {
                    //Create the new directory
                    Directory.CreateDirectory(newCacheRootDirectory + p.ParentPurview.ToString());
                    //Update the In-Memory (IM) purview entries
                    p.DestinationDirectory = newCacheRootDirectory + p.ParentPurview.ToString();
                }

                //Change the IM DataFile paths
                foreach (DataFile d in cache.DataFileMap.DataFiles)
                {
                    string newFullFilePath = newCacheRootDirectory + d.Purview.ToString() + @"\" + d.FileName;
                    //copy the file
                    if (File.Exists(newFullFilePath))
                    {
                        File.Delete(newFullFilePath);
                    }
                    File.Copy(d.FullFilePath, newFullFilePath);

                    //Update the IM DataFile data.
                    d.DirectoryPath = newCacheRootDirectory + d.Purview.ToString();
                    d.FullFilePath = newFullFilePath;
                }

                //Copy the remaining ECM folders to the new directory.
                string newConnStrFilePath = newCacheRootDirectory + cache.ConnectionStrFileName;
                string newConfigFilePath = newCacheRootDirectory + cache.ConfigFileName;
                if (File.Exists(newConnStrFilePath))
                {
                    File.Delete(newConnStrFilePath);
                }
                if (File.Exists(newConfigFilePath))
                {
                    File.Delete(newConnStrFilePath);
                }
                File.Copy(oldCacheRootDirectory + cache.ConnectionStrFileName, newConnStrFilePath);
                File.Copy(oldCacheRootDirectory + cache.ConfigFileName, newConfigFilePath);

                //Serialize the updated IM objects.
                cache._cacheRootDirectory = newCacheRootDirectory;
                cache.connStrFilePath = newConnStrFilePath;
                CacheMap.SerializePurviewMap(cache);
                CacheMap.SerializeFileMap(cache);

                //Write the directory path to the config file so that it is publicly available to other modules.
                KeyPairEntry keyPair = new KeyPairEntry() { key = "DefaultDirectoryPath", value = newCacheRootDirectory };
                BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                binarySerializingService.SerializeObject(keyPair, newConfigFilePath);
                binarySerializingService.SerializeObject(keyPair, cache.ApplicationCacheRootDirectoryFileName);

                //Update the cacherootdirectory path stored in the database.
                RestoreCommand restoreCommand = new RestoreCommand();
                restoreCommand.RecordConfiguration(newCacheRootDirectory);

                //Delete the contents of the previous cache root directory.
                Directory.Delete(oldCacheRootDirectory, true);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("PathCommand Error: " + e.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// Displays the contents of the local application file 'cacherootdirectory.dat' which is necessary for the ECM to properly
        /// load the CacheMap.
        /// </summary>
        public void ViewRootCacheDirectory()
        {
            CacheMap cache = CacheMap.Safe();
            try
            {
                BinarySerializingService<KeyPairEntry> binSerializingService = new BinarySerializingService<KeyPairEntry>();
                KeyPairEntry configValues = binSerializingService.DeserializeObject(cache.ApplicationCacheRootDirectoryFileName);
                Console.WriteLine("The Cache Root Directory listed in the application file is: \n" + configValues.value.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error deserializing '" + cache.ApplicationCacheRootDirectoryFileName  +"' :" + e.Message.ToString());
            }

        }

    }
}
