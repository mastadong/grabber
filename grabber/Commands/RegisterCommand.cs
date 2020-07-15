using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace grabber.Commands
{
    public class RegisterCommand
    {
        public RegistrationResult Register(Purview purview, string fileName, string somType)
        {
            //1. Load the cache map into memory
            CacheMap cacheMap = CacheMap.Load();
            if (cacheMap == null)
            {
                return RegistrationResult.IncompleteLoad;
            }

            //2. Test the provided filename for unique/valid
            foreach(DataFile d in cacheMap.DataFileMap.DataFiles)
            {
                if (d.FileName.ToUpper() == fileName.ToUpper())
                {
                    return RegistrationResult.DuplicateName;
                }
            }

            try
            {
                string purviewDirectoryPath = CacheMap.GetPurviewDirectoryPath(purview, cacheMap);
                string fullFilePath = purviewDirectoryPath + fileName;

                //3. Populate new DataFile object
                DataFile newDataFile = new DataFile(purview, fullFilePath, somType.ToString());

                //4. Update in-memory list
                cacheMap.DataFileMap.DataFiles.Add(newDataFile);

                //5.  Create the file
                FileInfo file = new FileInfo(fullFilePath);
                using (FileStream fs = file.Create());

                //6.  If the new file was created successfully, serialize the udpated datafile list to save the changes.
                XMLSerializingService<List<DataFile>> xmlSerializingService = new XMLSerializingService<List<DataFile>>();
                xmlSerializingService.SerializeObject(cacheMap.DataFileMap.DataFiles, cacheMap._cacheRootDirectory + "filemap.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RegistrationResult.Fail;
                
            }

            //6. TODO: Record document event in database
            return RegistrationResult.Success;
        }

    }
}
