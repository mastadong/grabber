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

        /// <summary>
        /// Deconverts from string somType to enum SOMType
        /// </summary>
        /// <param name="somType"></param>
        /// <returns></returns>
        public static SOMType DeconvertStringToSomeType(string somType)
        {
            SOMType matchedCase;

            if (Enum.TryParse(somType, out matchedCase))
            {
                switch (matchedCase)
                {
                    case SOMType.HoursEntry:
                        return SOMType.HoursEntry;
                    case SOMType.SystemEntry:
                        return SOMType.SystemEntry;
                    default:
                        return SOMType.Nothing;
                }
            }
            else
            {
                throw new Exception("Unable to deconvert SOMType: " + somType);

            }

        }
    }
}