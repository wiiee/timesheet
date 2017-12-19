namespace Entity.User
{
    using System.Collections.Generic;
    using Entity.ValueType;

    public class UserPerformance : BaseEntity
    {
        //Id为UserId

        public List<PerformanceItem> Items;

        public UserPerformance(string userId){
            this.Id = userId;
            this.Items = new List<PerformanceItem>();
        }
    }
}
