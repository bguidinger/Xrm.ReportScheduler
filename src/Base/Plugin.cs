namespace BGuidinger.Base
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;

    public abstract class Plugin : IPlugin
    {
        public readonly Dictionary<string, dynamic> UnsecureConfig;
        public readonly Dictionary<string, dynamic> SecureConfig;

        public Plugin(string unsecureConfig = null, string secureConfig = null)
        {
            UnsecureConfig = unsecureConfig.Parse();
            SecureConfig = secureConfig.Parse();
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