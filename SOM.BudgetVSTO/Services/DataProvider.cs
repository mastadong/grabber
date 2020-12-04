using Jumble.ExternalCacheManager.Enums;
using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using Jumble.ExternalCacheManager.Services;
using SOM.BudgetVSTO.Enums;
using SOM.BudgetVSTO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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


        /// <summary>
        /// Default method for returning a single query result from the specified internal list.  
        /// The consumer of the SOM may use the internals lists any way they choose. However,querying
        /// for a single value is the most common need, which is why it is provided as a default method
        /// here.
        /// </summary>
        /// <param name="searchParameter"></param>
        /// <returns></returns>
        public object QueryList(string phaseCode, EstimatingCacheType cacheType)
        {
            switch(cacheType)
            {
                case EstimatingCacheType.ProjectedHours:

                    try
                    {
                        var projectedAmount = from h in projectedHours
                                              where h.PhaseCode == phaseCode
                                              select h.SummedHours;
                        return projectedAmount;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error querying projected hours: " + e.Message.ToString());
                    }

                case EstimatingCacheType.ActualHours:

                    try
                    {
                        var actualAmount = from h in actualHours
                                           where h.PhaseCode == phaseCode
                                           select h.SummedHours;
                        return actualAmount;
                    }
                    catch (Exception e)
                    { 
                        throw new Exception("Error querying actual hours: " + e.Message.ToString());
                    }

                case EstimatingCacheType.BudgetedHours:

                    try
                    {
                        var budgetedAmount = from h in budgetedHours
                                             where h.PhaseCode == phaseCode
                                             select h.SummedHours;
                        return budgetedAmount;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error querying budgeted hours: " + e.Message.ToString());
                    }
                
                default:
                    break;
            }

            return 0;
        }

      
    }
}
