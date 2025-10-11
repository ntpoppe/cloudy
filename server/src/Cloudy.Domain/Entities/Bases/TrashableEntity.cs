namespace Cloudy.Domain.Entities.Bases;

public abstract class TrashableEntity : TrackableEntity
{
    public bool IsDeleted { get; private set; }
    public bool IsPendingDeletion { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public int DeletedBy { get; private set; }

    public void MarkAsPendingDeletion(int userId)
    {
        IsPendingDeletion = true;
        Touch(userId);
    }

    public void RestoreFromPendingDeletion(int userId)
    {
        IsPendingDeletion = false;
        Touch(userId);
    }
    
    public void SoftDelete(int userId)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        Touch(userId);
    }
}
