namespace Web.Helper.Report
{
    using Common;
    using Entity.User;
    using Entity.ValueType;
    using Extension;
    using Microsoft.AspNetCore.Mvc;
    using Model.Chart.Combo;
    using Model.Report;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Constant;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ReportByUserHelper : IHelper
    {
        private BaseController controller;
        private readonly string _userId;
        private readonly User _user;
        private string departmentId;
        private string groupId;
        private DateTime _startDate;
        private DateTime _endDate;

        private readonly UserService userService;
        private readonly DepartmentService departmentService;
        private readonly ProjectService projectService;
        private readonly TimeSheetService timeSheetService;

        public ReportByUserHelper(BaseController controller, string departmentId, string groupId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(_userId))
            {
                _userId = controller.GetUserId();
            }

            userService = controller.GetService<UserService>();
            departmentService = controller.GetService<DepartmentService>();
            timeSheetService = controller.GetService<TimeSheetService>();
            projectService = controller.GetService<ProjectService>();

            _userId = controller.GetUserId();
            _user = userService.Get(_userId);

            this.departmentId = departmentId;
            this.groupId = groupId;

            //如果没有指定日期，就查本周的数据
            if (startDate.IsEmpty())
            {
                this._startDate = DateTimeUtil.GetCurrentMonday();
            }
            else
            {
                this._startDate = startDate;
            }

            if (endDate.IsEmpty())
            {
                this._endDate = DateTimeUtil.GetCurrentMonday().AddDays(6);
            }
            else
            {
                this._endDate = endDate;
            }

            this.controller = controller;
        }

        public IActionResult Build()
        {
            controller.ViewData["SearchDateRange"] = new DateRange(_startDate, _endDate);

            if (!string.IsNullOrEmpty(groupId))
            {
                return BuildGroup(departmentService.GetUserGroupById(groupId));
            }

            if (!string.IsNullOrEmpty(departmentId) && _user.UserType != UserType.Leader)
            {
                return BuildDepartment(departmentService.Get(departmentId));
            }

            switch (_user.UserType)
            {
                case UserType.Admin:
                    return BuildDepartments(departmentService.Get());
                case UserType.Manager:
                    return BuildManager();
                case UserType.Leader:
                    return BuildLeader();
                default:
                    return BuildUser();
            }
        }

        private IActionResult BuildDepartments(List<Department> departments)
        {
            controller.ViewData["DepartmentPairs"] = departments.Select(o => new KeyValuePair<string, string>(o.Id, o.Id)).ToList();
            return controller.View("ReportByUser/Departments");
        }

        private IActionResult BuildDepartment(Department department)
        {
            var userIds = department.UserGroups.SelectMany(o => o.Value.UserIds).ToList();
            userIds = userIds.Where(o => userService.Get(o).AccountType == AccountType.Public).ToList();
            var userGroups = department.UserGroups.Values.ToList();

            controller.ViewData["Model"] = BuildComboModel(userIds);
            controller.ViewData["Pairs"] = userService.GetByIds(userIds).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["GroupPairs"] = userGroups.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["DepartmentId"] = department.Id;

            return controller.View("ReportByUser/Department");
        }

