namespace Web.Controllers
{
    using Common;
    using Entity.Project;
    using Entity.User;
    using Extension;
    using Helper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Platform.Enum;
    using Platform.Extension;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Authorize]
    public class TimeSheetController : BaseController
    {
        // GET: /<controller>/
        [Authorize(Roles = "1,2,3")]
        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            ViewData["UserId"] = this.GetUserId();

            return View();
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult ManageTimeSheet(string successMsg, string errorMsg)
        {
            var departmentService = this.GetService<DepartmentService>();

            this.BuildHeaderMsg(successMsg, errorMsg);

            return View();
        }

        //Id为星期一
        public IActionResult ReadTimeSheet(DateTime monday, string userId)
        {
            var projectService = this.GetService<ProjectService>();

            var projects = projectService.Get(o => o.UserIds.Contains(userId) || o.IsPublic)
                .Where(o => o.PlanDateRange.EndDate > monday.AddDays(-1) && o.PlanDateRange.StartDate < monday.AddDays(7))
                .ToList();

            ViewData["TimeSheetModels"] = BuildTimeSheetModels(monday, userId, projects);
            ViewData["Monday"] = monday;
            ViewData["UserId"] = userId;

            return View();
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult ReturnTimeSheet(string monday, string userId)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var departmentService = this.GetService<DepartmentService>();

            //没有权限
            if (!departmentService.IsBoss(this.GetUserId(), userId))
            {
                errorMsg = "You don't have right!";
                return RedirectToAction("ManageTimeSheet", new { successMsg = successMsg, errorMsg = errorMsg });
            }

            try
            {
                var userTimeSheetStatusService = this.GetService<UserTimeSheetStatusService>();
                userTimeSheetStatusService.ReturnTimeSheet(monday, userId);
                successMsg = string.Format("Return timesheet({0}) successfully!", monday + "_" + userId);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            return RedirectToAction("ManageTimeSheet", new { successMsg = successMsg, errorMsg = errorMsg });
        }

        //Id为星期一
        public IActionResult EditTimeSheet(DateTime monday, string userId, bool isSave, [FromForm]List<TimeSheetModel> model)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            if(model != null && model.Count > 0)
            {
                try
                {
                    SaveTimeSheet(monday, userId, model, isSave);
                    successMsg = string.Format("Edit time sheet({0}) successfully!", userId + "_" + monday);
                }
                catch(Exception ex)
                {
                    errorMsg = ex.Message;
                }

                if(this.GetUserType() == UserType.User)
                {
                    return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
                }
                else
                {
                    return RedirectToAction("ManageTimeSheet", new { successMsg = successMsg, errorMsg = errorMsg });
                }
            }
            else
            {
                var userTimeSheetStatusService = this.GetService<UserTimeSheetStatusService>();
                var timeSheet = userTimeSheetStatusService.Get(userId);
                if (timeSheet == null)
                {
                    Dictionary<string, KeyValuePair<Status, double>> weeks = new Dictionary<string, KeyValuePair<Status, double>>();
                    weeks.Add(monday.GetTimeSheetId(), new KeyValuePair<Status, double>(Status.Pending, 0));
                    timeSheet = new UserTimeSheetStatus(userId, weeks);
                }

                var status = timeSheet.GetStatus(monday.GetTimeSheetId());

                //如果不是管理员，不能编辑状态为完成的TimeSheet
                if (status == Status.Done && this.GetUserType() != UserType.Admin)
                {
                    errorMsg = "You don't have right!";
                    return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
                }

                var projectService = this.GetService<ProjectService>();

                //获取当前正在工作的项目
                var projects = projectService.Get(o => o.UserIds.Contains(userId) || o.IsPublic)
                    .Where(o => o.PlanDateRange.EndDate > monday.AddDays(-1) && o.PlanDateRange.StartDate < monday.AddDays(7))
                    .Where(o => !(o.Status == Status.Done && o.ActualDateRange.EndDate < monday))
                    .ToList();

                ViewData["TimeSheetModels"] = BuildTimeSheetModels(monday, userId, projects);
                ViewData["Monday"] = monday;
                ViewData["UserId"] = userId;
                return View();
            }
        }

        private List<TimeSheetModel> BuildTimeSheetModels(DateTime monday, string userId, List<Project> projects)
        {
            var timeSheetService = this.GetService<TimeSheetService>();
            var projectService = this.GetService<ProjectService>();
            var projectIds = projects.Select(o => o.Id).ToList();

            var result = new List<TimeSheetModel>();

            var timeSheets = timeSheetService.GetWeekTimeSheetByUserId(userId, monday.GetTimeSheetId());

            foreach(var entry in timeSheets)
            {
                //if(projectIds.Contains(entry.Key))
                //{
                //    result.Add(new TimeSheetModel(entry.Key, projects.Where(o => o.Id == entry.Key).FirstOrDefault().Name, entry.Value));
                //}
                //有些项目数据被更改，导致显示错误
                var project = projectService.Get(entry.Key);

                if (project.IsPublic)
                {
                    foreach (var item in entry.Value)
                    {
                        result.Add(new TimeSheetModel(project, -1, project.Name, project.Status == Status.Done, item.Value));
                    }
                }
                else
                {
                    foreach (var item in entry.Value)
                    {
                        result.Add(new TimeSheetModel(project, item.Key, project.Tasks.Find(o => o.Id == item.Key).Name, project.Tasks.Find(o => o.Id == item.Key).Status == Status.Done, item.Value));
                    }

                    //没有TimeSheet的Task
                    var taskIds = project.Tasks.Where(o => o.UserId == userId).Select(o => o.Id).Except(entry.Value.Keys).ToList();
                    if (!taskIds.IsEmpty())
                    {
                        foreach(var taskId in taskIds)
                        {
                            result.Add(new TimeSheetModel(project, taskId, project.Tasks.Find(o => o.Id == taskId).Name, false, new double[7]));
                        }
                    }
                }
            }

            var emptyIds = projectIds.Except(result.Select(o => o.ProjectId)).ToList();

            foreach(var item in emptyIds)
            {
                var project = projectService.Get(item);

                if (project.IsPublic)
                {
                    result.Add(new TimeSheetModel(project, -1, project.Name, false, new double[7]));
                }
                else
                {
                    foreach(var task in project.Tasks)
                    {
                        if(task.UserId == userId)
                        {
                            result.Add(new TimeSheetModel(project, task.Id, task.Name, task.Status == Status.Done, new double[7]));
                        }
                    }
                }
            }

            result = result.OrderBy(o => o.IsPublic).ToList();

            return result;
        }

        private void SaveTimeSheet(DateTime monday, string userId, List<TimeSheetModel> models, bool isSave)
        {
            var timeSheetService = this.GetService<TimeSheetService>();
            var userTimeSheetStatusService = this.GetService<UserTimeSheetStatusService>();
            var projectService = this.GetService<ProjectService>();

            double hour = 0;

            var uiModels = models.GroupBy(o => o.ProjectId);

            foreach(var uiModel in uiModels)
            {
                var project = projectService.Get(uiModel.Key);
                var timeSheet = timeSheetService.Get(uiModel.Key + "_" + userId);

                if(timeSheet == null)
                {
                    timeSheet = new TimeSheet(uiModel.Key, userId);
                    timeSheetService.Create(timeSheet);
                }

                foreach (var task in uiModel)
                {
                    if (task.IsSelected)
                    {
                        timeSheet.AddWeek(monday.GetTimeSheetId(), uiModel.Key, userId, task.TaskId, task.Week);
                        hour += task.Week.Sum();
                    }
                    else
                    {
                        timeSheet.AddWeek(monday.GetTimeSheetId(), uiModel.Key, userId, task.TaskId, new double[7]);
                    }

                    //更新状态
                    projectService.UpdateStatus(uiModel.Key, task.TaskId, task.IsDone);
                }

                timeSheetService.Update(timeSheet);

                //更新动态数据
                projectService.UpdateActualParts(timeSheet);
            }

            //保存不更改状态
            if (isSave)
            {
                userTimeSheetStatusService.UpdateUserTimeSheet(userId, monday.GetTimeSheetId(), Status.Ongoing, hour);
            }
            else
            {
                userTimeSheetStatusService.UpdateUserTimeSheet(userId, monday.GetTimeSheetId(), Status.Done, hour);
            }
        }
    }
}
