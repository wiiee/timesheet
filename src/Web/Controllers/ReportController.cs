namespace Web.Controllers
{
    using Common;
    using Entity.Project;
    using Entity.ValueType;
    using Extension;
    using Helper;
    using Helper.Report;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model.Report;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    [Authorize]
    public class ReportController : BaseController
    {
        // GET: /<controller>/
        [Authorize(Roles = "0,1,3")]
        public IActionResult ReportByUser(string departmentId, string groupId, DateTime startDate, DateTime endDate)
        {
            //if (!string.IsNullOrEmpty(userId)
            //    && !this.GetService<DepartmentService>().IsBoss(this.GetUserId(), userId))
            //{
            //    return new RedirectResult("~/Home/Index?errorMsg=" + WebUtility.UrlEncode("You don't have access to the page previously!"));
            //}

            return new ReportByUserHelper(this, departmentId, groupId, startDate, endDate).Build();
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult ReportByProject(string departmentId, string groupId, DateTime startDate, DateTime endDate)
        {
            //if (!string.IsNullOrEmpty(userId) 
            //    && !this.GetService<DepartmentService>().IsBoss(this.GetUserId(), userId))
            //{
            //    return new RedirectResult("~/Home/Index?errorMsg=" + WebUtility.UrlEncode("You don't have access to the page previously!"));
            //}

            return new ReportByProjectHelper(this, departmentId, groupId, startDate, endDate).Build();
        }

        //id为ProjectId
        public IActionResult ProjectOverview(string projectId, DateTime startDate, DateTime endDate)
        {
            //如果没有指定日期，就查本周的数据
            if (startDate.IsEmpty())
            {
                startDate = DateTimeUtil.GetCurrentMonday();
            }

            if (endDate.IsEmpty())
            {
                endDate = DateTimeUtil.GetCurrentMonday().AddDays(6);
            }

            ViewData["Model"] = BuildProjectOverviewModel(projectId, startDate, endDate);
            ViewData["Project"] = this.GetService<ProjectService>().Get(projectId);
            ViewData["SearchDateRange"] = new DateRange(startDate, endDate);

            return View("ProjectOverview/Index");
        }

        //id为UserId
        public IActionResult UserOverview(string userId, DateTime startDate, DateTime endDate)
        {
            if (!string.IsNullOrEmpty(userId)
                && !this.GetService<DepartmentService>().IsBoss(this.GetUserId(), userId))
            {
                return new RedirectResult("~/Home/Index?errorMsg=" + WebUtility.UrlEncode("You don't have access to the page previously!"));
            }

            return new UserOverviewHelper(this, userId, startDate, endDate).Build();
        }

        //id为UserId
        public IActionResult MonthlyReport(DateTime startDate, DateTime endDate, bool isShowImportant)
        {
            //如果没有指定日期，就查本月的数据
            if (startDate.IsEmpty())
            {
                startDate = DateTimeUtil.GetCurrentMonthStartDay();
            }

            if (endDate.IsEmpty())
            {
                endDate = DateTimeUtil.GetCurrentMonthEndDay();
            }

            ViewData["SearchDateRange"] = new DateRange(startDate, endDate);

            if (this.GetUserType() == UserType.Admin)
            {
                return getMonthlyReportViewResult<HotelMonthlyReportModel>(startDate, endDate, isShowImportant, DepartmentName.SHENZHEN_HOTEL_EN);
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_HOTEL))
            {
                return getMonthlyReportViewResult<HotelMonthlyReportModel>(startDate, endDate, isShowImportant, DepartmentName.SHENZHEN_HOTEL_EN);
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_FLIGHT))
            {
                return getMonthlyReportViewResult<FlightMonthlyReportModel>(startDate, endDate, isShowImportant, DepartmentName.SHENZHEN_FLIGHT_EN);
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_CORP_TRAVEL))
            {
                return getMonthlyReportViewResult<FlightMonthlyReportModel>(startDate, endDate, isShowImportant, DepartmentName.SHENZHEN_CORP_TRAVEL_EN);
            }
            else
            {
                return getMonthlyReportViewResult<HotelMonthlyReportModel>(startDate, endDate, isShowImportant, DepartmentName.SHANGHAI_CORP_TRAVEL_EN);
            }

        }

        private ViewResult getMonthlyReportViewResult<XModel>(DateTime startDate, DateTime endDate, bool isShowImportant, string department) where XModel : MonthlyReportModel
        {
            List<XModel> monthlyReportModels = GetMonthlyProjects<XModel>(startDate, endDate, isShowImportant, department);
            ViewData["ThisMonthDoneProject"] = monthlyReportModels.Where(p => p.IsDone && p.FinishedDate <= endDate).ToList();//完成日期必须落在该月
            ViewData["ThisMonthUnDoneProject"] = monthlyReportModels.Where(p => !p.IsDone).ToList();
            if (DepartmentName.SHENZHEN_FLIGHT_EN == department || DepartmentName.SHENZHEN_CORP_TRAVEL_EN == department)
            {
                ViewData["ThisMonthHighLevelProject"] = monthlyReportModels.Where(p => (p.Level == 2)).ToList();
            }
            else
            {
                ViewData["ThisMonthHighLevelProject"] = monthlyReportModels.Where(p => (p.Level == 1 || p.Level == 2)).ToList();
            }

            monthlyReportModels = GetMonthlyProjects<XModel>(DateTimeUtil.GetNextMonthStartDay(endDate), DateTimeUtil.GetNextMonthEndDay(endDate), isShowImportant, department);
            ViewData["NextMonthDoneProject"] = monthlyReportModels.Where(p => p.IsDone && p.FinishedDate <= DateTimeUtil.GetNextMonthEndDay(endDate)).ToList();
            ViewData["NextMonthUnDoneProject"] = monthlyReportModels.Where(p => !p.IsDone).ToList();
            ViewData["DepartmentName"] = this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id;

            return View("MonthlyReport/" + department);
        }

        private List<T> GetMonthlyProjects<T>(DateTime startDate, DateTime endDate, bool isShowImportant, string department) where T : MonthlyReportModel
        {
            var userId = this.GetUserId();
            var departmentService = this.GetService<DepartmentService>();
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();
            var userService = this.GetService<UserService>();
            var userGroups = departmentService.GetUserGroupsByUserId(userId);
            userGroups.Sort(UserGroupSort);
            List<T> monthlyReportModels = new List<T>();
            foreach (var userGroup in userGroups)
            {
                if (userGroup.UserIds != null && userGroup.UserIds.Count > 0)
                {
                    foreach (var groupOwnerId in userGroup.OwnerIds)
                    {
                        IEnumerable<Project> projectList = projectService.GetProjectsByOwnerId(groupOwnerId, startDate, endDate).Where(p => !p.IsPublic);

                        if (isShowImportant)
                        {
                            //机票月报只需要显示重要项目
                            if (DepartmentName.SHENZHEN_FLIGHT_EN == department || DepartmentName.SHENZHEN_CORP_TRAVEL_EN == department)
                            {
                                projectList = projectList.Where(o => o.Level == ProjectLevel.High);
                            }
                            else
                            {
                                projectList = projectList.Where(o => o.IsImportant()).Where(o => o.Id != "G10432_TimeSheet").ToList();
                            }
                        }

                        foreach (var project in projectList)
                        {
                            if (!monthlyReportModels.Exists(p => p.ProjectId == project.Id))
                            {
                                bool isDone = false;
                                if (startDate < DateTimeUtil.GetCurrentMonthEndDay())
                                {
                                    isDone = project.Status == Status.Done || project.PublishDate < endDate;
                                }
                                else
                                {
                                    isDone = project.PublishDate < endDate;
                                }
                                var userGroupName = userGroup.Name;
                                if (DepartmentName.SHENZHEN_FLIGHT_EN == department)
                                {
                                    userGroupName = getAllUserGroupNamesByProject(project);
                                }
                                //hotelMonthlyReportModels.Add(new HotelMonthlyReportModel(project.Id, project.Name, userGroup.Name, (int)project.Level, isDone));
                                monthlyReportModels.Add((T)Activator.CreateInstance(typeof(T), project.Id, project.Name, userGroupName, (int)project.Level, isDone, project.GetEndDate()));
                            }
                        }
                    }
                }
            }

            return monthlyReportModels;
        }

        private int UserGroupSort(UserGroup group1, UserGroup group2)
        {
            if (group1.Name.IndexOf("测试组") > -1 && group2.Name.IndexOf("测试组") > -1)
            {
                return string.Compare(group1.Name, group2.Name);
            }
            else if (group1.Name.IndexOf("测试组") > -1)
            {
                return 1;
            }
            else if (group2.Name.IndexOf("测试组") > -1)
            {
                return -1;
            }
            else
            {
                return string.Compare(group1.Name, group2.Name);
            }
        }

        private ProjectOverviewModel BuildProjectOverviewModel(string projectId, DateTime startDate, DateTime endDate)
        {
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();
            var userService = this.GetService<UserService>();
            var project = projectService.Get(projectId);
            var owners = userService.GetByIds(project.OwnerIds);
            var users = userService.GetByIds(project.UserIds);

            var percentage = project.GetTotalPlanHour() == 0 ? 1 : project.GetTotalActualHour() / project.GetTotalPlanHour();
            var devPercentage = project.GetPlanDevHour() == 0 ? 1 : project.GetActualDevHour() / project.GetPlanDevHour();
            var testPercentage = project.GetPlanTestHour() == 0 ? 1 : project.GetActualTestHour() / project.GetPlanTestHour();
            var lastUpdatedBy = project.LastUpdatedBy == null ? string.Empty : userService.Get(project.LastUpdatedBy).Name;
            var createdBy = project.CreatedBy == null ? string.Empty : userService.Get(project.CreatedBy).Name;

            var serialNumber = project.SerialNumber;
            var productManagerName = project.ProjectManagerName;

            return new ProjectOverviewModel(project, percentage, devPercentage, testPercentage, string.Join(",", owners.Select(o => o.Name)),
                string.Join(",", users.Select(o => o.Name)), createdBy, lastUpdatedBy,
                project.PostponeReasons.IsEmpty() ? "None" : string.Join(",", project.PostponeReasons),
                project.BuildHours(startDate, endDate), project.BuildLineModel(), project.Status.ToString());
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult WeeklyReport(DateTime startDate, DateTime endDate, bool isShowImportant, bool isShowDetails)
        {
            ViewData["isShowDetails"] = isShowDetails;
            bool isFlightDepart = this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_FLIGHT);
            if (isFlightDepart)
            {
                if (startDate.IsEmpty() || endDate.IsEmpty())
                {
                    startDate = DateTimeUtil.GetCurrentPeriodStartDay();
                    endDate = DateTimeUtil.GetCurrentPeriodStartDay().AddDays(6);
                }
            }
            //如果没有指定日期，就查本周的数据
            if (startDate.IsEmpty())
            {
                startDate = DateTimeUtil.GetCurrentMonday();
            }

            if (endDate.IsEmpty())
            {
                endDate = DateTimeUtil.GetCurrentMonday().AddDays(6);
            }

            ViewData["SearchDateRange"] = new DateRange(startDate, endDate);

            //机票部门用机票的，酒店部门用酒店的
            if (this.GetUserType() == UserType.Admin)
            {
                ViewData["HotelWeeklyReportModels"] = BuildHotelWeeklyReportModel(startDate, endDate, isShowImportant);
                return View("WeeklyReport/Hotel");
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_HOTEL))
            {
                ViewData["HotelWeeklyReportModels"] = BuildHotelWeeklyReportModel(startDate, endDate, isShowImportant);
                return View("WeeklyReport/Hotel");
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_FLIGHT))
            {
                ViewData["FlightWeeklyReportModels"] = BuildFlightWeeklyReportModel(startDate, endDate, isShowDetails);
                return View("WeeklyReport/Flight");
            }
            else if (this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId()).FirstOrDefault().Id.Equals(DepartmentName.SHENZHEN_CORP_TRAVEL))
            {
                ViewData["CorporateTravelWeeklyReportModel"] = BuildCorporateTravelWeeklyReportModel(startDate, endDate);
                return View("WeeklyReport/CorporateTravel");
            }
            else
            {
                ViewData["HotelWeeklyReportModels"] = BuildShCorporateWeeklyReportModel(startDate, endDate, isShowImportant);
                return View("WeeklyReport/ShCorporateTravel");
            }
        }

        private List<HotelWeeklyReportModel> BuildShCorporateWeeklyReportModel(DateTime startDate, DateTime endDate, bool isShowImportant)
        {
            var userId = this.GetUserId();
            var departmentService = this.GetService<DepartmentService>();
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();
            var userService = this.GetService<UserService>();
            var userGroups = departmentService.GetUserGroupsByUserId(userId);

            var hotelWeeklyReportModels = new List<HotelWeeklyReportModel>();
            //排序，把测试组放最后。
            if (userGroups.Count > 0)
            {
                var department = departmentService.GetDepartmentsByUserId(userId).Where(d => d.UserGroups.Count > 0).FirstOrDefault();
                var testGroup = departmentService.GetTestGroupsByDepartmentId(department.Id).FirstOrDefault();
                var testGroupName = testGroup == null ? "" : testGroup.Name;

                userGroups.Sort(UserGroupSort);
                foreach (var userGroup in userGroups)
                {
                    HashSet<string> owners = new HashSet<string>();
                    foreach (var item in userGroup.UserIds)
                    {
                        //开发项目不放到测试组，测试组自己的项目才放到测试组。
                        if (userGroup.Name != testGroupName && !departmentService.IsTester(item))
                        {
                            owners.Add(item);
                        }
                        else if (userGroup.Name == testGroupName && departmentService.IsTester(item))
                        {
                            owners.Add(item);
                        }
                    }
                    List<Project> projects = new List<Project>();
                    foreach (var item in owners)
                    {
                        //允许一个项目属于多个组，但是不允许开发项目属于测试组。
                        IEnumerable<Project> projectIdList = projectService.GetProjectsByOwnerId(item, startDate, endDate);
                        if (userGroup.Name == testGroupName)
                        {
                            projectIdList = projectIdList.Where(o => !hotelWeeklyReportModels.Exists(p => p.Projects.Exists(q => q.Key == o.Id)));
                        }

                        projects.AddRange(projectIdList);
                    }
                   
                    if (projects != null)
                    {
                        projects = projects.Where(p => !p.IsPublic).ToList();

                        if (isShowImportant)
                        {
                            projects = projects.Where(o => o.IsImportant()).ToList();
                        }

                        var projectGroupings = projects.GroupBy(p => p.Status == Status.Done ? p.ActualDateRange.EndDate : GetProjectMaxDate(p));
                        foreach (var projectGrouping in projectGroupings)
                        {
                            List<KeyValuePair<string, string>> projectNames = projectGrouping.Select(p => new KeyValuePair<string, string>(p.Id, p.Name)).ToList();
                            string comments = string.Join("<br/>", projectGrouping.Select(p => p.Comment));
                            double devPlanHours = projectGrouping.Sum(p => p.GetPlanDevHour());
                            double devActualHours = projectGrouping.Sum(p => p.GetActualDevHour());
                            double testPlanHours = projectGrouping.Sum(p => p.GetPlanTestHour());
                            double testActualHours = projectGrouping.Sum(p => p.GetActualTestHour());
                            double devPercentage = devPlanHours == 0D ? 1 : devActualHours / devPlanHours;
                            //大于100%,设为100%.
                            devPercentage = devPercentage > 1D ? 1 : devPercentage;
                            //测试时间为0，进度设置为0.
                            double testPercentage = testPlanHours == 0D ? 0 : testActualHours / testPlanHours;
                            testPercentage = testPercentage > 1D ? 1 : testPercentage;
                            bool isDone = projectGrouping.FirstOrDefault(o => o.Status != Status.Done) == null;
                            DateTime latestDate = projectGrouping.Max(o => GetProjectMaxDate(o));
                            string progressText = ReportHelper.GetProgressText(isDone, latestDate, devPercentage, testPercentage);
                            string nextWeekPlanText = ReportHelper.GetNextWeekPlan(devPercentage, testPercentage, projectGrouping.Key, progressText);
                            //项目已发布完成，自动把进度设为100%
                            if (progressText == "已发布")
                            {
                                devPercentage = 1;
                                testPercentage = 1;
                            }
                            DateTime projectEndDate = projectGrouping.Key;
                            //如果项目已结束，显示实际的EndDate.
                            if (isDone)
                            {
                                projectEndDate = projectGrouping.Max(p => p.ActualDateRange.EndDate);
                            }
                            hotelWeeklyReportModels.Add(new HotelWeeklyReportModel(userGroup.Name, projectNames, devPercentage, testPercentage, projectEndDate, progressText, nextWeekPlanText, comments));
                        }
                    }
                }
                var sortedWeeklyReportModels = new List<HotelWeeklyReportModel>();
                var sortedTestWeeklyReportModels = new List<HotelWeeklyReportModel>();
                sortedWeeklyReportModels = hotelWeeklyReportModels.Where(o => o.UserGroupName != testGroupName).OrderBy(p => p.UserGroupName).ThenBy(p => p.EndDate).ToList();
                sortedTestWeeklyReportModels = hotelWeeklyReportModels.Where(o => o.UserGroupName == testGroupName).OrderBy(p => p.UserGroupName).ThenBy(p => p.EndDate).ToList();
                sortedWeeklyReportModels.AddRange(sortedTestWeeklyReportModels.Where(o => !sortedWeeklyReportModels.Contains(o)));
                hotelWeeklyReportModels = sortedWeeklyReportModels;
            }

            return hotelWeeklyReportModels;
        }

        private List<HotelWeeklyReportModel> BuildHotelWeeklyReportModel(DateTime startDate, DateTime endDate, bool isShowImportant)
        {
            var userId = this.GetUserId();
            var departmentService = this.GetService<DepartmentService>();
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();
            var userService = this.GetService<UserService>();
            var userGroups = departmentService.GetUserGroupsByUserId(userId);

            var hotelWeeklyReportModels = new List<HotelWeeklyReportModel>();
            //排序，把测试组放最后。
            if (userGroups.Count > 0)
            {
                userGroups.Sort(UserGroupSort);
                foreach (var userGroup in userGroups)
                {
                    HashSet<string> owners = new HashSet<string>();
                    foreach (var item in userGroup.UserIds)
                    {
                        //开发项目不放到测试组，测试组自己的项目才放到测试组。
                        if (userGroup.Name != "深圳酒店测试组" && !departmentService.IsTester(item))
                        {
                            owners.Add(item);
                        }
                        else if (userGroup.Name == "深圳酒店测试组" && departmentService.IsTester(item))
                        {
                            owners.Add(item);
                        }
                    }
                    List<Project> projects = new List<Project>();
                    foreach (var item in owners)
                    {
                        //允许一个项目属于多个组，但是不允许开发项目属于测试组。
                        IEnumerable<Project> projectIdList = projectService.GetProjectsByOwnerId(item, startDate, endDate);
                        if (userGroup.Name == "深圳酒店测试组")
                        {
                            projectIdList = projectIdList.Where(o => !hotelWeeklyReportModels.Exists(p => p.Projects.Exists(q => q.Key == o.Id)));
                        }

                        projects.AddRange(projectIdList);
                    }

                    ///*var projectIds */= timeSheetService.GetWorkingProjectIds(userGroup.UserIds, startDate, endDate);
                    if (projects != null)
                    {
                        projects = projects.Where(p => !p.IsPublic).ToList();

                        if (isShowImportant)
                        {
                            projects = projects.Where(o => o.IsImportant()).Where(o => o.Id != "G10432_TimeSheet").ToList();
                        }

                        var projectGroupings = projects.GroupBy(p => p.Status == Status.Done ? p.ActualDateRange.EndDate : GetProjectMaxDate(p));
                        foreach (var projectGrouping in projectGroupings)
                        {
                            List<KeyValuePair<string, string>> projectNames = projectGrouping.Select(p => new KeyValuePair<string, string>(p.Id, p.Name)).ToList();
                            string comments = string.Join("<br/>", projectGrouping.Select(p => p.Comment));
                            double devPlanHours = projectGrouping.Sum(p => p.GetPlanDevHour());
                            double devActualHours = projectGrouping.Sum(p => p.GetActualDevHour());
                            double testPlanHours = projectGrouping.Sum(p => p.GetPlanTestHour());
                            double testActualHours = projectGrouping.Sum(p => p.GetActualTestHour());
                            double devPercentage = devPlanHours == 0D ? 1 : devActualHours / devPlanHours;
                            //大于100%,设为100%.
                            devPercentage = devPercentage > 1D ? 1 : devPercentage;
                            //测试时间为0，进度设置为0.
                            double testPercentage = testPlanHours == 0D ? 0 : testActualHours / testPlanHours;
                            testPercentage = testPercentage > 1D ? 1 : testPercentage;
                            bool isDone = projectGrouping.FirstOrDefault(o => o.Status != Status.Done) == null;
                            DateTime latestDate = projectGrouping.Max(o => GetProjectMaxDate(o));
                            string progressText = ReportHelper.GetProgressText(isDone, latestDate, devPercentage, testPercentage);
                            string nextWeekPlanText = ReportHelper.GetNextWeekPlan(devPercentage, testPercentage, projectGrouping.Key, progressText);
                            //项目已发布完成，自动把进度设为100%
                            if (progressText == "已发布")
                            {
                                devPercentage = 1;
                                testPercentage = 1;
                            }
                            DateTime projectEndDate = projectGrouping.Key;
                            //如果项目已结束，显示实际的EndDate.
                            if (isDone)
                            {
                                projectEndDate = projectGrouping.Max(p => p.ActualDateRange.EndDate);
                            }
                            hotelWeeklyReportModels.Add(new HotelWeeklyReportModel(userGroup.Name, projectNames, devPercentage, testPercentage, projectEndDate, progressText, nextWeekPlanText, comments));
                        }
                    }
                }
                var sortedWeeklyReportModels = new List<HotelWeeklyReportModel>();
                var sortedTestWeeklyReportModels = new List<HotelWeeklyReportModel>();
                sortedWeeklyReportModels = hotelWeeklyReportModels.Where(o => o.UserGroupName != "深圳酒店测试组").OrderBy(p => p.UserGroupName).ThenBy(p => p.EndDate).ToList();
                sortedTestWeeklyReportModels = hotelWeeklyReportModels.Where(o => o.UserGroupName == "深圳酒店测试组").OrderBy(p => p.UserGroupName).ThenBy(p => p.EndDate).ToList();
                sortedWeeklyReportModels.AddRange(sortedTestWeeklyReportModels.Where(o => !sortedWeeklyReportModels.Contains(o)));
                hotelWeeklyReportModels = sortedWeeklyReportModels;
            }

            return hotelWeeklyReportModels;
        }

        private DateTime GetProjectMaxDate(Project project)
        {
            DateTime maxDate = DateTime.MinValue;
            if (project.Tasks != null && project.Tasks.Count > 0)
            {
                maxDate = project.Tasks.Select(p => p.ActualDateRange.EndDate).Max();
            }

            return new DateTime[] { maxDate, project.ActualDateRange.EndDate, project.PublishDate, project.PlanDateRange.EndDate }.Max();
        }

        private string getAllUserGroupNamesByProject(Project project)
        {
            var departmentService = GetService<DepartmentService>();
            //遍历参与项目的组别，去掉'深圳机票'前缀
            var projUserGroups = departmentService.GetUserGroupsByOwnerIds(project.OwnerIds);
            List<string> groupNames = new List<string>();
            foreach (var projUserGroup in projUserGroups)
            {
                groupNames.Add(projUserGroup.Name);
            }
            return string.Join("\\", groupNames).Replace("深圳机票", "");
        }

        private List<FlightWeeklyReportModel> BuildFlightWeeklyReportModel(DateTime startDate, DateTime endDate, bool isShowDetails)
        {
            var userId = this.GetUserId();
            var departmentService = GetService<DepartmentService>();
            var projectService = GetService<ProjectService>();
            var timeSheetService = GetService<TimeSheetService>();
            var userService = GetService<UserService>();
            var userGroups = departmentService.GetUserGroupsByUserId(userId);

            var models = new List<FlightWeeklyReportModel>();
            if (userGroups.Count > 0)
            {
                foreach (var userGroup in userGroups)
                {
                    var projectIds = timeSheetService.GetWorkingProjectIds(userGroup.UserIds, startDate, endDate);
                    var projects = projectService.GetByIds(projectIds);
                    if (projects != null)
                    {
                        //过滤公共项目
                        projects = projects.Where(p => !p.IsPublic).ToList();
                        foreach (var project in projects)
                        {
                            //Manager拥有多个组，过滤重复项目
                            if (models.Exists(p => p.ProjectID == project.Id))
                            {
                                continue;
                            }

                            //计算开发、测试完成百分比
                            double devPercentage = ProjectExtension.GetDevPercentage(project);
                            double testPercentage = ProjectExtension.GetTestPercentage(project);
                            bool isDone = project.Status != Status.Done ? false : true;

                            //遍历参与开发、测试人员
                            List<string> devNames = new List<string>();
                            List<string> testNames = new List<string>();
                            foreach (var usrId in project.UserIds)
                            {
                                var usr = userService.Get(usrId);
                                if (departmentService.IsTester(usrId))
                                {
                                    testNames.Add(usr.Name);
                                }
                                else
                                {
                                    devNames.Add(usr.Name);
                                }
                            }

                            List<string> devManagers = new List<string>();
                            List<string> testManagers = new List<string>();
                            foreach (var ownerId in project.OwnerIds)
                            {
                                var usr = userService.Get(ownerId);
                                if (departmentService.IsTester(ownerId))
                                {
                                    testManagers.Add(usr.Name);
                                }
                                else
                                {
                                    devManagers.Add(usr.Name);
                                }
                            }

                            string progressText = ReportHelper.GetFlightProgressText(isDone, devPercentage, testPercentage);
                            string percentageCompletion = ReportHelper.GetPercentageCompletion(devNames, testNames, devPercentage, testPercentage);

                            models.Add(new FlightWeeklyReportModel(project, getAllUserGroupNamesByProject(project), percentageCompletion,
                                progressText, ReportHelper.GetProjectStatus(project), devManagers, testManagers, devNames, testNames));
                        }
                    }
                }
                //var sortedWeeklyReportModels = new List<FlightWeeklyReportModel>();
                //var sortedTestWeeklyReportModels = new List<FlightWeeklyReportModel>();
                //sortedWeeklyReportModels = models.OrderBy(p => p.UserGroupName).ThenBy(p => p.ActualEndDate).ToList();
                models = models.OrderByDescending(o => o.Project.Level).ThenBy(o => o.Project.GetEndDate()).ToList();
            }

            return models;
        }


        private List<CorporateTravelWeeklyReportModel> BuildCorporateTravelWeeklyReportModel(DateTime startDate, DateTime endDate)
        {
            var userId = this.GetUserId();
            var departmentService = GetService<DepartmentService>();
            var projectService = GetService<ProjectService>();
            var timeSheetService = GetService<TimeSheetService>();
            var userService = GetService<UserService>();
            var userGroups = departmentService.GetUserGroupsByUserId(userId);

            var models = new List<CorporateTravelWeeklyReportModel>();
            if (userGroups.Count > 0)
            {
                foreach (var userGroup in userGroups)
                {
                    var projectIds = timeSheetService.GetWorkingProjectIds(userGroup.UserIds, startDate, endDate);
                    var projects = projectService.GetByIds(projectIds);
                    if (projects != null)
                    {
                        //过滤公共项目
                        projects = projects.Where(p => !p.IsPublic && !p.IsCr).ToList();
                        foreach (var project in projects)
                        {
                            //Manager拥有多个组，过滤重复项目
                            if (models.Exists(p => p.ProjectID == project.Id))
                            {
                                continue;
                            }

                            //计算开发、测试完成百分比
                            double devPercentage = ProjectExtension.GetDevPercentage(project);
                            double testPercentage = ProjectExtension.GetTestPercentage(project);
                            bool isDone = project.Status != Status.Done ? false : true;

                            //遍历参与开发、测试人员
                            List<string> devNames = new List<string>();
                            List<string> testNames = new List<string>();
                            foreach (var usrId in project.UserIds)
                            {
                                var usr = userService.Get(usrId);
                                if (departmentService.IsTester(usrId))
                                {
                                    testNames.Add(usr.Name);
                                }
                                else
                                {
                                    devNames.Add(usr.Name);
                                }
                            }

                            List<string> devManagers = new List<string>();
                            List<string> testManagers = new List<string>();
                            foreach (var ownerId in project.OwnerIds)
                            {
                                var usr = userService.Get(ownerId);
                                if (departmentService.IsTester(ownerId))
                                {
                                    testManagers.Add(usr.Name);
                                }
                                else
                                {
                                    devManagers.Add(usr.Name);
                                }
                            }

                            string progressText = ReportHelper.GetFlightProgressText(isDone, devPercentage, testPercentage);
                            string percentageCompletion = ReportHelper.GetPercentageCompletion(devNames, testNames, devPercentage, testPercentage);

                            models.Add(new CorporateTravelWeeklyReportModel(project, getAllUserGroupNamesByProject(project), percentageCompletion,
                                progressText, ReportHelper.GetProjectStatus(project), devManagers, testManagers, devNames, testNames));
                        }
                    }
                }
                models = models.OrderByDescending(o => o.Project.Level).ThenBy(o => o.Project.GetEndDate()).ToList();
            }

            return models;
        }
    }
}