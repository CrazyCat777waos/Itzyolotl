using Itzyolotl.Web.Staff.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Staff.Components;

public partial class AccessForm
{
    [Inject]
    private StaffAccess Access { get; set; } = default!;

    private string key = string.Empty;

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        await Access.SignInAsync(key);
        key = string.Empty;
    }
}