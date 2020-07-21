using Jumble.ExternalCacheManager.Managers;
using Jumble.ExternalCacheManager.Model;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Jumble.ExternalCacheManager.Enums;

namespace Jumble.ExternalCacheManager.Services
{
    public class ConnectionStringService
    {
        /// <summary>
        /// Returns the default database connection string for the application. 
        /// </summary>
        /// <returns></returns>
        public string Get(TargetDatabase targetDatabase)
        {
            bool ConnectionStringLocated = false;
            CacheMap cache = CacheMap.RawLoad();
            if (File.Exists(cache.connStrFilePath))
            { 
                XMLSerializingService<List<KeyPairEntry>> xmlSerializingService = new XMLSerializingService<List<KeyPairEntry>>();
                List<KeyPairEntry> connectionStrings =  xmlSerializingService.DeserializeObject(cache.connStrFilePath);
                foreach(KeyPairEntry k in connectionStrings)
                {
                    if(k.key.ToString() == targetDatabase.ToString())
                    {
                        CryptoService cryptoService = new CryptoService();
                        string decryptedString = cryptoService.Decrypt(k.value.ToString());
                        ConnectionStringLocated = true;
                        return decryptedString;
                    }
                }

                if(ConnectionStringLocated == false)
                {
                    Console.WriteLine("Database connection string for " + targetDatabase.ToString() + " could not be retrieved from file.");
                }

            }
            else
            {
                Console.WriteLine("The Connection String configuration file :" + cache.connStrFilePath + " could not be located");
            }

            return null;
        }

    }
}
