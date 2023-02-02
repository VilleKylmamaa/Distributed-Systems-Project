using Microsoft.AspNetCore.SignalR;

namespace DistrChat.SignalR
{
    public class SignalrHub : Hub
    {
        public async Task MessageToRoom(Message message)
        {
            await Clients.Group(message.RoomName).SendAsync(
                "ReceiveMessage",
                message
            );
        }

        public async Task JoinRoom(User user)
        {
            var username = user.Username;
            var roomName = user.RoomName;
            var connectionId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connectionId, roomName);

            await Clients.Group(roomName).SendAsync(
                "ReceiveMessage",
                new Message {
                    Text = $"{username} has joined the chat",
                    Username = ""
                }
            );
        }

        public async Task LeaveRoom(User user)
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, user.RoomName);

            await Clients.Group(user.RoomName).SendAsync(
                "ReceiveMessage",
                new Message
                {
                    Text = $"{user.Username} has left the chat",
                    Username = ""
                }
            );
        }
    }
}
