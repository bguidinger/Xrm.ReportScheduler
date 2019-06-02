namespace BGuidinger.Reports
{
    using Base;
    using Microsoft.Xrm.Sdk;
    using System;

    public class FakeTracingService : ITracingService
    {
        public void Trace(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
}
