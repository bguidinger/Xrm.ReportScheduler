namespace BGuidinger.Base
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public static partial class Extensions
    {
        public static TType GetValue<TType>(this Configuration config, string key)
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

        public static void Write(this WebRequest request, Dictionary<string, string> parameters)
        {
            var body = Encoding.UTF8.GetBytes(parameters.UrlEncode());

            request.ContentLength = body.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
            }
        }

        public static string UrlEncode(this Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
        }
    }
}