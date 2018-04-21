namespace BGuidinger.Base
{
    using Microsoft.Xrm.Sdk;
    using System;

    public abstract class Plugin : IPlugin
    {
        public readonly Configuration UnsecureConfig;
        public readonly Configuration SecureConfig;

        public Plugin(string unsecureConfig = null, string secureConfig = null)
        {
            UnsecureConfig = Configuration.Deserialize(unsecureConfig);
            SecureConfig = Configuration.Deserialize(secureConfig);
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = new PluginProvider(serviceProvider);

            try
            {
                context.LoggingService.Write("Starting execution.");
                OnExecute(context);
            }
            catch (Exception ex)
            {
                context.LoggingService.Write(ex);

                if (ex is InvalidPluginExecutionException == false)
                {
                    throw new InvalidPluginExecutionException("Something went wrong.", ex);
                }               
            }
            finally
            {
                context.LoggingService.Write("Stopping execution.");
            }
        }
        public abstract void OnExecute(IPluginProvider provider);
    }
}