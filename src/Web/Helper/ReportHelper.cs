namespace Web.Helper
{
    using System;
    using Platform.Util;
    using System.Collections.Generic;
    using Platform.Extension;
    using Entity.Project;
    public static class ReportHelper
    {
        // 解析本周的任务进度状况。
        public static string GetProgressText(bool isDone, DateTime planEndDate, double devPercentage, double testPercentage)
        {
            if (isDone || planEndDate <= DateTimeUtil.GetCurrentWeekEndDay())
            {
                return "已发布";
            }
            else if (devPercentage >= 1D && testPercentage >= 1D)
            {
                return "待发布";
            }
            else if (devPercentage == 0D && testPercentage == 0D)
            {
                return "未开始";
            }
            else if (devPercentage > 0D && testPercentage == 0D)
            {
                return "开发";
            }
            else if (devPercentage == 0D && testPercentage > 0D)
            {
                return "测试";
            }
            else if (devPercentage > 0D && testPercentage > 0D)
            {
                return "开发,测试";
            }
            else
            {
                return "";
            }
        }

        public static string GetFlightProgressText(bool isDone, double devPercentage, double testPercentage)
        {
            if(isDone) return "已上线";
            if (devPercentage == 0D && testPercentage == 0D) return "未开始";
            if (devPercentage >= 1D) // 包含 单测试项目
            {
                if (testPercentage >= 1D) return "待发布";
                else if (testPercentage == 0D) return "待测试";
                else return "测试中";
            }
            if (devPercentage > 0D)
            {
                return testPercentage == 0D ? "开发中" : "开发测试中";
            }

            return string.Empty;
        }

        //完成百分比
        public static string GetPercentageCompletion(List<string> devNames, List<string> testNames, double devPercentage, double testPercentage)
        {
            if (devNames.IsEmpty())
            {
                return string.Format("测试:{0}%", (testPercentage * 100).ToString("0"));
            }
            if (testNames.IsEmpty())
            {
                return string.Format("开发:{0}%", (devPercentage * 100).ToString("0"));
            }
            return string.Format("开发:{0}%\r\n测试:{1}%", (devPercentage * 100).ToString("0"), (testPercentage * 100).ToString("0"));

        }

        public static string GetAllParticipants(List<string> devNames, List<string> testNames)
        {
            if (devNames.IsEmpty())
            {
                return string.Format("测试:{0}", string.Join("\\", testNames));
            }
            if (testNames.IsEmpty())
            {
                return string.Format("开发:{0}", string.Join("\\", devNames));
            }
            return string.Format("开发:{0} 测试:{1}", string.Join("\\", devNames), string.Join("\\", testNames));
        }

        //状态：准时；延期；提前
        public static string GetProjectStatus(Project project)
        {
            if (!project.PostponeReasons.IsEmpty())
            {
                return "延期";
            }
            int temp = project.GetEndDate().CompareTo(project.PlanEndDate);
            if (project.Status == Platform.Enum.Status.Done)
            {
                if (temp < 0 && !project.ActualDateRange.EndDate.IsEmpty())
                {
                    return "提前";
                }
                else if (temp > 0)
                {
                    return "延期";
                }
                else
                {
                    return "准时";
                }
            }
            else if (temp > 0 || DateTime.Today.CompareTo(project.GetEndDate()) > 0)
            {
                return "延期";
            }

            if (project.GetRiskFactor() > 200)
            {
                return "可能延期";
            }
            return "准时";
        }
    

        // 大致判断下周任务。
        public static string GetNextWeekPlan(double devPercentage, double testPercentage, DateTime planEndDate, string status)
        {
            if (status == "已发布")
            {
                return "无";
            }
            else if (status == "待发布")
            {
                return "发布";
            }
            else if (devPercentage < 1D && testPercentage == 0D)
            {
                return "开发";
            }
            else if (devPercentage < 1D && testPercentage > 0D)
            {
                return "开发,测试";
            }
            else if (devPercentage >= 1D && testPercentage < 1D)
            {
                return "测试";
            }
            else if (testPercentage > 1D)
            {
                return "测试";
            }
            else if (planEndDate < DateTimeUtil.GetCurrentMonday().AddDays(7).AddDays(6))
            {
                return "发布";
            }
            else
            {
                return "";
            }
        }
    }
}
