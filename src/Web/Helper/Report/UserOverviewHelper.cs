namespace Web.Helper.Report
{
    using Common;
    using Entity.Project;
    using Entity.User;
    using Entity.ValueType;
    using Extension;
    using Microsoft.AspNetCore.Mvc;
    using Model.Chart.Line;
    using Model.Chart.Timeline;
    using Model.Report;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UserOverviewHelper : IHelper
    {
        private BaseController controller;
        private string userId;
        private User user;
        private DateTime startDate;
        private DateTime endDate;

        private UserService userService;
        private DepartmentService departmentService;
        private ProjectService projectService;
        private TimeSheetService timeSheetService;

        public UserOverviewHelper(BaseController controller, string userId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = controller.GetUserId();
            }

            userService = controller.GetService<UserService>();
            departmentService = controller.GetService<DepartmentService>();
            timeSheetService = controller.GetService<TimeSheetService>();
            projectService = controller.GetService<ProjectService>();

            user = userService.Get(userId);

            //如果没有指定日期，就查本周的数据
            if (startDate.IsEmpty())
            {
                this.startDate = DateTimeUtil.GetCurrentMonday();
            }
            else
            {
                this.startDate = startDate;
            }

            if (endDate.IsEmpty())
            {
                this.endDate = DateTimeUtil.GetCurrentMonday().AddDays(6);
            }
            else
            {
                this.endDate = endDate;
            }

            this.controller = controller;
            this.userId = userId;
        }

        public IActionResult Build()
        {
            controller.ViewData["SearchDateRange"] = new DateRange(startDate, endDate);
            controller.ViewData["User"] = BuildUserOverviewModel();
            BuildTimelineAndPair();

            return controller.View("UserOverview/Index");
        }

        private UserOverviewModel BuildUserOverviewModel()
        {
            var hours = timeSheetService.GetUserHoursByProjectId(userId, startDate, endDate);
            var planProjectIds = projectService.GetProjectsByUserId(userId, startDate, endDate).Select(o => o.Id);

            foreach (var projectId in planProjectIds.Except(hours.Keys).ToList())
            {
                hours.Add(projectId, 0);
            }

            var projects = projectService.GetByIds(hours.Keys);
            var planHours = projects.ToDictionary(o => o.Id, o => o.GetPlanHour(userId, startDate, endDate));

            var publicProjectHours = new Dictionary<string, double>();
            var projectHours = new Dictionary<string, double>();
            var crProjectHours = new Dictionary<string, double>();

            foreach (var entry in hours)
            {
                var project = projectService.Get(entry.Key);

                if(project != null)
                {
                    if (project.IsPublic)
                    {
                        publicProjectHours.Add(entry.Key, entry.Value);
                    }
                    else if (project.IsCr)
                    {
                        crProjectHours.Add(entry.Key, entry.Value);
                    }
                    else
                    {
                        projectHours.Add(entry.Key, entry.Value);
                    }
                }
            }

            return new UserOverviewModel(userId, user.Name, BuildUserOverviewPart(publicProjectHours, planHours),
                BuildUserOverviewPart(projectHours, planHours), BuildUserOverviewPart(crProjectHours, planHours),
                BuildUserPlanActualLine(userId, startDate, endDate));
        }

        private void BuildTimelineAndPair()
        {
            var actualHours = timeSheetService.GetUserHoursByProjectId(userId, startDate, endDate);
            var planProjectIds = projectService.GetProjectsByUserId(userId, startDate, endDate).Select(o => o.Id);

            foreach (var projectId in planProjectIds.Except(actualHours.Keys).ToList())
            {
                actualHours.Add(projectId, 0);
            }

            controller.ViewData["Pairs"] = projectService.GetByIds(actualHours.Keys).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

            actualHours = actualHours.Where(pair => !projectService.Get(pair.Key).IsPublic).ToDictionary(pair => pair.Key, pair => pair.Value);

            var projects = projectService.GetByIds(actualHours.Keys);
            var planHours = projects.ToDictionary(o => o.Id, o => o.GetPlanHour(userId, startDate, endDate));

            var timelineItems = new List<TimelineItem>();

            foreach (var item in projects)
            {
                timelineItems.Add(BuildTimelineItem(item, planHours, actualHours));
            }

            controller.ViewData["TimelineModel"] = new TimelineModel(timelineItems);
        }

        private TimelineItem BuildTimelineItem(Project project, Dictionary<string, double> planHours, Dictionary<string, double> actualHours)
        {
            var parts = new List<TimelinePart>();

            var planHour = planHours.ContainsKey(project.Id) ? planHours[project.Id] : 0;
            var actualHour = actualHours.ContainsKey(project.Id) ? actualHours[project.Id] : 0;

            var name = project.Name + "|" + Math.Round(planHour, 2) + "/" + Math.Round(actualHour, 2);
            parts.Add(new TimelinePart(name, project.PlanDateRange.StartDate, project.PlanDateRange.EndDate));

            return new TimelineItem(project.Id, project.Name, project.SerialNumber, parts);
        }

        private Dictionary<string, KeyValuePair<double, double>> BuildUserOverviewPart(Dictionary<string, double> actualHours,
            Dictionary<string, double> planHours)
        {
            Dictionary<string, KeyValuePair<double, double>> result = new Dictionary<string, KeyValuePair<double, double>>();

            foreach (var entry in actualHours)
            {
                result.Add(entry.Key,
                    new KeyValuePair<double, double>(planHours.ContainsKey(entry.Key) ? Math.Round(planHours[entry.Key], 2) : 0, Math.Round(entry.Value, 2)));
            }

            return result;
        }

        private LineModel BuildUserPlanActualLine(string userId, DateTime startDate, DateTime endDate)
        {
            var planHours = projectService.GetPlanHoursByDate(userId, startDate, endDate).ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 2));
            var actualHours = timeSheetService.GetUserHoursWithoutPublicByDate(userId, startDate, endDate).ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 2));

            var names = planHours.Keys.Select(o => o.ToString("MM-dd")).ToList();

            List<LineItem> items = new List<LineItem>();
            items.Add(new LineItem("Plan", planHours.Values.ToList()));
            items.Add(new LineItem("Actual", actualHours.Values.ToList()));

            return new LineModel(names, items);
        }
    }
}
