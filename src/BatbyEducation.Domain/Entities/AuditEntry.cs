using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents an audit trail entry recording a change to a domain entity field.
/// </summary>
public class AuditEntry : Entity
{
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string FieldName { get; private set; } = string.Empty;
    public string OldValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }

    private AuditEntry() { }

    public static AuditEntry Create(Guid entityId, string entityType, string fieldName, string oldValue, string newValue)
    {
        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = entityType,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        };
    }
}
