using System;

namespace TradeSharp.Chat.Contract
{
    public class Message
    {
        public int Sender;
        public int Receiver;
        public string Room;
        public string Text;
        public DateTime TimeStamp;

        public Message()
        {
        }

        public Message(Message message)
        {
            Sender = message.Sender;
            Receiver = message.Receiver;
            Room = message.Room;
            Text = message.Text;
            TimeStamp = message.TimeStamp;
        }

        public override string ToString()
        {
            return "[" + TimeStamp + "] from " + Sender +
                (Receiver != 0 ? " to " + Receiver : " in " + Room) + ": " + Text;
        }
    }
}
