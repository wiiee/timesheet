namespace Entity
{
    using System;

    public interface IEntity
    {
        string Id { get; set; }
        DateTime Created { get; set; }
        DateTime LastUpdate { get; set; }
        string CreatedBy { get; set; }
        string LastUpdatedBy { get; set; }
    }
}
