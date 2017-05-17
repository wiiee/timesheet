namespace DataAccess.Connector.Base
{
    using System;

    public class ConnectorAdapter : IConnectorAdapter
    {
        private void Before()
        {

        }

        private void After()
        {

        }

        bool Supports()
        {
            return true;
        }

        public Rs Invoke<Rq, Rs>(Rq rq, IConnector<Rq, Rs> connector)
        {
            if (!Supports())
                throw new Exception("not supported");

            Before();

            Rs rs = connector.Invoke(rq);

            After();

            return rs;
        }
    }
}