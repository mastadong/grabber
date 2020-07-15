using System;
using System.Collections.Generic;
using System.Text;
using SQLManager;
using Jumble.ExternalCacheManager.Services;
using SOM.BudgetVSTO;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Enums;
using SOM.BudgetVSTO.CacheFiles;
using SQLManager.Core;
using System.Data;
using SOM.BudgetVSTO.Model;
using System.IO;

namespace grabber.CacheManager
{
    /// <summary>
    /// A decoupled class for running the database backups.  As files are added/changed, this single library can be modified 
    /// without having to recompile the grabber utility.
    /// </summary>
    public class BackupManager
    {
        public void RunBackup()
        {
            //Budget VSTO Files
            //1. Identify the target cache files for the purview.
            CacheMap cache = CacheMap.Load();
            List<string> dataFileNames = CacheMap.GetDataFileNamesByPurview(cache, Purview.Budget);

            //FOR EACH INDIVIDUAL FILE:
            CProjectedHours projectedHoursCache = new CProjectedHours();
            CActualHours actualHoursCache = new CActualHours();
            CBudgetedHours budgetedHoursCache = new CBudgetedHours();

            var startTime = DateTime.Now;


            bool BackupSuccessful = true;
                BackupSuccessful = WriteCacheFile(projectedHoursCache);
                BackupSuccessful = WriteCacheFile(actualHoursCache);
                BackupSuccessful = WriteCacheFile(budgetedHoursCache);
           
            var endTime = DateTime.Now;
            var elapsedTime = endTime - startTime;

            if(BackupSuccessful == true)
            {
                Console.WriteLine("Backup was successful. " + DateTime.Now);
                Console.WriteLine(elapsedTime.ToString());
                //TODO: Record the backup event in the backup event header table.
            }
            else
            {
                Console.WriteLine("The backup was unsuccessful. " + DateTime.Now);
                //TODO: Generate detailed error message for later review.
            }

        }

        /// <summary>
        /// Writes the cache data to file for the provided cacheable object.
        /// </summary>
        /// <param name="cacheableObject"></param>
        /// <returns></returns>
        private bool WriteCacheFile(ICacheable cacheableObject)
        {
            Console.WriteLine("Begin serialization for '" + cacheableObject.CacheFileName + "'...");
            var startTime = DateTime.Now;

            try
            {
                //Instantiate the list of HoursEntry objects
                List<HoursEntry> hourEntries = new List<HoursEntry>();

                //For HoursEntry objects, a parameter '@startRange" is included.  
                //Specify the earliest date for which records should be returned. 
                string searchStart = "   2170600";

                //2. Get the associated data request from the 'C(dataclass)' file. 
                string dataRequest = cacheableObject.DataRequest();

                //3. Perform the data request against the database
                ConnectionStringService connectionStringService = new ConnectionStringService();

                Console.WriteLine("Attempting to connect to " + cacheableObject.TargetDatabase.ToString() + " database...");

                SQLControl sql = new SQLControl(connectionStringService.Get(cacheableObject.TargetDatabase));
                sql.AddParam("@startRange", searchStart);
                sql.ExecQuery(dataRequest);
                if (sql.HasException())
                {
                    Console.WriteLine("Error in SQLControl while trying to retrieve data for " + cacheableObject.CacheFileName + ": \n" + sql.Exception.ToString());
                    return false;
                }
                else if (sql.DBDT.Rows.Count != 0)
                {
                    Console.WriteLine("Connection was successful.  The data request returned " + sql.RecordCount.ToString() + " records");
                    SpectrumConverter converter = new SpectrumConverter();

                    //DataSet dataset = new DataSet();
                    //dataset.Tables.Add(sql.DBDT);
                    //dataset.WriteXml(@"\\sbs\Users\Noahb\EstimatingAddIn\superAwesomeDataTable.xml");

                    //4. Process the datatable results into the correct format for serializing
                    for (int i = 0; i <= sql.DBDT.Rows.Count - 1; i++)
                    {
                        //Create an HoursEntry object and add it to the list. 
                        HoursEntry hoursEntry = new HoursEntry()
                        {
                            JobNumber = converter.ConvertFromSpectrumFormat(sql.DBDT.Rows[i].ItemArray[0].ToString(), SpectrumFormat.JobNumber),
                            PhaseCode = converter.ConvertFromSpectrumFormat(sql.DBDT.Rows[i].ItemArray[1].ToString(), SpectrumFormat.PhaseCode),
                            SummedHours = Convert.ToDouble(sql.DBDT.Rows[i].ItemArray[2])
                        };

                        //Add to the list.
                        hourEntries.Add(hoursEntry);
                    }
                }
                else
                {
                    Console.WriteLine("The query was successful, but no records were returned.");
                    return false;
                }

                //5. Serialize the data to file in the appropriate location.
                //Fetch the target data file path. 
                Console.WriteLine("Preparing to write to file...");
                string serializationTarget = CacheMap.GetDataFilePathByName(cacheableObject.CacheFileName);

                if (Path.GetExtension(cacheableObject.CacheFileName) == ".dat")
                {
                    BinarySerializingService<List<HoursEntry>> binarySerializingService = new BinarySerializingService<List<HoursEntry>>();
                    binarySerializingService.SerializeObject(hourEntries, serializationTarget);
                }
                else if (Path.GetExtension(cacheableObject.CacheFileName) == ".xml")
                {
                    XMLSerializingService<List<HoursEntry>> xmlSerializingService = new XMLSerializingService<List<HoursEntry>>();
                    xmlSerializingService.SerializeObject(hourEntries, serializationTarget);
                }
                else
                {
                    Console.WriteLine("Unrecognized file extension '" + Path.GetExtension(cacheableObject.CacheFileName) + "'.");
                    return false;
                }

                //6. Return TRUE, then repeat the process until all backups are complete.
                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;
                Console.WriteLine("The data was successfully written to the cache");
                Console.WriteLine("Elapsed time: " + elapsedTime.ToString());
                Console.WriteLine("End serialization for file '" + cacheableObject.CacheFileName + "'.\n");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to write to cache for '" + cacheableObject.CacheFileName + "': " + e.Message.ToString());
                return false;
            }
        }

    }
}
