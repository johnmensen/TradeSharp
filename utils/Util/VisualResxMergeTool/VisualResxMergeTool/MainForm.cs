using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FastGrid;
using VisualResxMergeTool.BL;

namespace VisualResxMergeTool
{
    public partial class MainForm : Form
    {
        class FileWithNum
        {
            public string fileName;

            public int? number;
        }

        private ChangeSet changeSet;
        private string origName;
        private string theirsName;

        public MainForm()
        {
            InitializeComponent();
            SetupGrid();
        }

        private void SetupGrid()
        {
            //var node = new ModifiedNode();
            gridModified.Columns.Add(new FastColumn("Key", "Ключ")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 40
                });
            gridModified.Columns.Add(new FastColumn("ValueOwn", "MINE"));
            gridModified.Columns.Add(new FastColumn("ValueTheirs", "THEIRS"));
            gridModified.rowExtraFormatter = delegate(object valueObject, List<FastColumn> columns)
                {
                    var nod = (ModifiedNode) valueObject;
                    if (nod.UseFrom == ChangeSource.Mine)
                        return new[] {nod.Key, "[v] " + nod.ValueOwn, nod.ValueTheirs};
                    return new[] {nod.Key, nod.ValueOwn, "[v] " + nod.ValueTheirs};
                };
            gridModified.CalcSetTableMinWidth();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            if (!File.Exists(openFileDialog.FileName)) return;

            var fileName = openFileDialog.FileName.Replace(".mine", "").Replace(".theirs", "");

            origName = fileName + ".mine";
            if (!File.Exists(origName))
            {
                MessageBox.Show("Не найден файл MINE");
                return;
            }

            theirsName = GetTheirsFileName();
            if (string.IsNullOrEmpty(theirsName) || !File.Exists(theirsName))
            {
                MessageBox.Show("Не найден файл THEIRS");
                return;
            }

            CompareFiles();
        }

        private string GetTheirsFileName()
        {
            var dir = Path.GetDirectoryName(origName);
            var nameOnly = Path.GetFileNameWithoutExtension(origName).Replace(".mine", "");

            var files = Directory.GetFiles(dir, nameOnly + ".r*");
            if (files.Length == 0)
                return string.Empty;

            var fileWithNum = files.Select(f =>
                {
                    var fn = new FileWithNum {fileName = f};
                    var ext = Path.GetExtension(f).Replace(".r", "");
                    int revNum;
                    if (int.TryParse(ext, out revNum))
                        fn.number = revNum;
                    return fn;
                }).Where(f => f.number.HasValue).OrderByDescending(f => f.number).FirstOrDefault();
            return fileWithNum == null ? "" : fileWithNum.fileName;
        }

        private void CompareFiles()
        {
            var docOrig = new XmlDocument();
            docOrig.Load(new StreamReader(origName, Encoding.UTF8));

            var docTheirs = new XmlDocument();
            docTheirs.Load(new StreamReader(theirsName, Encoding.UTF8));

            changeSet = GetChangeSet(docOrig, docTheirs);
            labelDeltaNodes.Text = changeSet.added.Count.ToString() + " добавилось, " +
                                   changeSet.deleted.Count.ToString() + " удалено";

            gridModified.DataBind(changeSet.modified);
            gridModified.UserHitCell += GridModifiedOnUserHitCell;
            gridModified.SelectEnabled = false;
            gridModified.Invalidate();
        }

        private void GridModifiedOnUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            if (mouseEventArgs.Button != MouseButtons.Left)
                return;

            var item = (ModifiedNode) gridModified.rows[rowIndex].ValueObject;
            item.UseFrom = item.UseFrom == ChangeSource.Mine ? ChangeSource.Theirs : ChangeSource.Mine;
            gridModified.UpdateRow(rowIndex, item);
            gridModified.InvalidateRow(rowIndex);
        }

        private ChangeSet GetChangeSet(XmlDocument docOrig, XmlDocument docTheirs)
        {
            var set = new ChangeSet();

            var dataNodesOrig = docOrig.GetElementsByTagName("data").Cast<XmlElement>().Select(
                n => new ResNode
                    {
                        Name = n.Attributes["name"].Value,
                        Value = n.ChildNodes[1].InnerText
                    }).ToList();
            var dataNodesTheirs = docTheirs.GetElementsByTagName("data").Cast<XmlElement>().Select(
                n => new ResNode
                    {
                        Name = n.Attributes["name"].Value,
                        Value = n.ChildNodes[1].InnerText
                    }).ToList();

            // удаленные и измененные ...
            foreach (var node in dataNodesOrig)
            {
                var nodeName = node.Name;
                var sameNode = dataNodesTheirs.FirstOrDefault(n => n.Name == nodeName);
                if (sameNode == null)
                {
                    set.deleted.Add(node);
                    continue;
                }
                if (sameNode.Value == node.Value)
                    continue;
                set.modified.Add(new ModifiedNode
                    {
                        Key = node.Name,
                        ValueOwn = node.Value,
                        ValueTheirs = sameNode.Value
                    });
            }

            // добавленные...
            foreach (var node in dataNodesTheirs)
            {
                var nodeName = node.Name;
                var hasSameNode = dataNodesOrig.Any(n => n.Name == nodeName);
                if (!hasSameNode)
                    set.added.Add(node);
            }

            return set;
        }

        private void btnMakeResult_Click(object sender, EventArgs e)
        {
            if (changeSet == null) return;

            var docOrig = new XmlDocument();
            docOrig.Load(new StreamReader(origName, Encoding.UTF8));

            // удалить из MINE те, что отсутствуют в THEIRS
            if (cbDeleteFromMine.Checked)
            {
                foreach (var node in changeSet.deleted)
                {
                    var childNode = docOrig.SelectSingleNode("/root/data[@name='" +
                        node.Name + "']");
                    childNode.ParentNode.RemoveChild(childNode);
                }
            }

            // добавить в MINE из THEIRS
            if (cbAddFromTheirs.Checked)
            {
                foreach (var node in changeSet.added)
                {
                    var newNode = (XmlElement) docOrig.DocumentElement.AppendChild(docOrig.CreateElement("data"));
                    newNode.Attributes.Append(docOrig.CreateAttribute("name")).Value = node.Name;
                    newNode.Attributes.Append(docOrig.CreateAttribute("xml:space")).Value = "preserve";
                    var child = (XmlElement) newNode.AppendChild(docOrig.CreateElement("value"));
                    child.InnerText = node.Value;
                }
            }

            // модифицировать
            var modItems =
                gridModified.GetRowValues<ModifiedNode>(false).Where(r => r.UseFrom == ChangeSource.Theirs).ToList();
            foreach (var item in modItems)
            {
                var childNode = docOrig.SelectSingleNode("/root/data[@name='" +
                        item.Key + "']");
                childNode.ChildNodes[1].InnerText = item.ValueTheirs;
            }

            // сохранить
            var fileNameResult = origName + ".result";
            using (var sw = new StreamWriter(fileNameResult, false, Encoding.UTF8))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    docOrig.Save(xw);
                }
            }

            // заменить .resx на .resx.old, а .mine.result на .resx
            var targetFileName = origName.Replace(".mine", "");
            try
            {
                if (File.Exists(targetFileName))
                    File.Move(targetFileName, targetFileName + ".old");
                File.Move(fileNameResult, targetFileName);
            }
            catch
            {
                MessageBox.Show("Невозможно переименовать файл " + fileNameResult + " в " + targetFileName);
                return;
            }
            MessageBox.Show("Файл " + targetFileName + " обновлен");
        }
    }
}
