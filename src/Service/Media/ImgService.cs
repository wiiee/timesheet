namespace Service.Media
{
    using Entity.Media;
    using Platform.Context;

    public class ImgService : BaseService<Img>
    {
        public ImgService(IContextRepository contextRepository) : base(contextRepository) { }
    }
}
