using System.Collections.Generic;
using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Server.BL
{
    // для хранения в очереди на отправку на сервере
    class ChatAnswer
    {
        public IClientCallback Client;
        public AnswerCode Code;
        public List<object> Arguments;

        public ChatAnswer(IClientCallback client, AnswerCode code, List<object> arguments)
        {
            Client = client;
            Code = code;
            Arguments = arguments;
        }
    }
}
