namespace DataAccess.Connector.Base
{
    public interface IConnectorAdapter
    {
        Rs Invoke<Rq, Rs>(Rq rq, IConnector<Rq, Rs> service);
    }
}