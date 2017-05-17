using System.Collections.Generic;

namespace Entity.Project
{
    //主要是为了统计
    public class ProjectGroup : BaseEntity
    {
        public string Name { get; set; }
        public List<string> OwnerIds { get; set; }
        public List<string> projectIds { get; set; }
    }
}
