using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using SQLManager.Core;

namespace grabber.Commands
{
    public class RenameCommand
    {
        public bool Rename(string oldName, string newName)
        {
            string oldFilePath = "";
            int oldFileMapIndex = 0;
            
            //Validate the file extensions.
            string oldFileExtension = Path.GetExtension(oldName);
            string newFileExtension = Path.GetExtension(newName);
         
            if(oldFileExtension == null || oldFileExtension.Substring(0,1) != "." || newFileExtension == null || newFileExtension.Substring(0,1) != ".")
            {
                Console.WriteLine("Please include file extensions.");
                return false;
            }

            if(oldFileExtension != newFileExtension)
            {
                Console.WriteLine("The file extension of the file you wish to rename does not match the file extension of the new filename.\nProceed? (Y or N): ");
                string response = Console.ReadLine();
                if(response.ToUpper() == "Y")
                {
                    //Continue
                }
                else
                {
                    Console.WriteLine("Operation was cancelled.");
                    return false;
                }
            }

            CacheMap cache = CacheMap.RawLoad();

            //Find the old file and grab its full file path for use later on. 
            foreach(DataFile d in cache.DataFileMap.DataFiles)
            {
                if(d.FileName.ToUpper() == oldName.ToUpper())
                {
                    oldFilePath = d.FullFilePath;
                    oldFileMapIndex = cache.DataFileMap.DataFiles.IndexOf(d);
                    break;
                }
            }

            //Was a match for the old file found in the DataMap?  
            if (oldFilePath == "")
            {
                Console.WriteLine("The file: " + oldName + " was not recognized by the External Cache Manager (ECM).  \nPlease verify that the file exists and is accessible to the ECM.");
                return false;
            }

            //Does the new filename already exist?
            foreach(DataFile d in cache.DataFileMap.DataFiles)
            {
                if(d.FileName.ToUpper() == newName.ToUpper())
                {
                    Console.WriteLine("File '" + newName + "' already exists.  Please use a unique filename and try again.");
                    return false;
                }

            }

            //If the file was matched, the last thing to test is whether or not the file actually exists where it's supposed to.
            if (File.Exists(oldFilePath))
            {
                //If the file exists, everything is good.  Record the rename event in the Jumble database.
                ConnectionStringService connStrService = new ConnectionStringService();
                string decodedString = connStrService.Get(TargetDatabase.Jumble);

                try
                {
                    SQLControl sql = new SQLControl(decodedString);
                    sql.AddParam("@oldName", oldName);
                    sql.AddParam("@newName", newName);
                    sql.ExecNonQuery("INSERT INTO ECM_RenameEvents (OldName, NewName) VALUES (@oldName, @newName)");

                    if (sql.HasException())
                    {
                        Console.WriteLine(sql.Exception.ToString());
                        return false;
                    }

                    //Rename the files in the directory
                    string directoryPath = Path.GetDirectoryName(oldFilePath) + @"\";
                    string newFilePath = directoryPath + newName;
                    File.Move(oldFilePath, newFilePath);

                    //Update the cachemap 
                    DataFile oldDataFile = cache.DataFileMap.DataFiles[oldFileMapIndex];
                    DataFile newDataFile = new DataFile()
                    {
                        FullFilePath = newFilePath,
                        DirectoryPath = Path.GetDirectoryName(newFilePath),
                        FileName = newName,
                        Purview = oldDataFile.Purview,
                        SOMType = oldDataFile.SOMType
                    };

                    //Adjust the in-memory DataFileMap
                    cache.DataFileMap.DataFiles.Remove(oldDataFile);
                    cache.DataFileMap.DataFiles.Add(newDataFile);
                    if (CacheMap.SerializeFileMap(cache) == false)
                    {
                        //Error message is displayed by CacheMap.SerializeFileMap
                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    //If the error came from the rename operation, the database record that was created must be rolled back.  
                    SQLControl rollback = new SQLControl(decodedString);
                    rollback.AddParam("@oldName", oldName);
                    rollback.AddParam("@newName", newName);
                    rollback.ExecQuery("SELECT * FROM ECM_RenameEvents WHERE OldName = @oldName AND NewName = @newName");
                    if(rollback.RecordCount != 0)
                    {
                        rollback.AddParam("@oldName", "oldFile");
                        rollback.AddParam("@newName", "newFile");
                        rollback.ExecNonQuery("DELETE FROM ECM_RenameEvents WHERE OldName = @oldName AND NewName = @newName");
                    }

                    Console.WriteLine("RenameCommandError: " + e.Message.ToString() +"\nPlease check both file names and try again");
                    return false;
                }

            }
            else
            {
                Console.WriteLine("The file: " + oldName + " could not be found in the cache.  Please verify that the file does exist before trying again.");
                return false;
            }
        }

    }
}
