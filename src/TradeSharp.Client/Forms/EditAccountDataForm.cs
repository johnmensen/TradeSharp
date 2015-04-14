using System.Windows.Forms;
using TradeSharp.Client.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class EditAccountDataForm : Form
    {
        public Cortege2<string, string>? AuthData
        {
            get
            {
                return userInfoControl.Registred
                           ? new Cortege2<string, string>(userInfoControl.Login,
                                                          userInfoControl.Password)
                           : (Cortege2<string, string>?)null;
            }
        }

        public EditAccountDataForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            userInfoControl.OnRegistred += b => { if (b) Close(); };
        }

        public void SetMode(bool newMode)
        {
            userInfoControl.SetMode(newMode ? UserInfoControl.UserControlMode.New : UserInfoControl.UserControlMode.Edit);
        }
    }
}
