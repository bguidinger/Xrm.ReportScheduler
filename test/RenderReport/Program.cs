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
            TestWordTemplate();
            TestExcelTemplate();
        }

        private static void TestStandardReport()
        {
            var report = new EntityReference("report", new Guid("08f936bd-c017-e811-a97f-000d3a192387"));
            var parameters = new ParameterCollection
            {
                ["Format"] = "PDF",
                ["Parameters"] = "{\"CRM_FilteredSystemUser\": \"<fetch version=\\\"1.0\\\" output-format=\\\"xml-platform\\\" mapping=\\\"logical\\\" distinct=\\\"false\\\"><entity name=\\\"systemuser\\\"><all-attributes /></entity></fetch>\"}"
            };
            RenderReport(report, parameters, "standard.pdf");
        }

        private static void TestCustomReport()
        {
            var report = new EntityReference("report", new Guid("3485F701-9146-E811-A95F-000D3A109D70"));
            var parameters = new ParameterCollection
            {
                ["Format"] = "PDF",
                ["Parameters"] = "{\"Color\": \"#FFFF00\"}"
            };
            RenderReport(report, parameters, "custom.pdf");
        }

        private static void TestWordTemplate()
        {
            var template = new EntityReference("documenttemplate", new Guid("9B77C5B0-1033-4741-A01C-AFDBDB1C3F22"));
            var parameters = new ParameterCollection
            {
                ["RecordId"] = "B5C0BF5E-3A2C-E811-A951-000D3A30D0CA"
            };
            RenderReport(template, parameters, "custom.docx");
        }
        private static void TestExcelTemplate()
        {
            var template = new EntityReference("documenttemplate", new Guid("C6CB7810-1033-47C3-95AA-F0FFF0A8D57D"));
            var parameters = new ParameterCollection
            {
                ["SavedView"] = new EntityReference("savedquery", new Guid("00000000-0000-0000-00AA-000010001030"))
            };
            RenderReport(template, parameters, "custom.xlsx");
        }

        private static void RenderReport(EntityReference target, ParameterCollection parameters, string filename)
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
                Target = target
            };

            var request = new Render(null, secure.ToJson());
            request.OnExecute(provider);
            File.WriteAllBytes(filename, Convert.FromBase64String(provider.ExecutionContext.OutputParameters["Output"].ToString()));
        }
    }
}
