using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jumble.ExternalCacheManager.Managers;

namespace grabber.Commands
{
    public class RestoreCommand
    {
        private string _defaultDirectoryPath { get; set; }
        private string _fileMapPath { get; set; }
        private string _purviewMapPath { get; set; }
        private string _configFilePath { get; set; }
        private string _connStrFilePath { get; set; }
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
                RestoreECMFiles();
                //Create destination folders in the specified location and serialize data to the ECM file.
                SetDefaultFolders();
                //Create the default datafiles and register them with the ECM file.
                CreateEmptyDefaultFiles();
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
                    DestinationDirectory = _defaultDirectoryPath + Purview.Silo.ToString()
                };
                PurviewEntry airePurview = new PurviewEntry()
                {
                    ParentPurview = Purview.Aire,
                    DestinationDirectory = _defaultDirectoryPath + Purview.Aire.ToString() 
                };
                PurviewEntry budgetVSTOPurview = new PurviewEntry()
                { 
                    ParentPurview = Purview.Budget, 
                    DestinationDirectory = _defaultDirectoryPath + Purview.Budget.ToString()
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
            catch (Exception e)
            {
                Console.WriteLine("Error creating default folders: " + e.Message.ToString());
            }
        }
       
        /// <summary>
        /// Generates the default cache files.
        /// </summary>
        private void CreateEmptyDefaultFiles()
        {
            try
            {
                //Create data file objects.
                string budgetPurviewDirectory = Purview.Budget.ToString() + @"\";

                DataFile ProjectedHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "projectedhours.dat", "HoursEntry");
                DataFile ActualHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "actualhours.dat", "HoursEntry");
                DataFile BudgetedHours = new DataFile(Purview.Budget, _defaultDirectoryPath + budgetPurviewDirectory + "budgetedhours.dat", "HoursEntry");

                //Add the datafile objects to the list.
                DataFileList.Add(ProjectedHours);
                DataFileList.Add(ActualHours);
                DataFileList.Add(BudgetedHours);

