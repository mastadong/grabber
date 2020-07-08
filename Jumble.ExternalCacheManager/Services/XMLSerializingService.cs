using Jumble.ExternalCacheManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using Jumble.ExternalCacheManager.Enums;

namespace Jumble.ExternalCacheManager.Services
{
    public class XMLSerializingService<T> 
    {
       
            public bool SerializeObject(T dataObject, string fileName)
            {
                try
                {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(T)) ;

                    using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        xmlFormat.Serialize(fStream, dataObject);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                    return false;
                }
            }

            public T DeserializeObject(string fileName)
            {
                T deserializedDataObject;

                try
                {
                    XmlSerializer xmlFormat = new XmlSerializer(typeof(T));

                    using (Stream fStream = File.OpenRead(fileName))
                    {
                        deserializedDataObject = (T)xmlFormat.Deserialize(fStream);
                        return deserializedDataObject;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());

                    throw new Exception("Failed to deserialize data object(s) from " + fileName.ToString());
                }
            }
        
    }
}