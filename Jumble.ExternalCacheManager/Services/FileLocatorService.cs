using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using SQLManager.Core;
using System;
using System.Collections.Generic;
using System.IO;
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
        public string GetCacheFilePath(Purview purviewType, string fileName)
        {
            bool FileFound = false;
            //Load the cacherootdirectory path from the database entry.
            CacheMap cache = CacheMap.Load();
            //The purviewDirectoryPath is the same for all files in a purview.  Take it from the 
            //first Datafile found in the list.
            string purviewDirectoryPath = cache.DataFileMap.DataFiles[0].DirectoryPath;
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
                return FindLatestFileName(purviewDirectoryPath, fileName);
            }

            return "How did I get here?";
        }

        /// <summary>
        /// Searches the path of rename events to find the last name assigned to the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string FindLatestFileName(string purviewDirectoryPath, string fileName)
        {
            bool IsLatestFileName = false;
            string targetFileName = fileName;
            ConnectionStringService connectionStringService = new ConnectionStringService();
           
            while (IsLatestFileName == false)
            {
                //Check if a rename event is registered for the provided filename. 
                SQLControl sql = new SQLControl(connectionStringService.Get(TargetDatabase.Jumble));
                sql.AddParam("@fileName", targetFileName);
                sql.ExecQuery("SELECT NewName FROM ECM_RenameEvents WHERE OldName = @fileName ORDER BY Id");
                if (sql.HasException())
                {
                    Console.WriteLine("Error trying to retrieve Rename Event: " + sql.Exception.ToString());
                    throw new Exception(sql.Exception.ToString());
                }
                else if (sql.DBDT.Rows.Count > 0)
                {
                    //Obtain the new name.
                    string updatedFileName = sql.DBDT.Rows[0].ItemArray[0].ToString();
                    string updatedFilePath = Path.Combine(purviewDirectoryPath, updatedFileName);
                    //If the file exists, return it to the caller and terminate the loop.
                    if(File.Exists(updatedFilePath))
                    {
                        IsLatestFileName = true;
                        return updatedFilePath;
                    }
                    //If the file doesn't exist, update the target file name and conduct the search again.
                    else
                    {
                        targetFileName = updatedFileName;
                    }

                }
                else //No records were returned.
                {
                    throw new Exception("The file : " + fileName + " cannot be found and no rename event can be matched to the filename.");

                }
            }

            return "How did I get here?  Because the compiler wants to statically verify that the WHILE loop can " +
                    "be terminated.";
        }
    }
}
