namespace Entity.Project.Scrum
{
    using System.Collections.Generic;
    using ValueType;

    public class Sprint : BaseEntity
    {
        //Id为[GroupId]_[Name]

        public string GroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        //项目Id, Task Id
        public List<KeyValuePair<string, string>> TaskIds { get; set; }

        public DateRange DateRange { get; set; }

        //Project的OwnerIds和UserIds
        public string UserIds { get; set; }

        public Sprint()
        {

        }
    }
}
