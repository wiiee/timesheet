namespace DataAccess.Connector.Config
{
    using System.Collections.Generic;

    public class HttpEndpoint
    {
        public string Url
        {
            get;
            set;
        }

        public string HttpRequestMethodName
        {
            get;
            set;
        }

        public List<Header> Headers
        {
            get;
            set;
        }

        public void Validate()
        {

        }
    }
}