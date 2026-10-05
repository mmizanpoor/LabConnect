using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabConnectPortal.Api.Infrastructure.Audit;

public interface IAuditableEntityRegistry
{
    bool TryGetDescriptor(Type clrType, out AuditableEntityDescriptor descriptor);

    bool IsIgnoredProperty(AuditableEntityDescriptor descriptor, string propertyName);
}
