namespace BGuidinger.Base
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public static partial class Extensions
    {
        public static Dictionary<string, dynamic> Parse(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var settings = new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, dynamic>), settings);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return serializer.ReadObject(stream) as Dictionary<string, dynamic>;
            }
        }
        public static string ToJson(this Dictionary<string, dynamic> dictionary)
        {
            var settings = new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            };

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, dynamic>), settings);
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                serializer.WriteObject(stream, dictionary);
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static TType GetValue<TType>(this Dictionary<string, dynamic> config, string key)
        {
            if (config.ContainsKey(key))
            {
                return (TType)config[key];
            }
            else
            {
                return default;
            }
        }

        public static string UrlEncode(this Dictionary<string, dynamic> parameters)
        {
            return string.Join("&", parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
        }
    }
}