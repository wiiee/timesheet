namespace Service.User
{
    using Entity.User;
    using Platform.Context;
    using Platform.Enum;
    using Platform.Extension;
    using System;
    using System.Collections.Generic;

    public class UserTimeSheetStatusService : BaseService<UserTimeSheetStatus>
    {
        public UserTimeSheetStatusService(IContextRepository contextRepository) : base(contextRepository) { }

        public void UpdateUserTimeSheet(string userId, string monday, Status status, double hour)
        {
            var entity = Get(userId);

            if (entity == null)
            {
                Dictionary<string, KeyValuePair<Status, double>> weeks = new Dictionary<string, KeyValuePair<Status, double>>();
                weeks.Add(monday, new KeyValuePair<Status, double>(status, hour == -1 ? 0 : hour));
                Create(new UserTimeSheetStatus(userId, weeks));
            }
            else
            {
                if (entity.Weeks == null)
                {
                    entity.Weeks = new Dictionary<string, KeyValuePair<Status, double>>();
                }

                if (entity.Weeks.ContainsKey(monday))
                {
                    entity.Weeks[monday] = new KeyValuePair<Status, double>(status, hour);
                }
                else
                {
                    entity.Weeks.Add(monday, new KeyValuePair<Status, double>(status, hour));
                }

                Update(entity);
            }
        }

        public void ReturnTimeSheet(string monday, string userId)
        {
            var entity = Get(userId);

            if (entity != null)
            {
                if (!entity.Weeks.IsEmpty() && entity.Weeks.ContainsKey(monday))
                {
                    var hour = entity.Weeks[monday].Value;
                    entity.Weeks[monday] = new KeyValuePair<Status, double>(hour > 0 ? Status.Ongoing : Status.Pending, hour);

                    Update(entity);
                }
            }
        }

        //得到指定日期的TimeSheet，包括状态和时间
        public Dictionary<string, KeyValuePair<Status, double>> GetUserTimeSheets(string userId, DateTime startDate, DateTime endDate)
        {
            Dictionary<string, KeyValuePair<Status, double>> result = new Dictionary<string, KeyValuePair<Status, double>>();
            var userTimeSheet = Get(userId);

            var startMonday = startDate.GetMonday();
            var endMonday = endDate.GetMonday();

            while (startMonday <= endMonday)
            {
                var timeSheetId = startMonday.GetTimeSheetId();
                if (userTimeSheet != null &&
                    userTimeSheet.Weeks != null &&
                    userTimeSheet.Weeks.ContainsKey(timeSheetId))
                {
                    result.Add(timeSheetId, userTimeSheet.Weeks[timeSheetId]);
                }
                else
                {
                    result.Add(timeSheetId, new KeyValuePair<Status, double>(Status.Pending, 0));
                }

                startMonday = startMonday.AddDays(7);
            }

            return result;
        }
    }
}
