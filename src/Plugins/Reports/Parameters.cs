namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk.Query;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    public class Parameters : Plugin
    {
        public Parameters(string unsecure, string secure) : base(unsecure, secure) { }

        public override void OnExecute(IPluginProvider provider)
        {
            // Get services
            var logger = provider.LoggingService;
            var service = provider.OrganizationService;
            var context = provider.ExecutionContext;

            var report = service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet("originalbodytext"));
            var body = report.GetAttributeValue<string>("originalbodytext");

            var serializer = new XmlSerializer(typeof(Report));
            using (var reader = new StringReader(body))
            {
                var r = serializer.Deserialize(reader) as Report;

                context.OutputParameters["Parameters"] = string.Join(", ", r.ReportParameters.Select(x => x.Name));
            }
        }
    }
}