namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;

    public class FakePluginProvider : IPluginProvider
    {
        public IPluginExecutionContext ExecutionContext { get; set; }

        public IOrganizationService OrganizationService { get; set; }

        public ILoggingService LoggingService { get; set; }

        public EntityReference Target { get; set; }
    }
}
