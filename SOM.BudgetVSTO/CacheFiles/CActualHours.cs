using Jumble.ExternalCacheManager.Enums;
using SOM.BudgetVSTO.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO.CacheFiles
{
    /// <summary>
    /// Stores summed actual hours from 'JC_TRANSACTION_HISTORY_MC'.  Requires parameter '@startRange' for job number start range.
    /// </summary>
    public class CActualHours : ICacheable
    {
        public string CacheFileName => "actualhours.dat";
        public string DataRequest()
        {
            return "SELECT Job_Number, Phase_Code, SUM(Total_Hours) AS 'SummedHours' " +
                    "FROM JC_TRANSACTION_HISTORY_MC " +
                    "WHERE Company_Code = 'EAI' " +
                    "AND Cost_Type = 'L' " +
                    "AND Job_Number > @startRange " +
                    "GROUP BY Job_Number, Phase_Code";
        }
        public SOMType SOMType => SOMType.HoursEntry;
        public TargetDatabase TargetDatabase => TargetDatabase.Spectrum;


        object ICacheable.GetType()
        {
            return SOMTypeConverter.ConvertToClass(this.SOMType);
        }

    }
}
