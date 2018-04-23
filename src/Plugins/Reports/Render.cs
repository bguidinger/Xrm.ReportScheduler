namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk.Extensions;
    using System;

    public class Render : Plugin
    {
        public Render(string unsecure, string secure) : base(unsecure, secure) { }

        public override void OnExecute(IPluginProvider provider)
        {
            // Get configuration values
            var resource = SecureConfig.GetValue<string>("resource");
            var username = SecureConfig.GetValue<string>("username");
            var password = SecureConfig.GetValue<string>("password");

            // Get input parameters
            var reportId = provider.Target.Id;
            var format = provider.ExecutionContext.InputParameterOrDefault<string>("Format").ToUpper();
            if (format == "WORD" || format == "EXCEL")
            {
                format = format + "OPENXML";
            }
            var parameters = provider.ExecutionContext.InputParameterOrDefault<string>("Parameters");

            // Render the report
            var renderer = new Renderer(resource, username, password);
            var report = renderer.Render(reportId, format, parameters);

            // Return as Base-64
            provider.ExecutionContext.OutputParameters["Output"] = Convert.ToBase64String(report);
        }
    }
}