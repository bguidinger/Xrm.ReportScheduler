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
        private static readonly Dictionary<string, dynamic> SecureConfig = new Dictionary<string, dynamic>()
        {
            ["resource"] = "https://organization.crm.dynamics.com",
            ["username"] = "admin@organization.onmicrosoft.com",
            ["password"] = "Pass@word!",
        };

        static void Main(string[] args)
        {
            TestStandardReport();
            TestCustomReport();
            TestWordTemplate();
            TestExcelTemplate();
            TestReportParameters();
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
        private static void TestReportParameters()
        {
            var report = new EntityReference("report", new Guid("2A167C40-5D4D-E911-A815-000D3A37F60D"));
            GetReportParameters(report);
        }

        private static FakeServiceProvider GetServiceProvider()
        {
            var connectionString = $"AuthType=Office365; Username={SecureConfig["username"]};Password={SecureConfig["password"]};Url={SecureConfig["resource"]};";
            var service = new CrmServiceClient(connectionString);

            var serviceProvider = new FakeServiceProvider();

            var factory = new FakeOrganizationServiceFactory(service);
            serviceProvider.AddService<IOrganizationServiceFactory>(factory);

            var tracing = new FakeTracingService();
            serviceProvider.AddService<ITracingService>(tracing);

            return serviceProvider;
        }

        private static void RenderReport(EntityReference target, ParameterCollection parameters, string filename)
        {
            var serviceProvider = GetServiceProvider();

            parameters.Add("Target", target);
            var context = new FakePluginExecutionContext
            {
                InputParameters = parameters,
            };

            serviceProvider.AddService<IPluginExecutionContext>(context);

            var request = new Render(null, SecureConfig.ToJson());
            request.Execute(serviceProvider);
            File.WriteAllBytes(filename, Convert.FromBase64String(context.OutputParameters["Output"].ToString()));
        }

        private static void GetReportParameters(EntityReference report)
        {
            var serviceProvider = GetServiceProvider();

            var context = new FakePluginExecutionContext
            {
                PrimaryEntityName = report.LogicalName,
                PrimaryEntityId = report.Id
            };
            serviceProvider.AddService<IPluginExecutionContext>(context);

            var request = new Parameters(null, SecureConfig.ToJson());
            request.Execute(serviceProvider);

            Console.WriteLine(context.OutputParameters["Parameters"]);
        }
    }
}
