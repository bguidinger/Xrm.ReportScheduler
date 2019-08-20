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

        private readonly ILoggingService _logger;

        private Token _token;

        public Renderer(ILoggingService logger)
        {
            _logger = logger;
        }

        public void Authenticate(string resource, string username, string password)
        {
            _logger.Write($"Authenticating....");
            _token = GetToken(resource, username, password);
            _logger.Write($"Authenticated.");
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

        public string RenderExcelTemplate(Guid templateId, Guid viewId, string callerId)
        {
            var url = "/api/data/v9.0/RenderTemplateFromView";
            var data = "{ \"Template\": { \"@odata.type\": \"Microsoft.Dynamics.CRM.documenttemplate\", \"documenttemplateid\": \"" + templateId.ToString("D") + "\"  }, \"View\": { \"@odata.type\": \"Microsoft.Dynamics.CRM.savedquery\", \"savedqueryid\": \"" + viewId.ToString("D") + "\"  } }";

            var request = GetRequest("POST", url, data, true, callerId);
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
                    _logger.Write(reader.ReadToEnd());
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
                    _logger.Write(reader.ReadToEnd());
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
                Dictionary<string, dynamic> parsedParameters;
                try
                {
                    parsedParameters = parameters.Parse();
                }
                catch (Exception)
                {
                    throw new Exception("Unable to parse report parameters.  Please ensure they are in JSON format.");
                }

                if (isCustom)
                {
                    foreach (var parameter in parsedParameters)
                    {
                        data.Add($"p:{parameter.Key}", parameter.Value);
                    }
                }
                else
                {
                    var filter = new StringBuilder();
                    filter.Append("<ReportFilter>");
                    foreach (var parameter in parsedParameters)
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

            if (response.Contains("ReportSession=") && response.Contains("ControlID="))
            {
                var sessionId = response.Substring(response.LastIndexOf("ReportSession=") + 14, 24);
                var controlId = response.Substring(response.LastIndexOf("ControlID=") + 10, 32);

                return new Tuple<string, string>(sessionId, controlId);
            }
            else
            {
                throw new Exception("Error while getting report session.  This is most likely an issue with invalid report parameters.");
            }
        }

        private HttpWebRequest GetRequest(string method, string url, string data = null, bool isJson = false, string callerId = null)
        {
            var request = WebRequest.CreateHttp($"{_token.Resource}{url}");
            request.Method = method;
            request.CookieContainer = _cookies;
            if (_token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
            }
            if (callerId != null)
            {
                request.Headers.Add("MSCRMCallerID", $"{callerId}");
            }
            request.AutomaticDecompression = DecompressionMethods.GZip;

            _logger.Write($"{request.Method} {request.RequestUri.ToString()}");
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

                //_logger.Write(data);
            }

            return request;
        }

        private byte[] GetResponse(HttpWebRequest request)
        {
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.ResponseUri.PathAndQuery.Contains("errorhandler.aspx"))
                {
                    var queryString = response.ResponseUri.Query.UrlDecode();

                    if (queryString.ContainsKey("Parm0"))
                    {
                        _logger.Write($"Parm0: {queryString["Parm0"]}");
                    }
                    if (queryString.ContainsKey("Parm1"))
                    {
                        _logger.Write($"Parm1: {queryString["Parm1"]}");
                    }

                    throw new Exception($"Error executing web request: {request.RequestUri}");
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
