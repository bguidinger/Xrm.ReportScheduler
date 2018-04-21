namespace BGuidinger.Base
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class Configuration : Dictionary<string, dynamic>
    {
        public static Configuration Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var settings = new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Configuration), settings);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return serializer.ReadObject(stream) as Configuration;
            }
        }
    }
}