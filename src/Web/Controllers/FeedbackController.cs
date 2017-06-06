namespace Web.Controllers
{
    using Common;
    using Entity.Other;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Util;
    using Service.Other;
    using System;
    using Model.Other;
    using Service.User;
    using System.Collections.Generic;
    using Platform.Enum;
    using Platform.Model.BootstrapFileInput;
    using Platform.Extension;
    using Service.Media;
    using System.Linq;

    [Authorize]
    public class FeedbackController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<FeedbackController>();

        // GET: /<controller>/
        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);

            ViewData["Feedbacks"] = BuildFeedbackModels(this.GetService<FeedbackService>().Get());
            return View();
        }

        private FeedbackModel BuildFeedbackModel(Feedback feedback)
        {
            bool isEdit = false;
            bool isDelete = false;

            var user = this.GetService<UserService>().Get(feedback.SuggesterId);

            if (this.GetUserType() == UserType.Admin || this.GetUserId() == feedback.SuggesterId)
            {
                isEdit = true;
                isDelete = true;
            }

            return new FeedbackModel(feedback, user != null ? user.Name : "", isEdit, isDelete);
        }

        private List<FeedbackModel> BuildFeedbackModels(List<Feedback> feedbacks)
        {
            var result = new List<FeedbackModel>();

            foreach(var item in feedbacks)
            {
                result.Add(BuildFeedbackModel(item));
            }

            result = result.OrderBy(o => o.Feedback.Status).ToList();

            return result;
        }

        public IActionResult AddFeedback(Feedback feedback)
        {
            if (string.IsNullOrEmpty(feedback.Description))
            {
                return View();
            }

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            try
            {
                {
                    feedback.SuggesterId = this.GetUserId();
                    var id = this.GetService<FeedbackService>().Create(feedback);
                    successMsg = string.Format("Add feedback({0}) successfully!", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                errorMsg = ex.Message;
            }

            return RedirectFeedback(successMsg, errorMsg);
        }

        public IActionResult DeleteFeedback(string id)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var feedbackService = this.GetService<FeedbackService>();
            var feedback = feedbackService.Get(id);

            if (feedback == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectFeedback(successMsg, errorMsg);
            }

            if (this.GetUserType() != UserType.Admin && this.GetUserId() != feedback.SuggesterId)
            {
                errorMsg = "You don't have right!";
                return RedirectFeedback(successMsg, errorMsg);
            }

            try
            {
                feedbackService.Delete(id);
                successMsg = string.Format("Delete feedback({0}) successfully!", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                errorMsg = ex.Message;
            }

            return RedirectFeedback(successMsg, errorMsg);
        }

        public IActionResult EditFeedback(string id, Feedback feedback)
        {
            var feedbackService = this.GetService<FeedbackService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            if (string.IsNullOrEmpty(feedback.Description))
            {
                var dbFeedback = feedbackService.Get(id);

                if (dbFeedback == null)
                {
                    errorMsg = id + "doesn't exist!";
                    return RedirectFeedback(successMsg, errorMsg);
                }

                ViewData["Feedback"] = dbFeedback;
                ViewData["SendingData"] = BuildSendingData(dbFeedback);
                return View();
            }
            else
            {
                try
                {
                    var dbFeedback = feedbackService.Get(id);

                    if (this.GetUserType() != UserType.Admin && this.GetUserId() != dbFeedback.SuggesterId)
                    {
                        errorMsg = "You don't have right!";
                        return RedirectFeedback(successMsg, errorMsg);
                    }

                    dbFeedback.Description = feedback.Description;
                    dbFeedback.ImgIds = feedback.ImgIds;
                    dbFeedback.Status = feedback.Status;
                    dbFeedback.Comment = feedback.Comment;

                    feedbackService.Update(dbFeedback);

                    successMsg = string.Format("Edit feedback({0}) successfully!", feedback.Id);
                }
                catch(Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectFeedback(successMsg, errorMsg);
            }
        }

        private SendingData BuildSendingData(Feedback feedback)
        {
            var result = new SendingData(new List<InitialPreview>(), new List<InitialPreviewConfig>());
            var imgService = this.GetService<ImgService>();


            if (!feedback.ImgIds.IsEmpty())
            {
                foreach (var item in feedback.ImgIds)
                {
                    var img = imgService.Get(item);

                    if(img != null)
                    {
                        result.initialPreviewConfigs.Add(new InitialPreviewConfig(img.Name, "120px", "/api/img/remove", img.Id, new { feedbackId = feedback.Id }));
                        result.initialPreviews.Add(new InitialPreview(img.Id, "file-preview-image", img.Name, img.Name));
                    }
                }
            }

            return result;
        }

        private IActionResult RedirectFeedback(string successMsg, string errorMsg)
        {
            return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
        }
    }
}
