namespace Web.Helper.Report
{
    using Common;
    using Microsoft.AspNetCore.Mvc;
    using Platform.Enum;
    using Service.User;
    using System;
    using Entity.User;
    using Entity.ValueType;
    using Model.Report;
    using Service.Project;
    using System.Collections.Generic;
    using Extension;
    using Model.Chart.Timeline;
    using Entity.Project;
    using Platform.Extension;
    using System.Linq;
    using Platform.Util;
    using Model.Chart.Bubble;

    public class ReportByProjectHelper : IHelper
    {
        private BaseController controller;
        private string userId;
        private User user;
        private string departmentId;
        private string groupId;
        private DateTime startDate;
        private DateTime endDate;

        private UserService userService;
        private DepartmentService departmentService;
        private ProjectService projectService;
        private TimeSheetService timeSheetService;

        public ReportByProjectHelper(BaseController controller, string departmentId, string groupId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = controller.GetUserId();
            }

            userService = controller.GetService<UserService>();
            departmentService = controller.GetService<DepartmentService>();
            timeSheetService = controller.GetService<TimeSheetService>();
            projectService = controller.GetService<ProjectService>();

            this.userId = controller.GetUserId();
            user = userService.Get(userId);

            this.departmentId = departmentId;
            this.groupId = groupId;

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
        }

        public IActionResult Build()
        {
            controller.ViewData["SearchDateRange"] = new DateRange(startDate, endDate);

            if (!string.IsNullOrEmpty(groupId))
            {
                return BuildGroup(departmentService.GetUserGroupById(groupId));
            }

            if (!string.IsNullOrEmpty(departmentId) && user.UserType != UserType.Leader)
            {
                return BuildDepartment(departmentService.Get(departmentId));
            }

            if (user.UserType == UserType.Admin)
            {
                return BuildDepartments(departmentService.Get());
            }
            else if (user.UserType == UserType.Manager)
            {
                return BuildManager();
            }
            else if (user.UserType == UserType.Leader)
            {
                return BuildLeader();
            }
            else
            {
                return BuildUser();
            }
        }

        private IActionResult BuildDepartments(List<Department> departments)
        {
            controller.ViewData["DepartmentPairs"] = departments.Select(o => new KeyValuePair<string, string>(o.Id, o.Id)).ToList();
            return controller.View("ReportByProject/Departments");
        }

        private IActionResult BuildDepartment(Department department)
        {
            var userIds = department.UserGroups.SelectMany(o => o.Value.UserIds).ToList();
            var projects = projectService.GetProjectsByUserIds(userIds, startDate, endDate);
            var userGroups = department.UserGroups.Values.ToList();

            controller.ViewData["Name"] = department.Id;
            controller.ViewData["Model"] = BuildTimelineModel(projects);
            controller.ViewData["BubbleModel"] = BuildBubbleModel(projects);
            controller.ViewData["Pairs"] = projects.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

            controller.ViewData["ExpiredProjects"] = projects.Where(o => !o.PostponeReasons.IsEmpty()).ToList();
            controller.ViewData["OvertimeProjects"] = projects.Where(o => o.GetTotalPlanHour() < o.GetTotalActualHour()).ToList();
            controller.ViewData["GroupPairs"] = userGroups.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["DepartmentId"] = department.Id;

            return controller.View("ReportByProject/Department");
        }

        private IActionResult BuildManager()
        {
            var departments = departmentService.GetDepartmentsByUserId(user.Id);

            //ToDo:错误处理
            if (departments.IsEmpty())
            {
                return null;
            }
            else
            {
                if (departments.Count > 1)
                {
                    return BuildDepartments(departments);
                }
                else
                {
                    return BuildDepartment(departments.FirstOrDefault());
                }
            }
        }

        private IActionResult BuildLeader()
        {
            var groups = departmentService.GetUserGroupsByOwnerId(userId);

            //ToDo:错误处理
            if (groups.IsEmpty())
            {
                return null;
            }
            else
            {
                if (groups.Count > 1)
                {
                    return BuildSuperGroup(groups);
                    //return BuildGroups(groups);
                }
                else
                {
                    return BuildGroup(groups.FirstOrDefault());
                }
            }
        }

        private IActionResult BuildGroup(UserGroup group)
        {
            var userIds = group.UserIds.ToList();
            var projects = projectService.GetProjectsByUserIds(userIds, startDate, endDate);

            var calProjects = projects.Where(o => o.Id != "G10432_TimeSheet").ToList();

            controller.ViewData["TestHour"] = calProjects.Sum(o => o.GetActualTestHour());
            controller.ViewData["DevHour"] = calProjects.Sum(o => o.GetActualDevHour());
            controller.ViewData["Projects"] = calProjects;

            controller.ViewData["Name"] = group.Name;
            controller.ViewData["Model"] = BuildTimelineModel(projects);
            controller.ViewData["BubbleModel"] = BuildBubbleModel(projects);
            controller.ViewData["Pairs"] = projects.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

            controller.ViewData["ExpiredProjects"] = projects.Where(o => !o.PostponeReasons.IsEmpty()).ToList();
            controller.ViewData["OvertimeProjects"] = projects.Where(o => o.GetTotalPlanHour() < o.GetTotalActualHour()).ToList();
            controller.ViewData["GroupId"] = group.Id;

            return controller.View("ReportByProject/Group");
        }

