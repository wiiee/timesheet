namespace Service.User
{
    using System.Linq;
    using Entity.User;
    using Entity.ValueType;
    using Platform.Context;

    public class UserPerformanceService: BaseService<UserPerformance>
    {
        public UserPerformanceService(IContextRepository contextRepository) : base(contextRepository) { }

        public void Update(string userId, PerformanceItem item){
            var entity = Get(userId);

            if(entity == null){
                entity = new UserPerformance(userId);
            }

            var index = entity.Items.FindIndex(o => o.Id == item.Id);

            index = index == -1 ? entity.Items.Count : index;

            var levels = ServiceFactory.Instance.GetService<UserService>().GetByIds(item.Values.Keys.ToList()).ToDictionary(o => o.Id, o => o.Level);

            item.Calculate(levels);

            entity.Items[index] = item;

            Update(entity);
        }
    }
}
