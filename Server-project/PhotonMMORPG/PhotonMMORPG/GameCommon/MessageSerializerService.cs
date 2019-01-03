﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace GameCommon
{
    public class MessageSerializerService
    {
        public static object SerializeObjectOfType<T>(T objectToSerialize) where T : class
        {
            object retrunValue = null;
#if DEBUG
            retrunValue = SerializeJson<T>(objectToSerialize);
#else
            retrunValue = SerializeBson<T>(objectToSerialize);

#endif
            return retrunValue;
        }

        protected static T DeserializeObjectOfType<T>(object objectToDeserialize) where T : class
        {
            T returnValue = null;
#if DEBUG
            returnValue = DeserializeJson<T>(objectToDeserialize);
#else
            returnValue = DeserializeBson<T>(objectToDeserialize);
#endif
            return returnValue;
        }

        #region Json
        protected static object SerializeJson<T>(T objectToSerialize) where T : class
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        public static T DeserializeJson<T>(object objectToDeserialize) where T : class
        {
            return JsonConvert.DeserializeObject<T>((string)objectToDeserialize);
        }
        #endregion

        #region Bson
        protected static object SerializeBson<T>(T objectToSerialize) where T : class
        {
            MemoryStream ms = new MemoryStream();
            using (BsonDataWriter writer = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, objectToSerialize);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        protected static T DeserializeBson<T>(object objectToDeserialize) where T : class
        {
            byte[] data = Convert.FromBase64String((string)objectToDeserialize);
            MemoryStream ms = new MemoryStream(data);
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
        #endregion

    }
}
