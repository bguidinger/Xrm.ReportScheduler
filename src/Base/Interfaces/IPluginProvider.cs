namespace BGuidinger.Base
{
    using Microsoft.Xrm.Sdk;

    public interface IPluginProvider
    {
        IPluginExecutionContext ExecutionContext { get; }
        ILoggingService LoggingService { get; }

        EntityReference Target { get; }
    }
}