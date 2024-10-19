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
        public async Task JoinDocumentGroup(string documentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
        }

        public async Task LeaveDocumentGroup(string documentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);
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
