using System;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class HotKeySetForm : Form
    {
        /// <summary>
        /// Ссылка на объект, который привязан к строке, по которой был клик.
        /// </summary>
        private ApplicationMessageBinding ObjHotKey { get; set; }
        private Keys? curentMod1Key;
        private Keys? curentMod2Key;
        private readonly MainForm mainForm;

        public HotKeySetForm(ApplicationMessageBinding objHotKey)
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            ObjHotKey = objHotKey;
            txtActionName.Text = EnumFriendlyName<ApplicationMessage>.GetString(ObjHotKey.Message);
            txtbxHotKeyNewValue.Text = ObjHotKey.Key;

            mainForm = Application.OpenForms[0] as MainForm;
            if (mainForm != null)
                mainForm.KeyPressedEvent += MainFormKeyPressedEvent;
        }

        /// <summary>
        /// перехват нажимания кнопок
        /// </summary>
        /// <param name="key">Нажатая кнопка</param>
        void MainFormKeyPressedEvent(Keys key)
        {
            var mod = Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(x => ModifierKeys.ToString().Split(',').Select(y => y.Trim()).Contains(x.ToString())).ToArray();
            curentMod1Key = mod.Length > 0 ? mod[0] : Keys.None;
            curentMod2Key = mod.Length > 1 ? mod[1] : Keys.None;


            if (key != Keys.ControlKey && key != Keys.ShiftKey)
            {
                ObjHotKey.mainKey = key;
                ObjHotKey.mod1 = curentMod1Key;
                ObjHotKey.mod2 = curentMod2Key;


                var sb = new StringBuilder(key.ToString());
                if (curentMod1Key != Keys.None && curentMod1Key != null)
                {
                    sb.Append(string.Format(" + {0}", curentMod1Key));
                    ObjHotKey.mod1 = curentMod1Key;
                }
                if (curentMod2Key != Keys.None && curentMod2Key != null)
                {
                    sb.Append(string.Format(" + {0}", curentMod2Key));
                    ObjHotKey.mod2 = curentMod2Key;
                }

                txtbxHotKeyNewValue.Text = sb.ToString();
            }
        }     

        private void BtnOkClick(object sender, System.EventArgs e)
        {                 
            DialogResult = DialogResult.OK;
        }

        private void HotKeySetFormFormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.KeyPressedEvent -= MainFormKeyPressedEvent;
        }
    }
}
