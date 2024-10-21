using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KollabR8.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KollabR8.Application.ConnectionHub
{
    public class DocumentHub : Hub
    {
        private readonly IDocumentService _documentService;
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();

        public DocumentHub(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            OnlineUsers[username] = connectionId;

            await Clients.All.SendAsync("UserConnected", username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            OnlineUsers.TryRemove(username, out _);

            await Clients.All.SendAsync("UserDisconnected", username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            return OnlineUsers.Keys.ToList();
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

        public async Task UpdateDocument(int documentId, string title, string content, int userId)
        {
            var updatedDoc = await _documentService.UpdateDocumentAsync(documentId, title, content, userId);
            await Clients.Group(documentId.ToString()).SendAsync("ReceiveDocumentUpdate", updatedDoc);
        }

        public async Task DeleteDocument(int documentId)
        {
            await Clients.Group(documentId.ToString()).SendAsync("NotifyDocumentDelete", true);
        }
    }
}
