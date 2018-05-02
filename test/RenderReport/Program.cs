namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Tooling.Connector;
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            TestStandardReport();
            TestCustomReport();
        }

        private static void TestStandardReport()
        {
            var reportId = new Guid("FCF836BD-C017-E811-A97F-000D3A192387");
            var parameters = new ParameterCollection
            {
                ["Format"] = "PDF",
                ["Parameters"] = "{\"CRM_FilteredAccount\": \"<fetch version=\\\"1.0\\\" output-format=\\\"xml-platform\\\" mapping=\\\"logical\\\" distinct=\\\"false\\\"><entity name=\\\"account\\\"><all-attributes/><filter type=\\\"and\\\"><condition attribute=\\\"accountid\\\" operator=\\\"eq\\\" uiname=\\\"Al Rossi\\\" uitype=\\\"account\\\" value=\\\"{9C662EA7-4E23-E811-A95D-000D3A109D70}\\\"/></filter></entity></fetch>\"}"
            };
            RenderReport(reportId, parameters, "standard.pdf");
        }

        private static void TestCustomReport()
        {
            var reportId = new Guid("3485F701-9146-E811-A95F-000D3A109D70");
            var parameters = new ParameterCollection
            {
                ["Format"] = "PDF",
                ["Parameters"] = "{\"Color\": \"#FFFF00\"}"
            };
            RenderReport(reportId, parameters, "custom.pdf");
        }

        private static void RenderReport(Guid reportId, ParameterCollection parameters, string filename)
        {
            var secure = new Dictionary<string, dynamic>()
            {
                ["resource"] = "https://organization.crm.dynamics.com",
                ["username"] = "admin@organization.onmicrosoft.com",
                ["password"] = "Pass@word!",
            };

            var connectionString = $"AuthType=Office365; Username={secure["username"]}; Password={secure["password"]}; Url={secure["resource"]};";
            var service = new CrmServiceClient(connectionString);

            var context = new FakePluginExecutionContext
            {
                InputParameters = parameters
            };

            var provider = new FakePluginProvider
            {
                ExecutionContext = context,
                OrganizationService = service,
                Target = new EntityReference("report", reportId)
            };

            var request = new Render(null, secure.ToJson());
            request.OnExecute(provider);
            File.WriteAllBytes(filename, Convert.FromBase64String(provider.ExecutionContext.OutputParameters["Output"].ToString()));
        }
    }
}
