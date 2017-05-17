namespace Entity.User
{
    using Platform.Enum;
    using System.Collections.Generic;

    public class UserTimeSheetStatus : BaseEntity
    {
        //Id为UserId

        //key为每周一的字符串
        public Dictionary<string, KeyValuePair<Status, double>> Weeks { get; set; }

        public UserTimeSheetStatus(string id, Dictionary<string, KeyValuePair<Status, double>> weeks)
        {
            this.Id = id;

            if(weeks == null)
            {
                weeks = new Dictionary<string, KeyValuePair<Status, double>>();
            }

            this.Weeks = weeks;
        }

        public Status GetStatus(string monday)
        {
            if (Weeks.ContainsKey(monday))
            {
                return Weeks[monday].Key;
            }

            return Status.Pending;
        }
    }
}
