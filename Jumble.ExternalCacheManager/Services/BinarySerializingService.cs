using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Jumble.ExternalCacheManager.Model;

namespace Jumble.ExternalCacheManager.Services
{
    public class BinarySerializingService<T> where T : CacheObject 
    {
        public bool SerializeObject(T dataObject, string fileName)
        {
            try
            {
                BinaryFormatter binFormat = new BinaryFormatter();

                using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormat.Serialize(fStream, dataObject);
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
                BinaryFormatter binFormat = new BinaryFormatter();

                using (Stream fStream = File.OpenRead(fileName))
                {
                    deserializedDataObject = (T)binFormat.Deserialize(fStream);
                    return deserializedDataObject;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                
                throw new Exception("Failed to deserialize data object(s) from " + fileName.ToString());
            }
        }


        public bool SerializeList(List<T> objectList, string fileName)
        {
            bool serializationResult = true;

            try
            {






            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;
            }




            return serializationResult;
        }




    }
}
