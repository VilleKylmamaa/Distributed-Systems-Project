using Microsoft.AspNetCore.SignalR;

namespace DistrChat.SignalR
{
    public class SignalrHub : Hub
    {
        public Dictionary<string, string> ConnectionToRoomMapping = new();
        public Dictionary<string, string> ConnectionToUsernameMapping = new();
        public Dictionary<string, int> RoomClientCounts = new();

        public Dictionary<string, string> GetConnectionToRoomMapping() { return ConnectionToRoomMapping; }
        public Dictionary<string, string> GetConnectionToUsernameMapping() { return ConnectionToUsernameMapping; }
        public Dictionary<string, int> GetRoomClientCounts() { return RoomClientCounts; }

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
                new Message(
                    text: $"{username} has joined the chat",
                    username: string.Empty
                )
            );

            bool isNewConnection = !ConnectionToRoomMapping.ContainsKey(connectionId);
            if (isNewConnection)
            {
                ConnectionToRoomMapping.Add(connectionId, roomName);
                ConnectionToUsernameMapping.Add(connectionId, username);
            }

            if (RoomClientCounts.TryGetValue(roomName, out int count))
            {
                RoomClientCounts[roomName] = count + 1;
            }
            else
            {
                RoomClientCounts.Add(roomName, 1);
            }
            await UpdateControlData();
        }

        public async Task LeaveRoom(User user)
        {
            var connectionId = Context.ConnectionId;

            await Clients.Group(user.RoomName).SendAsync(
                "ReceiveMessage",
                new Message(
                    text: $"{user.Username} has left the chat",
                    username: string.Empty
                )
            );

            if (RoomClientCounts.TryGetValue(user.RoomName, out int count))
            {
                if (count == 1)
                    RoomClientCounts.Remove(user.RoomName);
                else
                    RoomClientCounts[user.RoomName] = count - 1;
            }

            ConnectionToRoomMapping.Remove(connectionId);
            ConnectionToUsernameMapping.Remove(connectionId);
            await UpdateControlData();
        }

        public async Task SubscribeToControlData()
        {
            var host = Context.GetHttpContext()?.Request.Host;
            var groupName = "ControlData_" + host;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, "RedisStatusUpdates");
            await UpdateControlData();
        }

        public async Task UpdateControlData()
        {
            var host = Context.GetHttpContext()?.Request.Host;
            var groupName = "ControlData_" + host;
            await Clients.Group(groupName).SendAsync(
                "ControlViewUpdate",
                GetRoomClientCounts()
            );
        }

        /// <summary>
        /// Disconnect override - Handles case where the client closes browser instead of leaving the room first.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            bool didUpdateMappings = false;

            if (ConnectionToUsernameMapping.TryGetValue(connectionId, out string username))
            {
                if (username != null)
                {
                    ConnectionToUsernameMapping.Remove(connectionId);
                    didUpdateMappings = true;
                }
            }

            if (ConnectionToRoomMapping.TryGetValue(connectionId, out string roomName))
            {
                ConnectionToRoomMapping.Remove(connectionId);
                didUpdateMappings = true;

                if (RoomClientCounts.TryGetValue(roomName, out int count))
                {
                    // Users in the room go from 1 to 0 -> Remove room
                    if (count == 1)
                    {
                        RoomClientCounts.Remove(roomName);
                    }
                    // More than 1 user -> Decrement count
                    else
                    {
                        RoomClientCounts[roomName] = count - 1;
                    }
                }
            }

            if (didUpdateMappings)
            {
                await UpdateControlData();
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
