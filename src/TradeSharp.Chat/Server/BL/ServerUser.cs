using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Server.BL
{
    public class ServerUser : User
    {
        public IClientCallback ClientCallback;

        public ServerUser()
        {
        }

        public ServerUser(ServerUser user)
            : base(user)
        {
            ClientCallback = user.ClientCallback;
        }

        public ServerUser(User user)
            : base(user)
        {
        }
}
}
