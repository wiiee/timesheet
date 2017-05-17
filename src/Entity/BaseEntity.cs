namespace Entity
{
    using System;

    public abstract class BaseEntity : IEntity
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdate { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Id == (obj as BaseEntity).Id;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id.GetHashCode();
            //return base.GetHashCode();
        }
    }
}
