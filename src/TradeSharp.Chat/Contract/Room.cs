using System;

namespace TradeSharp.Chat.Contract
{
    /// <summary>
    /// комната чата
    /// </summary>
    public class Room
    {
        /// <summary>
        /// идентификатор
        /// </summary>
        public int Id;

        /// <summary>
        /// название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// пользователь-создатель
        /// </summary>
        public int Owner;

        /// <summary>
        /// описание (видно в списке комнат)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// приветствие (поститься при входе в комнату)
        /// </summary>
        public string Greeting;

        /// <summary>
        /// пароль
        /// </summary>
        public string Password;

        /// <summary>
        /// флаг сохранения комнаты в случае непосещения
        /// </summary>
        public bool IsBound { get; set; }

        /// <summary>
        /// дата и время автоматического удаления комнаты (в случае непосещения)
        /// </summary>
        public DateTime? ExpireTime;

        /// <summary>
        /// количество пользователей в комнате
        /// </summary>
        public int UserCount { get; set; }

        public Room()
        {
        }

        public Room(Room room)
        {
            Id = room.Id;
            Name = room.Name;
            Owner = room.Owner;
            Description = room.Description;
            Greeting = room.Greeting;
            Password = room.Password;
            ExpireTime = room.ExpireTime;
            IsBound = room.IsBound;
            UserCount = room.UserCount;
        }

        public override string ToString()
        {
            return "Room Name: " + Name + ", Owner: " + Owner + ", Id: " + Id;
        }
    }
}
