using SOM.BudgetVSTO.Enums;
using SOM.BudgetVSTO.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO
{
    internal class SOMTypeConverter
    {
        public static object ConvertToClass(SOMType somType)
        {
            switch (somType)
            {
                case SOMType.HoursEntry:
                    return new HoursEntry();
                case SOMType.SystemEntry:
                    return new SystemEntry();
                default:
                    return null;
            }
        }
    }
}