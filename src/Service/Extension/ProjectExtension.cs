namespace Service.Extension
{
    using Entity.Project;
    using Entity.ValueType;
    using Platform.Enum;
    using Project;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ProjectExtension
    {
        //更新项目相关的各种数据
        public static void UpdateProject(this Project project)
        {
            if (project.IsPublic)
            {
                return;
            }

            var timeSheets = ServiceFactory.Instance.GetService<TimeSheetService>().Get(o => o.ProjectId == project.Id);

            project.ActualHours = new Dictionary<string, double>();

            //更新总时间
            foreach (var item in timeSheets)
            {
                project.ActualHours.Add(item.UserId, item.GetTotalHours());
            }

            //更新Task
            foreach (var task in project.Tasks)
            {
                var timeSheet = timeSheets.Where(o => o.UserId == task.UserId).FirstOrDefault();

                //为空，则任务为初始状态
                if (timeSheet == null)
                {
                    task.Status = Status.Pending;
                    task.ActualHour = 0;
                    task.ActualDateRange = new DateRange();
                }
                else
                {
                    task.ActualHour = timeSheet.GetTaskHours(task.Id);

                    if (task.Status == Status.Done)
                    {
                        task.ActualDateRange.StartDate = timeSheet.GetTaskStartDate(task.Id);
                        task.ActualDateRange.EndDate = timeSheet.GetTaskEndDate(task.Id);
                    }
                    else
                    {
                        if (task.ActualHour > 0)
                        {
                            task.Status = Status.Ongoing;
                            task.ActualDateRange.StartDate = timeSheet.GetTaskStartDate(task.Id);
                            task.ActualDateRange.EndDate = DateTime.MinValue;
                        }
                        else
                        {
                            task.Status = Status.Pending;
                            task.ActualDateRange.StartDate = DateTime.MinValue;
                            task.ActualDateRange.EndDate = DateTime.MinValue;
                        }
                    }
                }
            }

            project.UpdateProjectStatus();
            project.UpdateProjectActualTime();
        }
    }
}