        private IActionResult BuildSuperGroup(List<UserGroup> groups)
        {
            var department = departmentService.GetDepartmentsByUserId(user.Id).FirstOrDefault();
            var userIds = groups.SelectMany(o => o.UserIds).ToList();
            var projects = projectService.GetProjectsByUserIds(userIds, startDate, endDate);

            controller.ViewData["Name"] = string.Join(",", groups.Select(o => o.Name).ToList());
            controller.ViewData["Model"] = BuildTimelineModel(projects);
            controller.ViewData["BubbleModel"] = BuildBubbleModel(projects);
            controller.ViewData["Pairs"] = projects.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

            controller.ViewData["ExpiredProjects"] = projects.Where(o => !o.PostponeReasons.IsEmpty()).ToList();
            controller.ViewData["OvertimeProjects"] = projects.Where(o => o.GetTotalPlanHour() < o.GetTotalActualHour()).ToList();
            controller.ViewData["GroupPairs"] = groups.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["DepartmentId"] = department.Id;

            return controller.View("ReportByProject/Department");
        }

        private IActionResult BuildGroups(List<UserGroup> groups)
        {
            //ToDo:添加组别

            var userIds = groups.SelectMany(o => o.UserIds).ToList();
            var projects = projectService.GetProjectsByUserIds(userIds, startDate, endDate);

            controller.ViewData["Name"] = string.Join(",", groups.Select(o => o.Name).ToList());
            controller.ViewData["Model"] = BuildTimelineModel(projects);
            controller.ViewData["BubbleModel"] = BuildBubbleModel(projects);
            controller.ViewData["Pairs"] = projects.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

            controller.ViewData["ExpiredProjects"] = projects.Where(o => !o.PostponeReasons.IsEmpty()).ToList();
            controller.ViewData["OvertimeProjects"] = projects.Where(o => o.GetTotalPlanHour() < o.GetTotalActualHour()).ToList();
            controller.ViewData["GroupId"] = groups[0].Id;

            return controller.View("ReportByProject/Group");
        }

        private IActionResult BuildUser()
        {
            return controller.View();
        }

        private BubbleModel BuildBubbleModel(List<Project> projects)
        {
            var bubbleItems = new List<BubbleItem>();

            foreach (var item in projects)
            {
                bubbleItems.Add(BuildBubbleItem(item));
            }

            return new BubbleModel(bubbleItems);
        }

        private BubbleItem BuildBubbleItem(Project project)
        {
            double percentage = 100;

            if (project.GetTotalPlanHour() > 0)
            {
                percentage = Math.Round(project.GetTotalActualHour() * 100 / project.GetTotalPlanHour(), 2);
            }

            var factor = project.GetRiskFactor();

            var riskLevel = RiskLevel.Normal;

            if(factor > 200)
            {
                riskLevel = RiskLevel.High;
            }
            else if(factor > 150)
            {
                riskLevel = RiskLevel.Medium;
            }
            else if (factor > 100)
            {
                riskLevel = RiskLevel.Low;
            }

            return new BubbleItem(project.Id, project.Name, percentage, factor, riskLevel.ToString(), project.GetTotalPlanHour());
        }

        private TimelineModel BuildTimelineModel(List<Project> projects)
        {
            var timelineItems = new List<TimelineItem>();
            
            foreach(var item in projects)
            {
                timelineItems.Add(BuildTimelineItem(item));
            }

            return new TimelineModel(timelineItems);
        }

        private TimelineItem BuildTimelineItem(Project project)
        {
            var parts = new List<TimelinePart>();
            var name = string.Join(",", userService.GetByIds(project.UserIds).Select(o => o.Name).ToList()) + "|" + project.GetTotalPlanHour() + "/" + project.GetTotalActualHour();

            if (project.PostponeReasons.IsEmpty())
            {
                parts.Add(new TimelinePart(name, project.PlanDateRange.StartDate, project.PlanDateRange.EndDate));
            }
            else
            {
                parts.Add(new TimelinePart(name, project.PlanDateRange.StartDate, project.PlanEndDate));
                parts.Add(new TimelinePart("Actual", project.PlanEndDate, project.PlanDateRange.EndDate));
            }

            return new TimelineItem(project.Id, project.Name, project.SerialNumber, parts);
        }

        private List<ReportByProjectModel> BuildReportByProjectModels()
        {
            var departmentService = controller.GetService<DepartmentService>();
            var timeSheetService = controller.GetService<TimeSheetService>();

            var userIds = departmentService.GetSubordinatesByUserId(controller.GetUserId());

            var workingProjectIds = timeSheetService.GetWorkingProjectIds(userIds, startDate, endDate);

            return BuildReportByProjectModels(workingProjectIds);
        }

        private List<ReportByProjectModel> BuildReportByProjectModels(List<string> projectIds)
        {
            List<ReportByProjectModel> result = new List<ReportByProjectModel>();

            foreach (var item in projectIds)
            {
                if (!controller.GetService<ProjectService>().Get(item).IsPublic)
                {
                    result.Add(BuildReportByProjectModel(item));
                }
            }

            return result;
        }

        private ReportByProjectModel BuildReportByProjectModel(string projectId)
        {
            var projectService = controller.GetService<ProjectService>();
            var userService = controller.GetService<UserService>();

            var project = projectService.Get(projectId);

            var projectException = ProjectException.Normal.ToString();

            if ((project.Status == Status.Done && project.ActualDateRange.EndDate > project.PlanDateRange.EndDate)
                || (project.Status != Status.Done && DateTime.Today > project.PlanDateRange.EndDate))
            {
                projectException = ProjectException.Expired.ToString();
            }
            else if (project.GetTotalActualHour() > project.GetTotalPlanHour())
            {
                projectException = ProjectException.Overtime.ToString();
            }

            var model = new ReportByProjectModel(projectId, project.Name, project.GetTotalPlanHour(), project.GetPlanDevHour(),
                project.GetPlanTestHour(), project.GetTotalActualHour(), project.GetActualDevHour(), project.GetActualTestHour(),
                project.PlanDateRange, project.ActualDateRange, project.Status.ToString(), projectException);

            return model;
        }
    }
}
