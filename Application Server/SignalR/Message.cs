namespace DistrChat.SignalR
{
    public class Message
    {
        public string Text { get; set; }
        public string Username { get; set; }
        public string RoomName { get; set; }
        public string Color { get; private set; }

        public Message(string text, string username)
        {
            Text = text;
            Username = username;
            Color = GetRandomColor(username);
        }

        private static string GetRandomColor(string username)
        {
            if (username == string.Empty)
            {
                return "#FFFFFF";
            }

            var randomColors = new List<string> {
                "#FAB4FF",
                "#FEEE85",
                "#BEFAE1",
                "#57BEE6",
                "#00FAFA",
                "#59FF82"
            };

            var randomNumber = new Random().Next(0, randomColors.Count);
            string randomColor = randomColors[randomNumber];

            return randomColor;
        }
    }
}
