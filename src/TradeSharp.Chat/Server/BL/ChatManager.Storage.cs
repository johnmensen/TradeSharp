using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Chat.Server.BL.Model;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Server.BL
{
    public partial class ChatManager
    {
        // все пользователи (только для использования внутри GetUserAccount, GetUserAccountsFromDb)
        private readonly List<User> userAccounts = new List<User>();
        // блокировка для записи
        private readonly object fileLock = new object();
        // время последнего сохранения сообщений
        // (время самого последнего из сохраненных в БД сообщений)
        private DateTime lastSavedMessageTime;

        private List<Contract.Room> LoadRooms()
        {
            var result = new List<Contract.Room>();
            lock (fileLock)
            {
                using (var context = DatabaseContext.Instance.MakeChat())
                {
                    try
                    {
                        foreach (var room in context.Room)
                        {
                            var newRoom = new Contract.Room
                                {
                                    Name = room.Name,
                                    Owner = room.Owner,
                                    Description = room.Description,
                                    Greeting = room.Greeting,
                                    Password = room.Password,
                                    ExpireTime = room.ExpireTimeStamp,
                                    IsBound = room.IsBound
                                };
                            result.Add(newRoom);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("LoadRoomsFromDb()", ex);
                        throw;
                    }
                }
            }
            return result;
        }

        private void SaveRooms()
        {
            bool timeoutFlag;
            var allRooms = rooms.GetAll(lockTimeout, out timeoutFlag).Select(r => new Contract.Room(r)).ToList();
            SaveRooms(allRooms);
        }

        private void SaveRooms(List<Contract.Room> allRooms)
        {
            lock (fileLock)
            {
                using (var context = DatabaseContext.Instance.MakeChat())
                {
                    try
                    {
                        foreach (var room in allRooms)
                        {
                            var dbRoom = context.Room.FirstOrDefault(r => r.Name == room.Name);
                            if (dbRoom == null) // новая комната - вставляем в БД
                            {
                                var newRoom = new Model.Room
                                    {
                                        Name = room.Name,
                                        Owner = room.Owner,
                                        Description = room.Description,
                                        Greeting = room.Greeting,
                                        Password = room.Password,
                                        ExpireTimeStamp = room.ExpireTime,
                                        IsBound = room.IsBound
                                    };
                                context.Room.Add(newRoom);
                            }
                            else // существующая комната - обновляем БД // TODO: проверить необходимость обновления
                            {
                                dbRoom.Owner = room.Owner;
                                dbRoom.Description = room.Description;
                                dbRoom.Greeting = room.Greeting;
                                dbRoom.Password = room.Password;
                                dbRoom.ExpireTimeStamp = room.ExpireTime;
                                dbRoom.IsBound = room.IsBound;
                            }
                        }
                        // удаленные комнаты удаляем из БД
                        foreach (var dbRoom in context.Room)
                        {
                            var room = allRooms.FirstOrDefault(r => r.Name == dbRoom.Name);
                            if (room == null)
                                context.Room.Remove(dbRoom);
                        }
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("SaveRooms()", ex);
                    }
                }
            }
        }

        private List<Contract.Message> LoadMessages()
        {
            //return LoadMessagesFromFile();
            return LoadMessagesFromDb();
            //return new List<Message>();
        }

        private List<Contract.Message> LoadMessagesFromFile()
        {
            var result = new List<Contract.Message>();
            lock (fileLock)
            {
                using (var fs = new FileStream(ExecutablePath.ExecPath + "/messages.log", FileMode.OpenOrCreate))
                using (var sr = new StreamReader(fs))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            break;
                        var words = line.Split(new[] { "&#32;" }, StringSplitOptions.None);
                        if (words.Length < 5)
                            break;
                        try
                        {
                            var message = new Contract.Message
                            {
                                TimeStamp = DateTime.Parse(words[0]),
                                Sender = words[1].ToInt(),
                                Room = words[2],
                                Receiver = words[3].ToInt(),
                                Text = words[4].Replace("&#10;", "\n")
                            };
                            result.Add(message);
                            if (message.TimeStamp > lastSavedMessageTime)
                                lastSavedMessageTime = message.TimeStamp;
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                }
            }
            return result;
        }

        private List<Contract.Message> LoadMessagesFromDb()
        {
            var result = new List<Contract.Message>();
            lock (fileLock)
            {
                using (var context = DatabaseContext.Instance.MakeChat())
                {
                    try
                    {
                        foreach (var message in context.Message)
                        {
                            var newMessage = new Contract.Message
                            {
                                TimeStamp = message.SendTimeStamp,
                                Sender = message.Sender,
                                Room = string.IsNullOrEmpty(message.RoomName) ? "" : message.RoomName,
                                Receiver = message.Receiver.HasValue ? message.Receiver.Value : 0,
                                Text = message.Text
                            };
                            result.Add(newMessage);
                            if (newMessage.TimeStamp > lastSavedMessageTime)
                                lastSavedMessageTime = newMessage.TimeStamp;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("LoadMessagesFromDb()", ex);
                        throw;
                    }
                }
            }
            return result;
        }

        private void SaveMessages()
        {
            while (!isStopping)
            {
                SaveMessagesToDb();
                //SaveMessagesToFile();
                Thread.Sleep(1000);
            }
        }

        private void SaveMessagesToFile()
        {
            var messagesToSave = GetMessagesInternal(lastSavedMessageTime);
            lock (fileLock)
            {
                using (var fs = new FileStream(ExecutablePath.ExecPath + "/messages.log", FileMode.Append))
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var message in messagesToSave)
                    {
                        message.Text = message.Text.Replace("\n", "&#10;");
                        sw.WriteLine(message.TimeStamp + "&#32;" + message.Sender + "&#32;" + message.Room + "&#32;" +
                                 message.Receiver + "&#32;" + message.Text);
                        if (message.TimeStamp > lastSavedMessageTime)
                            lastSavedMessageTime = message.TimeStamp;
                    }
                }
            }
        }

        private void SaveMessagesToDb()
        {
            var messagesToSave = GetMessagesInternal(lastSavedMessageTime);
            SaveMessagesToDb(messagesToSave);
            if (messagesToSave.Count != 0)
                lastSavedMessageTime = messagesToSave.Select(m => m.TimeStamp).Max();
        }

        private void SaveMessagesToDb(List<Contract.Message> messagesToSave)
        {
            lock (fileLock)
            {
                using (var context = DatabaseContext.Instance.MakeChat())
                {
                    try
                    {

                        foreach (var message in messagesToSave)
                        {
                            context.Message.Add(new Model.Message
                                {
                                    SendTimeStamp = message.TimeStamp,
                                    Receiver = message.Receiver == 0 ? (int?) null : message.Receiver,
                                    Sender = message.Sender,
                                    RoomName = string.IsNullOrEmpty(message.Room) ? null : message.Room,
                                    Text = message.Text
                                });
                        }
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("SaveMessagesToDb()", ex);
                    }
                }
            }
        }

        private void SaveMessagesAndRooms()
        {
            while (!isStopping)
            {
                SaveMessagesAndRoomsToDb();
                Thread.Sleep(1000);
            }
        }

        private void SaveMessagesAndRoomsToDb()
        {
            // делаем "снимок" комнат и сообщений
            bool timeoutFlag;
            var allRooms = rooms.GetAll(lockTimeout, out timeoutFlag).Select(r => new Contract.Room(r)).ToList();
            var messagesToSave = GetMessagesInternal(lastSavedMessageTime);

            // из-за того, что "снимки" производились в разное время, могло возникнуть рассогласование между ними
            // рассогласование недопустимо в БД, поэтому пытаемся согласовать их
            messagesToSave.RemoveAll(m => !string.IsNullOrEmpty(m.Room) && !allRooms.Exists(r => r.Name == m.Room));
            SaveRooms(allRooms);
            SaveMessagesToDb(messagesToSave);
            if (messagesToSave.Count != 0)
                lastSavedMessageTime = messagesToSave.Select(m => m.TimeStamp).Max();
        }

        private User GetUserAccount(int id)
        {
            var result = userAccounts.FirstOrDefault(u => u.ID == id);
            if (result != null)
                return result;
            if (DateTime.Now.Subtract(lastRequestPlatformUserTime).TotalMinutes > 1)
            {
                GetUserAccountsFromDb();
                return userAccounts.FirstOrDefault(u => u.ID == id);
            }
            return null;
        }

        private void GetUserAccountsFromDb()
        {
            using (var cxt = DatabaseContext.Instance.MakeTerminal())
            {
                userAccounts.Clear();
                userAccounts.AddRange(cxt.PLATFORM_USER.Select(x =>
                                                               new User
                                                                   {
                                                                       ID = x.ID,
                                                                       Title = x.Title,
                                                                       Name = x.Name,
                                                                       Surname = x.Surname,
                                                                       Patronym = x.Patronym,
                                                                       Description = x.Description,
                                                                       Login = x.Login,
                                                                       RoleMask = (UserRole) x.RoleMask
                                                                   }
                                          ));
            }
            lastRequestPlatformUserTime = DateTime.Now;
        }
    }
}
