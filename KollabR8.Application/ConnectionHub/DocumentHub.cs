using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace KollabR8.Application.ConnectionHub
{
    public class DocumentHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            await Clients.All.SendAsync("UserConnected", username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            await Clients.All.SendAsync("UserDisconnected", username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinDocumentGroup(string documentId)
        {
            var username = Context.User.Identity.Name;
            await Groups.AddToGroupAsync(username, documentId);
        }

        public async Task LeaveDocumentGroup(string documentId)
        {
            var username = Context.User.Identity.Name;
            await Groups.RemoveFromGroupAsync(username, documentId);
        }

        public async Task SendDocumentUpdate(string documentId, string content)
        {
            await Clients.Group(documentId).SendAsync("RecieveDocumentUpdate", content);
        }

        public async Task NotifyCollaborators(string documentId, string message)
        {
            await Clients.Group(documentId).SendAsync("RecieveNotification", message);
        }
    }
}
