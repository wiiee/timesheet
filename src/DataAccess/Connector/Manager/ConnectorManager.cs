namespace DataAccess.Connector.Base
{
    public class ConnectorManager
    {
        private static IConnectorFactory connectorFactory = null;
        private static IConnectorAdapter connectorAdapter = null;
        private static ConnectorManager connectorManager = null;

        public static IConnectorAdapter CurrentConnectorAdapter
        {
            get
            {
                return connectorAdapter;
            }
            set
            {
                connectorAdapter = value;
            }
        }

        public static IConnectorFactory CurrentConnectorFactory
        {
            get
            {
                return connectorFactory;
            }
            set
            {
                connectorFactory = value;
            }
        }

        public static ConnectorManager Current
        {
            get
            {
                if (connectorManager == null)
                {
                    if (connectorAdapter == null)
                    {
                        connectorAdapter = new ConnectorAdapter();
                    }

                    if (connectorFactory == null)
                    {
                        connectorFactory = new ConnectorFactory();
                    }

                    connectorManager = new ConnectorManager(connectorFactory, connectorAdapter);
                }

                return connectorManager;
            }
        }

        private ConnectorManager(IConnectorFactory connectorFactory, IConnectorAdapter connectorAdapter)
        {
            ConnectorManager.connectorFactory = connectorFactory;
            ConnectorManager.connectorAdapter = connectorAdapter;
        }

        public Rs Invoke<Rq, Rs>(Rq rq)
        {
            IConnector<Rq, Rs> connector = connectorFactory.GetConnector<Rq, Rs>();

            Rs rs = connectorAdapter.Invoke<Rq, Rs>(rq, connector);

            return rs;
        }
    }
}
