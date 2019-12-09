namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Extensions;
    using Microsoft.Xrm.Sdk.Query;
    using System;

    public class Render : Plugin
    {
        public Render(string unsecure, string secure) : base(unsecure, secure) { }

        public override void OnExecute(IPluginProvider provider)
        {
            if (SecureConfig == null)
            {
                throw new ArgumentNullException(nameof(SecureConfig));
            }

            // Get configuration values
            var resource = SecureConfig.GetValue<string>("resource");
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }
            var username = SecureConfig.GetValue<string>("username");
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }
            var password = SecureConfig.GetValue<string>("password");
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }


            // Get services
            var service = provider.OrganizationService;
            var context = provider.ExecutionContext;
            var target = provider.Target;
            var logger = provider.LoggingService;

            var renderer = new Renderer(logger);
            renderer.Authenticate(resource, username, password);

            // Render the report
            var output = string.Empty;
            switch (target.LogicalName)
            {
                case "report":
                    var reportColumns = new ColumnSet("reportnameonsrs", "languagecode", "defaultfilter");
                    var report = service.Retrieve(target.LogicalName, target.Id, reportColumns);
                    var format = context.InputParameterOrDefault<string>("Format");
                    var parameters = context.InputParameterOrDefault<string>("Parameters");
                    var rendered = renderer.RenderReport(report, format, parameters);
                    output = Convert.ToBase64String(rendered);
                    break;
                case "documenttemplate":
                    var templateColumns = new ColumnSet("documenttype", "associatedentitytypecode");
                    var template = service.Retrieve(target.LogicalName, target.Id, templateColumns);

                    var callerId = context.InputParameterOrDefault<string>("CallerId");

                    switch (template.GetAttributeValue<OptionSetValue>("documenttype")?.Value)
                    {
                        case 1:
                            var savedView = Guid.Parse(context.InputParameterOrDefault<string>("ViewId"));
                            output = renderer.RenderExcelTemplate(template.Id, savedView, callerId);
                            break;
                        case 2:
                            var typeCode = template.GetAttributeValue<string>("associatedentitytypecode");
                            var metadata = service.GetEntityMetadata(typeCode);
                            var recordId = Guid.Parse(context.InputParameterOrDefault<string>("RecordId"));
                            output = renderer.RenderWordTemplate(template.Id, recordId, metadata.ObjectTypeCode ?? 0, callerId);
                            break;
                        default:
                            throw new Exception("Invalid document template type.");
                    }
                    break;
                default:
                    throw new Exception("Invalid input target.");
            }
            // Return as Base-64
            provider.ExecutionContext.OutputParameters["Output"] = output;
        }
    }
}