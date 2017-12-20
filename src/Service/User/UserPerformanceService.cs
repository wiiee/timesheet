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

        public void Update(string userId, PerformanceItem item){
            var entity = Get(userId);

            if(entity == null){
                entity = new UserPerformance(userId);
            }

            var index = entity.Items.FindIndex(o => o.Id == item.Id);

            if(index == -1)
            {
                index = entity.Items.IsEmpty() ? 1 : entity.Items.Last().Id + 1;
            }

            var levels = ServiceFactory.Instance.GetService<UserService>().GetByIds(item.Values.Keys.ToList()).ToDictionary(o => o.Id, o => o.Level);

            item.Calculate(levels);

            entity.Items[index] = item;

            Update(entity);
        }
    }
}
