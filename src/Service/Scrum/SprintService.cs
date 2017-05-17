namespace Service.Scrum
{
    using Entity.Project.Scrum;
    using Platform.Context;

    public class SprintService : BaseService<Sprint>
    {
        public SprintService(IContextRepository contextRepository) : base(contextRepository) { }
    }
}
