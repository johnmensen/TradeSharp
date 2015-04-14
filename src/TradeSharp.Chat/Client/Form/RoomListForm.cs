using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Contract;
using System;

namespace TradeSharp.Chat.Client.Form
{
    public partial class RoomListForm : System.Windows.Forms.Form
    {
        private readonly ChatControlBackEnd engine;

        public RoomListForm(ChatControlBackEnd engine)
        {
            InitializeComponent();
            roomFastGrid.Columns.Add(new FastColumn("IsBound", "*")
                {
                    ImageList = imageList,
                    ColumnMinWidth = 25,
                    ColumnWidth = 25
                });
            roomFastGrid.Columns.Add(new FastColumn("Name", "Комната"));
            roomFastGrid.Columns.Add(new FastColumn("UserCount", "Пользователи") {ColumnWidth = 80});
            roomFastGrid.Columns.Add(new FastColumn("Description", "Описание"));
            roomFastGrid.SelectionChanged += RoomFastGridSelectionChanged;
            roomFastGrid.UserHitCell += RoomFastGridUserHitCell;
            this.engine = engine;
            this.engine.RoomsReceived += RoomsReceived;
            UpdateRooms();
        }

        public Room GetRoom()
        {
            var selectedRooms = roomFastGrid.GetRowValues<Room>(true).ToList();
            if (selectedRooms.Count == 0)
                return null;
            return selectedRooms[0];
        }

        private void RoomsReceived(List<Room> rooms)
        {
            Invoke(new Action<List<Room>>(OnRoomsReceive), rooms);
        }

        private void OnRoomsReceive(List<Room> rooms)
        {
            roomFastGrid.DataBind(rooms);
            UpdateUi();
        }

        private void UpdateRooms()
        {
            engine.GetRooms();
        }

        private void UpdateUi()
        {
            var enable = GetRoom() != null;
            enterButton.Enabled = enable;
            changeButton.Enabled = enable;
            destroyButton.Enabled = enable;
        }

        private void CreateButtonClick(object sender, EventArgs e)
        {
            var form = new RoomForm(engine.CurrentUserId);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            var room = form.GetRoom();
            room.Owner = engine.CurrentUserId;
            engine.CreateRoom(room);
            UpdateRooms();
        }

        private void СhangeButtonClick(object sender, EventArgs e)
        {
            var oldRoom = GetRoom();
            if (oldRoom == null)
                return;
            var form = new RoomForm(engine.CurrentUserId);
            //var oldRoomName = oldRoom.Name;
            form.SetRoom(oldRoom);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            //var room = form.GetRoom();
            //chat.UpdateRoom(oldRoomName, room);
        }

        private void DestroyButtonClick(object sender, EventArgs e)
        {
            var room = GetRoom();
            if (room == null)
                return;
            if (MessageBox.Show(this, string.Format("Удалить комнату \"{0}\"?", room.Name), "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            engine.DestroyRoom(room.Name);
            UpdateRooms();
        }

        private void RoomFastGridSelectionChanged(MouseEventArgs e, int rowIndex, FastColumn col)
        {
            UpdateUi();
            var room = GetRoom();
            if (room == null)
                return;
            descriptionRichTextBox.Text = room.Description;
        }

        private void RoomFastGridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Clicks != 2)
                return;
            var room = GetRoom();
            if (room == null)
                return;
            DialogResult = DialogResult.OK;
        }

        private void RoomListFormFormClosed(object sender, FormClosedEventArgs e)
        {
            engine.RoomsReceived -= RoomsReceived;
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            UpdateRooms();
        }
    }
}
