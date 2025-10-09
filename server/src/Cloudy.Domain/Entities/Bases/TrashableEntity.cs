namespace Cloudy.Domain.Entities.Bases;

public abstract class TrashableEntity : TrackableEntity
{
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public int DeletedBy { get; private set; }

    public void SoftDelete(int userId)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        Touch(userId);
    }
}
