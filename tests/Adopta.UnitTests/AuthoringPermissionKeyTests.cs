using Adopta.Application.Identity;

namespace Adopta.UnitTests;

public sealed class AuthoringPermissionKeyTests
{
    [Fact]
    public void Authoring_permission_keys_are_represented_in_catalog()
    {
        Assert.Contains(AdoptaPermissionKeys.AuthoringRead, AdoptaPermissionKeys.All);
        Assert.Contains(AdoptaPermissionKeys.AuthoringManage, AdoptaPermissionKeys.All);
        Assert.Contains(AdoptaPermissionKeys.AuthoringReview, AdoptaPermissionKeys.All);
        Assert.Contains(AdoptaPermissionKeys.AuthoringApprove, AdoptaPermissionKeys.All);
        Assert.Contains(AdoptaPermissionKeys.AuthoringPublish, AdoptaPermissionKeys.All);
    }
}
