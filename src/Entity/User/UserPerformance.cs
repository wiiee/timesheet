namespace Entity.User
{
    using System.Collections.Generic;
    using Entity.ValueType;

    public class UserPerformance : BaseEntity
    {
        //Id为GroupId

        public List<PerformanceItem> Items;

        public UserPerformance(string groupId){
            this.Id = groupId;
            this.Items = new List<PerformanceItem>();
        }
    }
}
