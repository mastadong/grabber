using Jumble.ExternalCacheManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jumble.ExternalCacheManager.Model
{
    [Serializable]
    public class DataFile : CacheObject
    {
       //TODO: Additional parameter is needed to identify the purpose of the datafile.  This is used in combination with 
       //other properties to uniquely identify the file regardless of filename. 
        public string SOMType { get; set; }
        public string DirectoryPath { get; set; }
        public string FileName { get; set; }
        private string _fullFilePath;
        public string FullFilePath
        {
            get { return _fullFilePath; }
            set { _fullFilePath = value; }
        }

        public Purview Purview { get; set; }

        //METHODS 
        public DataFile()
        {
        }

        public DataFile(Purview methodPurview, string fullFilePath, string somType)
        {
            Purview = methodPurview;
            SOMType = somType;
            _fullFilePath = fullFilePath;

            FileName = Path.GetFileName(_fullFilePath);
            DirectoryPath = Path.GetDirectoryName(_fullFilePath);
        }

    }
}
