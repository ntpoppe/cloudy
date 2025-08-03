using Cloudy.Domain.Entities;

public abstract class TrackableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public int? CreatedBy { get; protected set; }
    public int? UpdatedBy { get; protected set; }

    public void Touch(int userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}