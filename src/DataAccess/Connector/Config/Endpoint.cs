namespace DataAccess.Connector.Config
{
    using System;

    public class Endpoint
    {
        public string Type
        {
            get;
            set;
        }

        public HttpEndpoint HttpEndPointEntity
        {
            get;
            set;
        }

        public EndpointType GetEndPointType()
        {
            return (EndpointType)Enum.Parse(typeof(EndpointType), Type);
        }

        public void Validate()
        {
            return;
        }
    }
}