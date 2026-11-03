using Microsoft.AspNetCore.Identity;

namespace AtlasInfrastructure.Identity;

public sealed class AtlasStaffUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
