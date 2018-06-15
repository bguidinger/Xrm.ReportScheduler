namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public class Renderer
    {
        private CookieContainer _cookies = new CookieContainer();

        private readonly Token _token;

        public Renderer(string resource, string username, string password)
        {
            _token = GetToken(resource, username, password);
        }

        public byte[] RenderReport(Entity report, string format, string parameters)
        {
            var session = GetReportSession(report, parameters);

            var url = "/Reserved.ReportViewerWebControl.axd";
            var lcid = report.GetAttributeValue<int>("languagecode");
            format = format.ToUpper();
            if (format == "WORD" || format == "EXCEL")
            {
                format = format + "OPENXML";
            }
            var data = new Dictionary<string, dynamic>()
            {
                ["OpType"] = "Export",
                ["Format"] = format,
                ["ContentDisposition"] = "AlwaysAttachment",
                ["FileName"] = string.Empty,
                ["Culture"] = lcid.ToString(),
                ["CultureOverrides"] = "False",
                ["UICulture"] = lcid.ToString(),
                ["UICultureOverrides"] = "False",
                ["ReportSession"] = session.Item1,
                ["ControlID"] = session.Item2
            };

            return GetResponse(GetRequest("GET", $"{url}?{data.UrlEncode()}"));
        }

        public string RenderExcelTemplate(Guid templateId, Guid viewId)
        {
            var url = "/api/data/v9.0/RenderTemplateFromView";
            var data = "{ \"Template\": { \"@odata.type\": \"Microsoft.Dynamics.CRM.documenttemplate\", \"documenttemplateid\": \"" + templateId.ToString("D") + "\"  }, \"View\": { \"@odata.type\": \"Microsoft.Dynamics.CRM.savedquery\", \"savedqueryid\": \"" + viewId.ToString("D") + "\"  } }";

            var request = GetRequest("POST", url, data, true);
            try
            {
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var body = reader.ReadToEnd().Parse();
                    return body["ExcelFile"];
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.Write(reader.ReadToEnd());
                    throw;
                }
            }
        }

        public string RenderWordTemplate(Guid templateId, Guid entityId, int entityTypeCode)
        {
            var url = "/api/data/v9.0/ExportWordDocument";
            var data = "{ \"EntityTypeCode\": " + entityTypeCode + ", \"SelectedRecords\": \"[ '{" + entityId.ToString("D") + "}' ]\", \"SelectedTemplate\": { \"@odata.type\": \"Microsoft.Dynamics.CRM.documenttemplate\", \"documenttemplateid\": \"" + templateId.ToString("D") + "\" } }";

            var request = GetRequest("POST", url, data, true);

            try
            {
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var body = reader.ReadToEnd().Parse();
                    return body["WordFile"];
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.Write(reader.ReadToEnd());
                    throw;
                }
            }
        }

        private Token GetToken(string resource, string username, string password)
        {
            var uri = $"https://login.microsoftonline.com/common/oauth2/token";

            var parameters = new Dictionary<string, dynamic>()
            {
                ["grant_type"] = "password",
                ["client_id"] = "2ad88395-b77d-4561-9441-d0e40824f9bc",
                ["username"] = username,
                ["password"] = password,
                ["resource"] = resource,
            };
            var request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            var body = Encoding.ASCII.GetBytes(parameters.UrlEncode());

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = body.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
            }

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(Token));
                return serializer.ReadObject(stream) as Token;
            }
        }

        private Tuple<string, string> GetReportSession(Entity report, string parameters)
        {
            var name = report.GetAttributeValue<string>("reportnameonsrs");
            var isCustom = name == null;

            var url = "/CRMReports/RSViewer/ReportViewer.aspx";
            var data = new Dictionary<string, dynamic>()
            {
                ["id"] = report.Id.ToString("B"),
                ["iscustomreport"] = isCustom.ToString().ToLower(),
                ["reportnameonsrs"] = name
            };

            if (!string.IsNullOrEmpty(parameters))
            {
                if (isCustom)
                {
                    foreach (var parameter in parameters.Parse())
                    {
                        data.Add($"p:{parameter.Key}", parameter.Value);
                    }
                }
                else
                {
                    var filter = new StringBuilder();
                    filter.Append("<ReportFilter>");
                    foreach (var parameter in parameters.Parse())
                    {
                        filter.Append($"<ReportEntity paramname=\"{parameter.Key}\">{parameter.Value}</ReportEntity>");
                    }
                    filter.Append("</ReportFilter>");
                    data.Add("CRM_Filter", filter.ToString());
                }
            }
            else
            {
                if (!isCustom)
                {
                    var defaultFilter = report.GetAttributeValue<string>("defaultfilter");
                    data.Add("CRM_Filter", defaultFilter);
                }
            }

            var response = Encoding.UTF8.GetString(GetResponse(GetRequest("POST", url, data.UrlEncode())));

            var sessionId = response.Substring(response.LastIndexOf("ReportSession=") + 14, 24);
            var controlId = response.Substring(response.LastIndexOf("ControlID=") + 10, 32);

            return new Tuple<string, string>(sessionId, controlId);
        }

        private HttpWebRequest GetRequest(string method, string url, string data = null, bool isJson = false)
        {
            var request = WebRequest.CreateHttp($"{_token.Resource}{url}");
            request.Method = method;
            request.CookieContainer = _cookies;
            if (_token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
            }
            request.AutomaticDecompression = DecompressionMethods.GZip;

            if (string.IsNullOrEmpty(data) == false)
            {
                if (isJson)
                {
                    var body = Encoding.ASCII.GetBytes(data);

                    request.ContentType = "application/json";
                    request.ContentLength = body.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(body, 0, body.Length);
                    }
                }
                else
                {
                    var body = Encoding.ASCII.GetBytes(data);

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = body.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(body, 0, body.Length);
                    }
                }
            }

            return request;
        }

        private byte[] GetResponse(HttpWebRequest request)
        {
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.ResponseUri.PathAndQuery.Contains("errorhandler.aspx"))
                {
                    throw new Exception("Error executing request.");
                }

                using (var stream = response.GetResponseStream())
                using (var stream2 = new MemoryStream())
                {
                    stream.CopyTo(stream2);
                    return stream2.ToArray();
                }
            }
        }
    }
}