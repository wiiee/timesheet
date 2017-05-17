namespace DataAccess.Connector.Config
{
    using System;

    public class ServiceDescriptor
    {
        public string RequestType
        {
            get;
            set;
        }

        public string ResponseType
        {
            get;
            set;
        }

        public Endpoint EndpointEntity
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(RequestType) || string.IsNullOrEmpty(ResponseType) || string.IsNullOrEmpty(Name))
            {
                //log error
            }
            else
            {
                RequestType = RequestType.Trim();
                ResponseType = ResponseType.Trim();
                Name = Name.Trim();
            }

            // validate endpoint
            try
            {
                this.EndpointEntity.Validate();
            }
            catch (Exception)
            {
                //log error
            }
        }
    }
}