namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            var context = new FakePluginExecutionContext();
            context.InputParameters["Format"] = "PDF";
            context.InputParameters["Parameters"] = "{\"Color\": \"#00FF00\"}";

            var provider = new FakePluginProvider
            {
                ExecutionContext = context,
                Target = new EntityReference("report", new Guid("2B5E5311-2546-E811-A95F-000D3A109D70"))
            };

            var secure = new Dictionary<string, dynamic>()
            {
                ["resource"] = "https://organization.crm.dynamics.com",
                ["username"] = "admin@organization.onmicrosoft.com",
                ["password"] = "Pass@word!",
            };

            var request = new Render(null, secure.ToJson());
            request.OnExecute(provider);
            File.WriteAllBytes(@"report.pdf", Convert.FromBase64String(provider.ExecutionContext.OutputParameters["Output"].ToString()));
        }
    }
}
