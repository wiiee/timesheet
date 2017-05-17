namespace DataAccess.Database.Base
{
    public class GridFSFile
    {
        public string Id { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] MetaData { get; set; }
    }
}
