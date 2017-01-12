//Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Geocode
{
    /// <summary>
    /// Generic methods for serializing and deserializing objects
    /// to/from JSON string format
    /// </summary>
    class ObjectSerialization
    {
        /// <summary>
        /// Given an object in JSON form, deserialize the string to create
        /// an object instance
        /// </summary>
        /// <typeparam name="T">The type of object to hydrate</typeparam>
        /// <param name="json">The Json string representation of the object</param>
        /// <returns>An instance of the type T</returns>
        public static T JsonToObject<T>(string json)
        {
            if (null == json || json == String.Empty)
                return default(T);//null
            return JsonToObject<T>(System.Text.Encoding.Unicode.GetBytes(json));
        }
        /// <summary>
        /// Given an object in JSON form, deserialize the bytes to create an
        /// object instance
        /// </summary>
        /// <typeparam name="T">The type of object to hydrate</typeparam>
        /// <param name="bytes">The Json string representation of the object</param>
        /// <returns>An instance of the type T</returns>
        public static T JsonToObject<T>(Byte[] bytes)
        {
            if (null == bytes || bytes.Length == 0)
                return default(T);//null

            T obj = default(T);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
                obj = (T)ds.ReadObject(ms);
            }

            return obj;

        }
        /// <summary>
        /// Given an object of type T, serialize it into a Json
        /// string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">The instance of type T to be serialized</param>
        /// <returns>A Json string</returns>
        public static string ObjectToJson<T>(T obj)
        {
            if (null == obj)
                return "";
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(obj.GetType());
            ds.WriteObject(ms, obj);
            ms.Position = 0;
            StreamReader stmReader = new StreamReader(ms);
            string json = stmReader.ReadToEnd();
            ms.Close();
            return json;
        }

    }
}
