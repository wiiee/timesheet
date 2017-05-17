namespace Service.Project
{
    using Entity.Project;
    using Platform.Context;

    public class TaskTemplateService : BaseService<TaskTemplate>
    {
        public TaskTemplateService(IContextRepository contextRepository) : base(contextRepository) { }
    }
}
