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

        public void AddItem(string userGroupId, PerformanceItem item){
            var entity = Get(userGroupId);

            if(entity == null){
                entity = new UserPerformance(userGroupId);
                Create(entity);
            }

            if (entity.Items.IsEmpty())
            {
                item.Id = 0;
            }
            else
            {
                item.Id = ++entity.Items.Last().Id;
            }

            entity.Items.Add(item);

            Update(entity);
        }

        public void RemoveItem(string userGroupId, int itemId)
        {
            var entity = Get(userGroupId);

            if (entity != null)
            {
                var index = entity.Items.FindIndex(o => o.Id == itemId);
                entity.Items.RemoveAt(index);

                Update(entity);
            }
        }
    }
}
