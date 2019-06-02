namespace BGuidinger.Reports
{
    using System;
    using System.Collections.Generic;

    public class FakeServiceProvider : IServiceProvider
    {
        public Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<TService>(TService service)
        {
            _services.Add(typeof(TService), service);
        }

        public object GetService(Type serviceType)
        {
            return _services[serviceType];
        }
    }
}