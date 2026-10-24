using Core.Security.Constants;
using Core.Security.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Modules.Notification.Realtime;

[Authorize]
public sealed class CreditApplicationsHub : Hub
{
    public const string Path = "/hubs/credit-applications";
    public const string CreditApplicationChanged = "creditApplicationChanged";

    public override async Task OnConnectedAsync()
    {
        System.Security.Claims.ClaimsPrincipal user = Context.User
            ?? throw new HubException("Unauthenticated.");

        if (user.IsInRole(GeneralOperationClaims.BankStaff)
            || user.IsInRole(GeneralOperationClaims.BankManager))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, CreditApplicationRealtimeGroups.Staff);
        }

        Guid? customerId = user.GetCustomerId();
        if (customerId.HasValue)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                CreditApplicationRealtimeGroups.Customer(customerId.Value));
        }

        await base.OnConnectedAsync();
    }
}
