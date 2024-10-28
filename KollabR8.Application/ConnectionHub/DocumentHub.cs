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
        private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new ConcurrentDictionary<string, HashSet<string>>();

        public DocumentHub(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            OnlineUsers.AddOrUpdate(username,
                _ => new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    connections.Add(connectionId);
                    return connections;
                });

            await Clients.All.SendAsync("UserConnected", username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            if (OnlineUsers.TryGetValue(username, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    OnlineUsers.TryRemove(username, out _);
                    await Clients.All.SendAsync("UserDisconnected", username);
                };
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            return OnlineUsers.Keys.ToList();
        }

        public async Task JoinDocumentGroup(string documentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
        }

        public async Task LeaveDocumentGroup(string documentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);
        }

        public async Task UpdateDocument(int documentId, string title, string content, int userId)
        {
            try
            {
                var updatedDoc = await _documentService.UpdateDocumentAsync(documentId, title, content, userId);
                await Clients.Group(documentId.ToString()).SendAsync("ReceiveDocumentUpdate", updatedDoc);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DeleteDocument(int documentId)
        {
            await Clients.Group(documentId.ToString()).SendAsync("NotifyDocumentDelete", true);
        }
    }
}
