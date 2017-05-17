namespace Entity.Other
{
    using Platform.Enum;
    using System.Collections.Generic;

    public class Feedback : BaseEntity
    {
        public string Description { get; set; }
        public List<string> ImgIds { get; set; }
        public string Comment { get; set; }
        //提出建议的人
        public string SuggesterId { get; set; }
        public Status Status { get; set; }
    }
}
