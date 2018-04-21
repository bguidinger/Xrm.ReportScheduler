namespace BGuidinger.Base
{
    using Microsoft.Xrm.Sdk;
    using System;

    public interface ILoggingService
    {
        void Write(string message);
        void Write(Entity entity);
        void Write(Exception ex);

        void Write(EntityReference entity);
    }
}