namespace Service.Other
{
    using Entity.Other;
    using Media;
    using Platform.Context;
    using Platform.Extension;

    public class FeedbackService : BaseService<Feedback>
    {
        public FeedbackService(IContextRepository contextRepository) : base(contextRepository) { }

        public override void Delete(string id)
        {
            var feedback = Get(id);

            if(!feedback.ImgIds.IsEmpty())
            {
                ServiceFactory.Instance.GetService<ImgService>().Delete(feedback.ImgIds);
            }

            base.Delete(id);
        }

        public void RemoveImg(string feedbackId, string imgId)
        {
            var feedback = Get(feedbackId);

            if(feedback != null && !feedback.ImgIds.IsEmpty() && feedback.ImgIds.Contains(imgId))
            {
                feedback.ImgIds.Remove(imgId);
                Update(feedbackId, "ImgIds", feedback.ImgIds);
            }
        }
    }
}
