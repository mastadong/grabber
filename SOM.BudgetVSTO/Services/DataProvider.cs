using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using SOM.BudgetVSTO.Enums;
using SOM.BudgetVSTO.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO.Services
{
    public class DataProvider
    {
        public List<HoursEntry> projectedHours { get; set; }
        public List<HoursEntry> actualHours { get; set; }
        public List<HoursEntry> budgetedHours { get; set; }
       

        /// <summary>
        /// Load the cached data into memory.
        /// </summary>
        public void LoadData()
        {
            try
            {
                FileLocatorService fileLocator = new FileLocatorService();
                
                projectedHours = (List<HoursEntry>)GetLoadedList((fileLocator.GetCacheFilePath(Purview.Budget, "projectedhours.dat")), SOMType.HoursEntry);
                actualHours = (List<HoursEntry>)GetLoadedList((fileLocator.GetCacheFilePath(Purview.Budget, "actualhours.dat")), SOMType.HoursEntry);
                budgetedHours = (List<HoursEntry>)GetLoadedList((fileLocator.GetCacheFilePath(Purview.Budget, "budgetedhours.dat")), SOMType.HoursEntry);
            }
            catch (Exception e)
            {
                throw new Exception("Error loading the Budget data cache: " + e.Message);
            }
        }


        /// <summary>
        /// Returns a populated list from the deserialized cache file.  The base data object is 
        /// detected through the SOMType associated with the file.  The caller needs to provide 
        /// only the filepath (having already specified the purview to the FileLocatorService) 
        /// and (string)somType associated with the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="somType"></param>
        /// <returns></returns>
        private object GetLoadedList(string filePath, SOMType somType)
        {
            switch (somType)
            {
                case SOMType.HoursEntry:
                    BinarySerializingService<List<HoursEntry>> hrsSerializingService = new BinarySerializingService<List<HoursEntry>>();
                    return hrsSerializingService.DeserializeObject(filePath);
                case SOMType.SystemEntry:
                    BinarySerializingService<List<SystemEntry>> sysSerializingService = new BinarySerializingService<List<SystemEntry>>();
                    return sysSerializingService.DeserializeObject(filePath);
                case SOMType.Nothing:
                    return null;
            }

            Console.WriteLine("SOMDataProvider: Unable to locate and/or deserialize the file: " + filePath);
            return null;
        }


    }
}
