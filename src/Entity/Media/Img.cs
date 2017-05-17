namespace Entity.Media
{
    //图片
    public class Img : BaseEntity
    {
        public byte[] Content { get; set; }

        public int Length { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public Img(int length, string name, byte[] content, string contentType)
        {
            this.Length = length;
            this.Name = name;
            this.Content = content;
            this.ContentType = contentType;
        }

        public Img() { }
    }
}
