namespace DataAccess.Connector.Config
{
    using System;
    using System.Collections.Generic;

    public class ServiceRegistry
    {
        public ServiceRegistry()
        {
            this.serviceDescriptorDicByName = new Dictionary<String, ServiceDescriptor>();
            this.serviceDescriptorDicByRqRs = new Dictionary<String, ServiceDescriptor>();
        }

        private Dictionary<String, ServiceDescriptor> serviceDescriptorDicByName;
        private Dictionary<String, ServiceDescriptor> serviceDescriptorDicByRqRs;

        public List<ServiceDescriptor> ServiceDescriptors
        {
            get;
            set;
        }

        public void Init()
        {
            if (ServiceDescriptors != null)
            {
                foreach (ServiceDescriptor desc in ServiceDescriptors)
                {
                    try
                    {
                        desc.Validate();
                    }
                    catch (Exception)
                    {
                        
                    }

                    if (!string.IsNullOrEmpty(desc.Name))
                    {
                        this.serviceDescriptorDicByName.Add(desc.Name, desc);
                    }

                    if (!string.IsNullOrEmpty(desc.RequestType) && !string.IsNullOrEmpty(desc.ResponseType))
                    {
                        String key = MakeDescriptorKey(desc.RequestType, desc.ResponseType);
                        this.serviceDescriptorDicByRqRs.Add(key, desc);
                    }
                }
            }
            else
            {
                ServiceDescriptors = new List<ServiceDescriptor>();
            }
        }

        private String MakeDescriptorKey(string requestType, string responseType)
        {
            return requestType.Trim() + responseType.Trim();
        }

        public ServiceDescriptor GetDescriptor(string name)
        {
            if (this.serviceDescriptorDicByName.ContainsKey(name))
            {
                return this.serviceDescriptorDicByName[name];
            }

            return null;
        }

        public ServiceDescriptor GetDescriptor(String requestType, String responseType)
        {
            string key = MakeDescriptorKey(requestType, responseType);

            if (this.serviceDescriptorDicByRqRs.ContainsKey(key))
            {
                return this.serviceDescriptorDicByRqRs[key];
            }

            return null;
        }
    }
}