        private IActionResult BuildManager()
        {
            var departments = departmentService.GetDepartmentsByUserId(_user.Id);

            //ToDo:错误处理
            if (departments.IsEmpty())
            {
                return null;
            }
            else
            {
                if(departments.Count > 1)
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
            var groups = departmentService.GetUserGroupsByOwnerId(_userId);

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
            userIds = userIds.Where(o => userService.Get(o).AccountType == AccountType.Public).ToList();

            controller.ViewData["Model"] = BuildComboModel(userIds);
            controller.ViewData["Pairs"] = userService.GetByIds(userIds).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["GroupId"] = group.Id;

            return controller.View("ReportByUser/Group");
        }

        private IActionResult BuildSuperGroup(List<UserGroup> userGroups)
        {
            var department = departmentService.GetDepartmentsByUserId(_user.Id).FirstOrDefault();

            var userIds = userGroups.SelectMany(o => o.UserIds).ToList();
            userIds = userIds.Where(o => userService.Get(o).AccountType == AccountType.Public).ToList();

            controller.ViewData["Model"] = BuildComboModel(userIds);
            controller.ViewData["Pairs"] = userService.GetByIds(userIds).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["GroupPairs"] = userGroups.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["DepartmentId"] = department.Id;

            return controller.View("ReportByUser/Department");
        }

        private IActionResult BuildGroups(List<UserGroup> groups)
        {
            //ToDo:添加组别

            var userIds = groups.SelectMany(o => o.UserIds).ToList();
            userIds = userIds.Where(o => userService.Get(o).AccountType == AccountType.Public).ToList();

            controller.ViewData["Model"] = BuildComboModel(userIds);
            controller.ViewData["Pairs"] = userService.GetByIds(userIds).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            controller.ViewData["GroupId"] = groups[0].Id;

            return controller.View("ReportByUser/Group");
        }

        private IActionResult BuildUser()
        {
            return controller.View();
        }

        private ComboModel BuildComboModel(List<string> userIds)
        {
            var comboItems = new List<ComboItem>();

            foreach (var item in userIds)
            {
                comboItems.Add(BuildComboItem(item));
            }

            return new ComboModel(comboItems);
        }

        private ComboItem BuildComboItem(string userId)
        {
            var user = userService.Get(userId);

            //获取项目的时间，除掉公共项目
            var hours = timeSheetService.GetUserHoursByProjectId(userId, _startDate, _endDate).Where(o => o.Value > 0).ToDictionary(pair => pair.Key, pair => pair.Value);
            hours = hours.Where(o => !projectService.Get(o.Key).IsPublic).ToDictionary(pair => pair.Key, pair => pair.Value);

            var planProjectIds = projectService.GetProjectsByUserId(userId, _startDate, _endDate).Select(o => o.Id).ToList();

            foreach (var projectId in planProjectIds.Except(hours.Keys).ToList())
            {
                hours.Add(projectId, 0);
            }

            var projects = projectService.GetByIds(hours.Keys);

            var planHours = projects.Select(o => o.GetPlanHour(userId, _startDate, _endDate)).ToList();

            projects.AddRange(projectService.GetProjectsByUserId(userId, _startDate, _endDate).Where(o => o.Name.StartsWith(Constant.REWARD_PROJECT_PREFIX, StringComparison.CurrentCulture) && Math.Abs(o.GetTotalActualHour()) < float.Epsilon));
            projects = projects.Distinct().ToList();
            var contributions = projects.Select(o => o.GetContribution(userId, _startDate, _endDate)).ToList();

            var items = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("Plan", Math.Round(planHours.Sum(), 2)),
                new KeyValuePair<string, double>("Actual", Math.Round(hours.Sum(o => o.Value), 2)),
                new KeyValuePair<string, double>("Contribution", Math.Round(contributions.Sum(), 2))
            };
            return new ComboItem(userId, user.Name, user.UserType.ToString(), items);
        }

        private List<ReportByUserModel> BuildReportByUserModels(DateTime startDate, DateTime endDate)
        {
            var result = new List<ReportByUserModel>();

            var userIds = departmentService.GetSubordinatesByUserId(_userId);

            if (_user.UserType == UserType.Admin)
            {
                userIds.Remove(_userId);
            }

            foreach (var item in userIds)
            {
                var user = userService.Get(item);
                var actualHours = timeSheetService.GetUserHoursByProjectId(item, startDate, endDate);
                actualHours = actualHours.Where(o => !projectService.Get(o.Key).IsPublic).ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 2));
                var planProjectIds = projectService.GetProjectsByUserId(item, startDate, endDate).Select(o => o.Id);

                foreach (var projectId in planProjectIds.Except(actualHours.Keys).ToList())
                {
                    actualHours.Add(projectId, 0);
                }

                var projects = projectService.GetByIds(actualHours.Keys);
                var planHours = projects.ToDictionary(o => o.Id, o => Math.Round(o.GetPlanHour(item, startDate, endDate), 2));

                result.Add(new ReportByUserModel(user.Id, user.Name, planHours.Sum(o => o.Value), actualHours.Sum(o => o.Value),
                    projects.Count(o => o.PlanDateRange.EndDate < (o.ActualDateRange == null || o.ActualDateRange.EndDate.IsEmpty() ? DateTime.Today : o.ActualDateRange.EndDate)),
                    projects.Count(o => o.GetPlanHour(item, startDate, endDate) < (actualHours.ContainsKey(o.Id) ? actualHours[o.Id] : 0)),
                    projects.Count()));
            }

            return result;
        }
    }
}
