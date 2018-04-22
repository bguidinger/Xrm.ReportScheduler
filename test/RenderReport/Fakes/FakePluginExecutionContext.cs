namespace BGuidinger.Reports
{
    using Microsoft.Xrm.Sdk;
    using System;

    class FakePluginExecutionContext : IPluginExecutionContext
    {
        public Guid? RequestId => throw new NotImplementedException();

        public ParameterCollection InputParameters { get; set; } = new ParameterCollection();

        public ParameterCollection OutputParameters { get; set; } = new ParameterCollection();

        public string MessageName => throw new NotImplementedException();

        public string PrimaryEntityName => throw new NotImplementedException();

        public string SecondaryEntityName => throw new NotImplementedException();
        public int Stage => throw new NotImplementedException();

        public IPluginExecutionContext ParentContext => throw new NotImplementedException();

        public int Mode => throw new NotImplementedException();

        public int IsolationMode => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public ParameterCollection SharedVariables => throw new NotImplementedException();

        public Guid UserId => throw new NotImplementedException();

        public Guid InitiatingUserId => throw new NotImplementedException();

        public Guid BusinessUnitId => throw new NotImplementedException();

        public Guid OrganizationId => throw new NotImplementedException();

        public string OrganizationName => throw new NotImplementedException();

        public Guid PrimaryEntityId => throw new NotImplementedException();

        public EntityImageCollection PreEntityImages => throw new NotImplementedException();

        public EntityImageCollection PostEntityImages => throw new NotImplementedException();

        public EntityReference OwningExtension => throw new NotImplementedException();

        public Guid CorrelationId => throw new NotImplementedException();

        public bool IsExecutingOffline => throw new NotImplementedException();

        public bool IsOfflinePlayback => throw new NotImplementedException();

        public bool IsInTransaction => throw new NotImplementedException();

        public Guid OperationId => throw new NotImplementedException();

        public DateTime OperationCreatedOn => throw new NotImplementedException();
    }
}
