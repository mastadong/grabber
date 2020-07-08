using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace grabber.Commands
{
    public class RestoreCommand
    {
        private string _defaultDirectoryPath { get; set; }
        private string _fileMapPath { get; set; }
        private string _purviewMapPath { get; set; }
        private string _configFilePath { get; set; }
        private List<PurviewEntry> PurviewList { get; set; }
        private List<DataFile> DataFileList { get; set; }
        public RestoreCommand()
        {
        }

        public void RestoreDefaults(string defaultDirectoryPath)
        {
            try
            {
                _defaultDirectoryPath = defaultDirectoryPath;
                //Initialize List objects.
                PurviewList = new List<PurviewEntry>();
                DataFileList = new List<DataFile>();
                //Create the default directory
                DirectoryInfo folder = new DirectoryInfo(_defaultDirectoryPath);
                folder.Create();

                //Reset the ECM files.
                ResetECMFiles();
                //Create destination folders in the specified location and serialize data to the ECM file.
                SetDefaultFolders();
                //Create the default datafiles and register them with the ECM file.
                SetDefaultFiles();
                //Write the changes to the new ECM files
                WriteToFile();

                Console.WriteLine("The default configuration was successfully restored.  The root cache folder is now located at: \n" + _defaultDirectoryPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Create and assign default purview folders
        /// </summary>
        private void SetDefaultFolders()
        {
            try
            {
                //Create subdirectory objects
                PurviewEntry siloPurview = new PurviewEntry()
                {
                    ParentPurview = Purview.Silo, 
                    DestinationDirectory = _defaultDirectoryPath + @"silo"
                };
                PurviewEntry airePurview = new PurviewEntry() 
                { 
                    ParentPurview = Purview.Aire, 
                    DestinationDirectory = _defaultDirectoryPath + @"aire" 
                };
                PurviewEntry budgetVSTOPurview = new PurviewEntry()
                { 
                    ParentPurview = Purview.Budget, 
                    DestinationDirectory = _defaultDirectoryPath + @"budgetvsto"
                };


                PurviewList.Add(siloPurview);
                PurviewList.Add(airePurview);
                PurviewList.Add(budgetVSTOPurview);


                //Create subdirectories.
                foreach (PurviewEntry p in PurviewList)
                {
                    DirectoryInfo directory = new DirectoryInfo(p.DestinationDirectory);
                    directory.Create();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
       
        /// <summary>
        /// Generates the default cache files.
        /// </summary>
        private void SetDefaultFiles()
        {
            try
            {
                //Create data file objects.
                string budgetPurviewDirectory = @"budgetvsto\";

                DataFile ProjectedHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "projectedhours.xml", "HoursEntry");
                DataFile ActualHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "actualhours.xml", "HoursEntry");
                DataFile BudgetedHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "budgetedhours.xml", "HoursEntry");

                //Add the datafile objects to the list.
                DataFileList.Add(ProjectedHours);
                DataFileList.Add(ActualHours);
                DataFileList.Add(BudgetedHours);

                //Create the file objects
                foreach(DataFile f in DataFileList)
                {
                    FileInfo file = new FileInfo(f.FullFilePath);
                    using (FileStream fs = file.Create()) ;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Resets the default ECM files 'filemap' and 'purviewmap' in the default cache directory.
        /// </summary>
        private void ResetECMFiles()
        {
            try
            {
                //The ECM files to reset are "filemap.xml" and "purviewmap.xml", located in the default cache directory. 
                //Create the files if they don't exist already; if they do exist, they will be overwritten with new, blank files.
                FileInfo fileMap = new FileInfo(_defaultDirectoryPath + "filemap.xml");
                FileInfo purviewMap = new FileInfo(_defaultDirectoryPath + "purviewmap.xml");
                FileInfo configFile = new FileInfo(_defaultDirectoryPath + "config.dat"); ;

                using (FileStream fs = fileMap.Create()) { };
                using (FileStream fs = purviewMap.Create()) { };
                using (FileStream fs = configFile.Create()) { };
                
                _fileMapPath = fileMap.FullName;
                _purviewMapPath = purviewMap.FullName;
                _configFilePath = configFile.FullName;

            }
            catch (Exception)
            {

                throw;
            }
        
        }

        /// <summary>
        /// Serializes the in-memory file and purview lists to the ECM files 'filemap' and 'purviewmap'.
        /// </summary>
        private void WriteToFile()
        {
            try
            {
                //Write the directory path to the config file so that it is publicly available to other modules.
                KeyPairEntry keyPair = new KeyPairEntry() { key = "DefaultDirectoryPath", value = _defaultDirectoryPath };
                BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                binarySerializingService.SerializeObject(keyPair, _configFilePath);
                binarySerializingService.SerializeObject(keyPair, "cacherootdirectory.dat");
                //TODO: This path must be recorded in the database whenever it is changed.  The database record provides a public location 
                //for other applications to stay up to date on the cache folder address.


                //Write purview data to 'purviewmap.xml'
                XMLSerializingService<List<PurviewEntry>> purviewmapSerializingService = new XMLSerializingService<List<PurviewEntry>>();
                purviewmapSerializingService.SerializeObject(PurviewList, _purviewMapPath);

                //Write DataFile contents to 'filemap.xml'
                XMLSerializingService<List<DataFile>> filemapSerializingService = new XMLSerializingService<List<DataFile>>();
                filemapSerializingService.SerializeObject(DataFileList, _fileMapPath);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
