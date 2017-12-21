namespace Service.User
{
    using System.Linq;
    using Entity.User;
    using Entity.ValueType;
    using Platform.Context;
    using Platform.Extension;

    public class UserPerformanceService: BaseService<UserPerformance>
    {
        public UserPerformanceService(IContextRepository contextRepository) : base(contextRepository) { }

        public void Update(string userGroupId, PerformanceItem item){
            var entity = Get(userGroupId);

            if(entity == null){
                entity = new UserPerformance(userGroupId);
            }

            var index = entity.Items.FindIndex(o => o.Id == item.Id);

            if(index == -1)
            {
                index = entity.Items.IsEmpty() ? 1 : entity.Items.Last().Id + 1;
            }

            item.Calculate();

            entity.Items[index] = item;

            Update(entity);
        }
    }
}
