public abstract class TrashableEntity : TrackableEntity
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public int? DeletedBy { get; protected set; }

    public void SoftDelete()
    {
        if (IsDeleted) return;
        IsDeleted  = true;
        DeletedAt  = DateTime.UtcNow;
    }
}