                //Create the file objects
                foreach(DataFile f in DataFileList)
                {
                    FileInfo file = new FileInfo(f.FullFilePath);
                    using (FileStream fs = file.Create());
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
        private void RestoreECMFiles()
        {
            try
            {
                //The ECM files to reset are "filemap.xml" and "purviewmap.xml", located in the default cache directory. 
                //Create the files if they don't exist already; if they do exist, they will be overwritten with new, blank files.
                FileInfo fileMap = new FileInfo(_defaultDirectoryPath + "filemap.xml");
                FileInfo purviewMap = new FileInfo(_defaultDirectoryPath + "purviewmap.xml");
                FileInfo configFile = new FileInfo(_defaultDirectoryPath + "config.dat"); 
                FileInfo connStringFile = new FileInfo(_defaultDirectoryPath + "connectionstr.xml");
                CacheMap cache = CacheMap.Safe();
                FileInfo applicationLocalCacheRootFile = new FileInfo(cache.ApplicationCacheRootDirectoryFileName); 

                using (FileStream fs = fileMap.Create()) { };
                using (FileStream fs = purviewMap.Create()) { };
                using (FileStream fs = configFile.Create()) { };
                using (FileStream fs = connStringFile.Create()) { };
                using (FileStream fs = applicationLocalCacheRootFile.Create()) { };

                _fileMapPath = fileMap.FullName;
                _purviewMapPath = purviewMap.FullName;
                _configFilePath = configFile.FullName;
                _connStrFilePath = connStringFile.FullName;

                //Encode and serialize the default connection strings.
                CryptoService cryptoService = new CryptoService();
                string defaultJumbleConnectionString = @"Data Source=ADEAI-ALT\TRIMBLE;Initial Catalog=Jumble_dev;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                string estimatingDatabaseString = @"Data Source=ADEAI-ALT\TRIMBLE;Initial Catalog=BudgetVSTO;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                string spectrumDatabaseString = @"Data Source=SPECTRUM;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                string encodedJumbleConnectionString = cryptoService.Encrypt(defaultJumbleConnectionString);
                string encodedEstimatingConnectionString = cryptoService.Encrypt(estimatingDatabaseString);
                string encodedSpectrumConnectionString = cryptoService.Encrypt(spectrumDatabaseString);


                //Create the data objects to serialize.
                List<KeyPairEntry> connectionStrings = new List<KeyPairEntry>();
                KeyPairEntry jumbleString = new KeyPairEntry() { key = TargetDatabase.Jumble.ToString(), value = encodedJumbleConnectionString };
                KeyPairEntry estimatingString = new KeyPairEntry() { key = TargetDatabase.Estimating.ToString(), value = encodedEstimatingConnectionString };
                KeyPairEntry spectrumString = new KeyPairEntry() { key = TargetDatabase.Spectrum.ToString(), value = encodedSpectrumConnectionString };
               
                //Add the strings to the list.
                connectionStrings.Add(jumbleString);
                connectionStrings.Add(estimatingString);
                connectionStrings.Add(spectrumString);

                //Serialize the list to the connection string file.
                XMLSerializingService<List<KeyPairEntry>> xmlSerializingService = new XMLSerializingService<List<KeyPairEntry>>();
                xmlSerializingService.SerializeObject(connectionStrings, _connStrFilePath);

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

                CacheMap cache = CacheMap.Safe();
                //binarySerializingService.SerializeObject(keyPair, "cacherootdirectory.dat");
                binarySerializingService.SerializeObject(keyPair, cache.ApplicationCacheRootDirectoryFileName);
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

        /// <summary>
        /// Regenerates the 
        /// </summary>
        /// <param name="defaultDirectoryName"></param>
        public void RestoreCacheRootDirectoryFile(string defaultDirectoryName, string targetDirectory=null)
        {
            bool DirectoryExists = true;

            if(!Directory.Exists(defaultDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(defaultDirectoryName);

                }
                catch
                {
                    DirectoryExists = false;
                    Console.WriteLine("Unable to create the directory: " + defaultDirectoryName);
                    Console.WriteLine("Process was cancelled. ");
                }
            }

            if(DirectoryExists == true)
            {
                if(!string.IsNullOrEmpty(targetDirectory))
                {
                    if(Directory.Exists(targetDirectory) == true)
                    {
                        CacheMap cache = CacheMap.Safe();
                        //Create the target file path.
                        string targetFilePath = Path.Combine(targetDirectory + cache.ApplicationCacheRootDirectoryFileName);
                        //Create the file instance in the target folder.
                        try
                        {
                            FileInfo applicationLocalCacheRootFile = new FileInfo(targetFilePath);
                            using (FileStream fs = applicationLocalCacheRootFile.Create()) { };
                            //Write the data to file. 
                            KeyPairEntry keyPair = new KeyPairEntry() { key = "DefaultDirectoryPath", value = defaultDirectoryName };
                            BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                            binarySerializingService.SerializeObject(keyPair, targetFilePath);
                           
                            Console.WriteLine("The file was successfully created at: " + targetFilePath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error trying to create file '" + targetFilePath + "': " + e.Message.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("The directory " + targetDirectory + " cannot be found and cannot be created at this time.\nOperation has been cancelled.");
                    }
                }
                else if (string.IsNullOrEmpty(targetDirectory))
                { 
                    //Create the file instance in the local application folder.
                    CacheMap cache = CacheMap.Safe();
                    FileInfo applicationLocalCacheRootFile = new FileInfo(cache.ApplicationCacheRootDirectoryFileName);
                    using (FileStream fs = applicationLocalCacheRootFile.Create()) { };

                    //Write the data to file. 
                    KeyPairEntry keyPair = new KeyPairEntry() { key = "DefaultDirectoryPath", value = defaultDirectoryName };
                    BinarySerializingService<KeyPairEntry> binarySerializingService = new BinarySerializingService<KeyPairEntry>();
                    binarySerializingService.SerializeObject(keyPair, cache.ApplicationCacheRootDirectoryFileName);
                    
                    Console.WriteLine("The file was successfully created.");
                }
                
               
            }

        }

    }
}
