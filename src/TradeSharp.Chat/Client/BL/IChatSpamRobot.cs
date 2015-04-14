namespace TradeSharp.Chat.Client.BL
{
    public interface IChatSpamRobot
    {
        event ChatControlBackEnd.EnterRoomDel EnterRoom;

        event ChatControlBackEnd.SendMessageInRoomDel SendMessageInRoom;
    }
}
