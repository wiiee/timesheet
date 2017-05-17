namespace Web.Helper
{
    using Common;
    using Extension;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service.User;
    using System;
    using System.Collections.Generic;

    public class TimeSheetHelper : IHelper
    {
        public static DateTime MIN_DATE = new DateTime(2016, 2, 1);

        private BaseController controller;

        public TimeSheetHelper(BaseController controller)
        {
            this.controller = controller;
        }

        public IActionResult Build() {
            return null;
        }

        public List<TimeSheetOverviewModel> BuildTimeSheetOverViewModel(string userId)
        {
            var userTimeSheetStatusService = this.controller.GetService<UserTimeSheetStatusService>();
            var userService = this.controller.GetService<UserService>();

            var user = userService.Get(userId);

            var startMonday = MIN_DATE;

            if (user.Created != null && user.Created > MIN_DATE)
            {
                startMonday = user.Created.GetMonday().Date;
            }

            var monday = DateTimeUtil.GetCurrentMonday();
            var weeks = new List<TimeSheetOverviewModel>();
            var statuses = userTimeSheetStatusService.GetUserTimeSheets(userId, startMonday, monday);

            var userType = this.controller.GetUserType();

            foreach (var entry in statuses)
            {
                var dateTime = DateTime.Parse(entry.Key);
                var today = DateTime.Today;

                bool isReturn = false;
                bool isShow = true;

                if(entry.Value.Key == Status.Done)
                {
                    if ((userType == UserType.User && (today.GetMonday() - dateTime).Days <= 7) || userType != UserType.User)
                    {
                        isReturn = true;
                    }
                }

                weeks.Add(new TimeSheetOverviewModel(dateTime, dateTime.AddDays(6), entry.Value.Key.ToString(), BuildText(dateTime), entry.Value.Value, isReturn, isShow));
            }

            return weeks;
        }

        public Dictionary<string, List<TimeSheetOverviewModel>> BuildUserTimeSheetOverViewModels(IEnumerable<string> userIds)
        {
            Dictionary<string, List<TimeSheetOverviewModel>> result = new Dictionary<string, List<TimeSheetOverviewModel>>();

            foreach (var item in userIds)
            {
                result.Add(item, BuildTimeSheetOverViewModel(item));
            }

            return result;
        }

        private string BuildText(DateTime firstDay)
        {
            return string.Format("{0} ~ {1}", firstDay.GetTimeSheetId(), firstDay.AddDays(6).GetTimeSheetId());
        }
    }
}
