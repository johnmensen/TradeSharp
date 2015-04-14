using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Controls
{
    public partial class ToolButtonSettingsPanel : UserControl
    {
        private ImageList lstIcons;

        private ChartToolButtonSettings.ToolButtonType buttonType;

        public ToolButtonSettingsPanel()
        {
            InitializeComponent();
            treeButtons.NodeMouseClick += (sender, args) => treeButtons.SelectedNode = args.Node;
        }

        public ToolButtonSettingsPanel(ImageList lstIcons) : this()
        {
            this.lstIcons = lstIcons;
            btnPicture.ImageList = lstIcons;
            treeButtons.ImageList = lstIcons;
        }

        public void SetIcons(ImageList icons)
        {
            lstIcons = icons;
            btnPicture.ImageList = lstIcons;
            treeButtons.ImageList = lstIcons;
        }

        public List<ChartToolButtonSettings> GetButtons()
        {
            var buttons = new List<ChartToolButtonSettings>();
            foreach (TreeNode node in treeButtons.Nodes)
            {
                var button = node.Tag as ChartToolButtonSettings;
                if (button != null)
                {
                    buttons.Add(button);
                    continue;
                }
                foreach (TreeNode subnode in node.Nodes)
                {
                    var subButton = subnode.Tag as ChartToolButtonSettings;
                    if (subButton == null)
                        continue;
                    buttons.Add(subButton);
                }
            }
            return buttons;
        }

        public List<ToolButtonGroup> GetGroups()
        {
            var groups = new List<ToolButtonGroup>();
            foreach (TreeNode node in treeButtons.Nodes)
            {
                var group = node.Tag as ToolButtonGroup;
                if (group == null)
                    continue;
                groups.Add(group);
            }
            return groups;
        }

        public void SetButtonsAndGroups(List<ChartToolButtonSettings> buttons, List<ToolButtonGroup> groups, ChartToolButtonSettings.ToolButtonType buttonType)
        {
            this.buttonType = buttonType;
            treeButtons.Nodes.Clear();
            foreach (var group in groups)
            {
                var nodeGroup = new TreeNode(string.Format("[{0}]", group.Title),
                                             group.ImageIndex, group.ImageIndex) { Tag = group };
                treeButtons.Nodes.Add(nodeGroup);
            }
            foreach (var btn in buttons)
            {
                var node = new TreeNode(btn.ToString(), btn.Image, btn.Image) { Tag = btn };
                if (btn.Group != null)
                {
                    var groupTitle = btn.Group.Title;
                    var parent = treeButtons.Nodes.Cast<TreeNode>().First(n => n.Tag is ToolButtonGroup && ((ToolButtonGroup)n.Tag).Title == groupTitle);
                    if(parent != null)
                        parent.Nodes.Add(node);
                    else
                        treeButtons.Nodes.Add(node);
                }
                else
                    treeButtons.Nodes.Add(node);
            }
            ShowSelectedButton(null);
        }

        private void ShowSelectedButton(TreeNode item)
        {
            panelParams.Controls.Clear();
            if (item == null)
            {
                tbButtonDisplayName.Text = "";
                btnPicture.ImageIndex = -1;
                return;
            }

            var group = item.Tag as ToolButtonGroup;
            if (group != null)
            {
                // показать настройки группы кнопок - картинку и заголовок
                tbButtonDisplayName.Text = group.Title;
                btnPicture.ImageIndex = group.ImageIndex;
                isVisibleDisplayNameCheckBox.Enabled = false;
                isVisibleDisplayNameCheckBox.Checked = false;
                return;
            }

            var btnSets = (ChartToolButtonSettings) item.Tag;
            if (btnSets == null)
                return;
            tbButtonDisplayName.Text = btnSets.ToString();
            btnPicture.ImageIndex = btnSets.Image;
            isVisibleDisplayNameCheckBox.Enabled = true;
            isVisibleDisplayNameCheckBox.Checked = btnSets.IsVisibleDisplayName;

            // показать параметры команды
            const int editorHeight = 23;
            foreach (var ptr in btnSets.toolParams)
            {
                var editor = new GenericObjectEditor(ptr.title, ptr.paramType, ptr.defaultValue);
                editor.SetBounds(0, 0, 100, editorHeight);
                editor.Dock = DockStyle.Top;
                panelParams.Controls.Add(editor);
            }
        }

        private void BtnPictureClick(object sender, EventArgs e)
        {
            var dlg = new SelectPictureForm(lstIcons);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            btnPicture.ImageIndex = dlg.ImageIndex;
            /*var item = treeButtons.SelectedNode;
            if (item == null)
                return;
            var group = item.Tag as ToolButtonGroup;
            if (group != null)
            {
                group.ImageIndex = btnPicture.ImageIndex;
                item.ImageIndex = btnPicture.ImageIndex;
                item.SelectedImageIndex = btnPicture.ImageIndex;
                return;
            }
            var btnSets = item.Tag as ChartToolButtonSettings;
            if (btnSets == null)
                return;
            btnSets.Image = btnPicture.ImageIndex;
            item.ImageIndex = btnPicture.ImageIndex;
            item.SelectedImageIndex = btnPicture.ImageIndex;*/
        }

        private void BtnUpdateToolButtonClick(object sender, EventArgs e)
        {
            var node = treeButtons.SelectedNode;
            if (node == null || node.Tag == null)
                return;

            // группа?
            var group = node.Tag as ToolButtonGroup;
            if(group != null)
            {
                // текст
                group.Title = tbButtonDisplayName.Text;
                node.Text = group.Title;
                // картинка
                group.ImageIndex = btnPicture.ImageIndex;
                node.ImageIndex = btnPicture.ImageIndex;
                node.SelectedImageIndex = btnPicture.ImageIndex;
                return;
            }

            // кнопка?
            var button = node.Tag as ChartToolButtonSettings;
            if (button == null)
                return;
            // название
            button.DisplayName = tbButtonDisplayName.Text;
            node.Text = button.ToString();
            button.IsVisibleDisplayName = isVisibleDisplayNameCheckBox.Checked;
            // картинка
            button.Image = btnPicture.ImageIndex;
            node.ImageIndex = btnPicture.ImageIndex;
            node.SelectedImageIndex = btnPicture.ImageIndex;
            // параметры
            foreach (GenericObjectEditor editor in panelParams.Controls)
            {
                var paramTitle = editor.Title;
                button.toolParams.First(ptr => ptr.title == paramTitle).defaultValue = editor.PropertyValue;
            }
        }

        private void TreeButtonsAfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowSelectedButton(treeButtons.SelectedNode);
        }

        private void MenuItemAddButtonClick(object sender, EventArgs e)
        {
            var dialog = new ListSelectDialog();
            dialog.Initialize(ChartToolButtonStorage.Instance.allButtons.Where(b => b.ButtonType == buttonType));
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            var button = dialog.SelectedItem as ChartToolButtonSettings;
            if (button == null)
                return;
            var item = new TreeNode(button.ToString(), button.Image, button.Image);
            treeButtons.Nodes.Add(item);
            item.Tag = new ChartToolButtonSettings(button);
        }

        private void MenuItemAddGroupClick(object sender, EventArgs e)
        {
            // добавить группу кнопок
            DialogResult rst;
            var groupName = Dialogs.ShowInputDialog(Localizer.GetString("TitleGroupName"), "", out rst);
            if (rst != DialogResult.OK || string.IsNullOrEmpty(groupName))
                return;
            var dlg = new SelectPictureForm(lstIcons);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            if (dlg.ImageIndex < 0)
                return;

            // добавить в дерево
            var group = new ToolButtonGroup {ImageIndex = dlg.ImageIndex, Title = groupName, ButtonType = buttonType};
            var nodeGroup = new TreeNode(string.Format("[{0}]", group.Title),
                                         group.ImageIndex, group.ImageIndex) {Tag = group};
            treeButtons.Nodes.Add(nodeGroup);
        }

        private void MenuItemRemoveButtonClick(object sender, EventArgs e)
        {
            if (treeButtons.SelectedNode == null)
                return;
            treeButtons.Nodes.Remove(treeButtons.SelectedNode);
        }

        private void IsVisibleDisplayNameCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            /*var item = treeButtons.SelectedNode;
            if (item == null)
                return;
            var btnSets = item.Tag as ChartToolButtonSettings;
            if (btnSets == null)
                return;
            btnSets.IsVisibleDisplayName = isVisibleDisplayNameCheckBox.Checked;*/
        }
    }
}
