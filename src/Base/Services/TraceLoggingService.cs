namespace BGuidinger.Base
{
    using System;
    using Microsoft.Xrm.Sdk;

    public class TraceLoggingService : ILoggingService
    {
        private readonly ITracingService _tracingService;

        public TraceLoggingService(ITracingService tracingService)
        {
            _tracingService = tracingService;
        }

        public void Write(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (_tracingService != null)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss.fff");
                _tracingService.Trace($"{timestamp} - {message}");
            }
        }

        public void Write(EntityReference entity)
        {
            Write($"Entity Reference: <{entity.LogicalName}> ({entity.Id.ToString("X")})");
        }

        public void Write(Entity entity)
        {
            Write(entity.ToEntityReference());
        }

        public void Write(Exception ex)
        {
            Write(ex.Message);
        }
    }
}