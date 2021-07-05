using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Trading.SignalR.Shared.Methods;
using Trading.SignalR.Shared.Requests;
using Trading.SignalR.Shared.Responses;

namespace Trading.WebApi.SignalR.Hubs
{
    public class VirtualMachinesHub : Hub
    {
        public async Task PingRequest(PingRequest request)
        {
            await Clients.OthersInGroup(request.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.REQUEST_METHODS[VirtualMachinesHubMethods.PING_REQUEST], request);
        }

        public async Task ChangeAppRequest(ChangeAppRequest request)
        {
            await Clients.OthersInGroup(request.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.REQUEST_METHODS[VirtualMachinesHubMethods.CHANGE_APP_REQUEST]+request.VirtualMachineId, request);
        }

        public async Task ChangeAppResponse(ChangeAppResponse response)
        {
            await Clients.OthersInGroup(response.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.RESPONSE_METHODS[VirtualMachinesHubMethods.CHANGE_APP_RESPONSE], response);
        }

        public async Task InstallAppRequest(InstallAppRequest request)
        {
            await Clients.OthersInGroup(request.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.REQUEST_METHODS[VirtualMachinesHubMethods.INSTALL_APP_REQUEST] + request.VirtualMachineId, request);
        }

        public async Task InstallAppResponse(InstallAppResponse response)
        {
            await Clients.OthersInGroup(response.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.RESPONSE_METHODS[VirtualMachinesHubMethods.INSTALL_APP_RESPONSE], response);
        }

        public async Task RemoveAppRequest(RemoveAppRequest request)
        {
            await Clients.OthersInGroup(request.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.REQUEST_METHODS[VirtualMachinesHubMethods.REMOVE_APP_REQUEST] + request.VirtualMachineId, request);
        }

        public async Task RemoveAppResponse(RemoveAppResponse response)
        {
            await Clients.OthersInGroup(response.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.RESPONSE_METHODS[VirtualMachinesHubMethods.REMOVE_APP_RESPONSE], response);
        }

        public async Task ChangeVpsUserPasswordRequest(ChangeVpsUserPasswordRequest request)
        {
            await Clients.OthersInGroup(request.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.REQUEST_METHODS[VirtualMachinesHubMethods.CHANGE_VPS_USER_PW_REQUEST] + request.VirtualMachineId, request);
        }

        public async Task ChangeVpsUserPasswordResponse(ChangeVpsUserPasswordResponse response)
        {
            await Clients.OthersInGroup(response.SignalRHubGroupId)
                .SendAsync(VirtualMachinesHubMethods.RESPONSE_METHODS[VirtualMachinesHubMethods.CHANGE_VPS_USER_PW_RESPONSE], response);
        }

        public async Task PingResponse(PingResponse response)
        {
            await Clients.Client(response.SignalRHubReceiverId)
                .SendAsync(VirtualMachinesHubMethods.RESPONSE_METHODS[VirtualMachinesHubMethods.PING_RESPONSE], response);
        }

        public async Task AddToGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            await Clients.Caller.SendAsync("Joined");

            await Clients.OthersInGroup(groupId).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupId}.");
        }

        public async Task RemoveFromGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);

            await Clients.Caller.SendAsync("Left");

            await Clients.Group(groupId).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupId}.");
        }
    }
}