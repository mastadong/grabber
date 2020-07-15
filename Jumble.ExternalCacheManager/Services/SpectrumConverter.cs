using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jumble.ExternalCacheManager.Services
{
    public class SpectrumConverter
    {
        /// <summary>
        /// Convert a value into a SPECTRUM-compatible format.
        /// </summary>
        /// <param name="dataValue"></param>
        /// <param name="spectrumFormat"></param>
        /// <returns></returns>
        public string ConvertToSpectrumFormat(string dataValue, SpectrumFormat spectrumFormat)
        {
            switch (spectrumFormat)
            {
                case SpectrumFormat.JobNumber:
                    //Job numbers must be padded with 3 leading spaces in order to be recognized by SPECTRUM.
                    dataValue = "   " + dataValue;
                    return dataValue;
                case SpectrumFormat.PhaseCode:
                    return dataValue;
                default:
                    return "";
            }
            
        }

        /// <summary>
        /// Convert a value from a SPECTRUM format into one compatible with other EAI databases and user applications.
        /// </summary>
        /// <param name="dataValue"></param>
        /// <param name="spectrumFormat"></param>
        /// <returns></returns>
        public string ConvertFromSpectrumFormat(string dataValue, SpectrumFormat spectrumFormat)
        {
            switch (spectrumFormat)
            {
                case SpectrumFormat.JobNumber:
                    //Job numbers from SPECTRUM are padded with 3 leading spaces.  These should be removed before interfacing 
                    //with other databases or with users.
                    dataValue = dataValue.Remove(0, 3);
                    return dataValue;
                case SpectrumFormat.PhaseCode:
                    dataValue = dataValue.Trim();
                    return dataValue;
                default:
                    return "";
            }
        }

    }
}
