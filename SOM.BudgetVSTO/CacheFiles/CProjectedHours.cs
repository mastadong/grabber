using Jumble.ExternalCacheManager.Enums;
using SOM.BudgetVSTO.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO.CacheFiles
{
    /// <summary>
    /// Stores summed projected hours from 'JC_PROJ_COST_HISTORY_MC'.  Requires parameter '@startRange' for job number start range.
    /// </summary>
    public class CProjectedHours : ICacheable
    {
        public string CacheFileName => "projectedhours.dat";
        public string DataRequest()
        {
            return "SELECT Job, Phase, SUM(Projected_Hours) AS 'SummedHours' " +
                    "FROM JC_PROJ_COST_HISTORY_MC " +
                    "WHERE Company_Code = 'EAI' " +
                    "AND Cost_Type = 'L' " +
                    "AND Job > @startRange " +
                    "GROUP BY Job, Phase";
        }
        public SOMType SOMType => SOMType.HoursEntry;
        public TargetDatabase TargetDatabase => TargetDatabase.Spectrum;


        object ICacheable.GetType()
        {
            return SOMTypeConverter.ConvertToClass(this.SOMType);
        }
    }
}
