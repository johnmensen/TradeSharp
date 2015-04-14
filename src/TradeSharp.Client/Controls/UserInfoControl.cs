using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.Util;
using TradeSharp.Util.Controls;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Controls
{
    public partial class UserInfoControl : UserControl
    {
        private readonly string buttonTitleRegisterSignalService;

        private readonly string buttonTitleDeregisterSignalService;

        private readonly string buttonTitleRegisterPammService;

        private readonly string buttonTitleDeregisterPammService;

        public string Password
        {
            get { return tbPassword.Text; }
        }

        public string Login
        {
            get { return tbLogin.Text; }
        }

        public bool Registred { get; private set; }

        private const bool EnablePasswordOnRegistration = true;

        public enum UserControlMode { New = 0, Edit = 1 }

        private UserControlMode mode;

        private Account editedAccount;

        private PlatformUser editedUser;

        private string oldPassword;

        private Action<bool> onRegistred;

        public event Action<bool> OnRegistred
        {
            add { onRegistred += value; }
            remove { onRegistred -= value; }
        }

        private readonly BackgroundWorker workerLoadManagerServices = new BackgroundWorker();

        public UserInfoControl()
        {
            InitializeComponent();

            buttonTitleRegisterSignalService = Localizer.GetString("TitleProvideTradeSignals");
            buttonTitleDeregisterSignalService = Localizer.GetString("TitleProvideTradeSignals");
            buttonTitleRegisterPammService = Localizer.GetString("TitleOpenPAMMAccount");
            buttonTitleDeregisterPammService = Localizer.GetString("TitleTurnOffPAMMAccount");
            workerLoadManagerServices.DoWork += LoadManagerServicesDataSync;
            workerLoadManagerServices.RunWorkerCompleted += WorkerLoadManagerServicesOnRunWorkerCompleted;
            SetupGrids();
        }

        private void SetupGrids()
        {
            var blank = new PaidService();
            // торговые сигналы
            gridServiceSignal.Columns.Add(new FastColumn(blank.Property(s => s.AccountId),
                                                         Localizer.GetString("TitleAccount"))
                {
                    formatter = value => Localizer.GetString("TitleAccountNumber") + value.ToString(),
                    SortOrder = FastColumnSort.Ascending
                });
            gridServiceSignal.Columns.Add(new FastColumn(blank.Property(s => s.FixedPrice),
                                                         Localizer.GetString("TitlePerDay"))
                {
                    rowFormatter = valueObject =>
                        {
                            var srv = (PaidService) valueObject;
                            return srv.FixedPrice.ToStringUniformMoneyFormat() + " " + srv.Currency;
                        }
                });
            gridServiceSignal.Columns.Add(new FastColumn(blank.Property(s => s.FixedPriceMonth),
                                                         Localizer.GetString("TitleInMonth"))
                {
                    rowFormatter = valueObject =>
                        {
                            var srv = (PaidService) valueObject;
                            return srv.FixedPriceMonth.ToStringUniformMoneyFormat() + " " + srv.Currency;
                        }
                });
            
            gridServiceSignal.CalcSetTableMinWidth();

            // ПАММ
            gridServicePAMM.Columns.Add(new FastColumn(blank.Property(s => s.AccountId), Localizer.GetString("TitleAccount"))
            {
                formatter = value => Localizer.GetString("TitleAccountNumber") + value.ToString(),
                SortOrder = FastColumnSort.Ascending
            });
            gridServicePAMM.Columns.Add(new FastColumn(blank.Property(s => s.FixedPrice), Localizer.GetString("TitleReward"))
            {
                rowFormatter = valueObject =>
                {
                    var srv = (PaidService)valueObject;
                    if (srv.serviceRates.Count == 0) return "-";
                    return string.Join(", ", 
                        srv.serviceRates.Select(s => string.Format("{0}{1}%",
                            s.UserBalance == 0 ? "" : (Localizer.GetString("TitleFromSmall") + " " + s.UserBalance.ToStringUniformMoneyFormat() + " - "), s.Amount)));
                }
            });

            gridServicePAMM.CalcSetTableMinWidth();
        }

        public void SetMode(UserControlMode mode)
        {
            this.mode = mode;
            // скрыть определенные поля в определенном режиме
            if (mode == UserControlMode.Edit)
            {
                cbDepoSize.Enabled = false;
                cbCurrency.Visible = false;
                lblCurreny.Visible = false;
                btnComplete.Text = Localizer.GetString("TitleEdit");
                tbPassword.Visible = true;
                lblPassword.Visible = true;
                additionalTabPage.Enabled = true;
                try
                {

                    var metadataSettings = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("UserInfoEx");
                    if (metadataSettings != null)
                    {
                        var bigSize = (int)metadataSettings["BigAvatarMaxSize"];
                        bigAvatarPanel.Width = bigSize;
                        bigAvatarPanel.Height = bigSize;
                        var smallSize = (int)metadataSettings["SmallAvatarMaxSize"];
                        smallAvatarPanel.Width = smallSize;
                        smallAvatarPanel.Height = smallSize;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info("UserInfoControl.SetMode: server error", ex);
                }
                tabPageService.Enabled = true;
            }
            else
            {
                // режим открытия счета
                tbPassword.Visible = EnablePasswordOnRegistration;
                lblPassword.Visible = EnablePasswordOnRegistration;
                additionalTabPage.Enabled = false;

                tabPageService.Enabled = false;
            }
        }

        private void UserInfoControlLoad(object sender, EventArgs e)
        {
            if (mode == UserControlMode.Edit) LoadData();
            cbCurrency.Items.Add("USD");
            cbCurrency.SelectedIndex = 0;
        }

        private void LoadData()
        {
            // загрузить данные о пользователе и счете
            editedAccount = AccountStatus.Instance.AccountData;
            if (editedAccount == null) return;
            // валюта...
            var indexCurx = cbCurrency.Items.IndexOf(editedAccount.Currency);
            if (indexCurx < 0) indexCurx = cbCurrency.Items.Add(editedAccount.Currency);
            cbCurrency.SelectedIndex = indexCurx;
            // баланс...
            cbDepoSize.Text = editedAccount.Equity.ToStringUniformMoneyFormat(false);
            // макс плечо...
            tbMaxLeverage.Text = editedAccount.MaxLeverage.ToStringUniform();
            
            // получить данные о пользователе...
            var login = AccountStatus.Instance.Login;
            if (string.IsNullOrEmpty(login)) return;

            var password = UserSettings.Instance.GetPasswordForLogin(login);
            oldPassword = password;
            var passwordPrompt = Localizer.GetString("MessageEnterOldPassword");
            
            while (true)
            {
                if (string.IsNullOrEmpty(password))
                {
                    DialogResult rst;
                    password = Dialogs.ShowInputDialog(passwordPrompt, "", true, out rst);
                    if (rst == DialogResult.Cancel) return;
                    oldPassword = password;
                }

                var resp = MainForm.serverProxyTrade.proxy.GetUserDetail(login, password, out editedUser);
                if (resp == AuthenticationResponse.AccountInactive
                    || resp == AuthenticationResponse.InvalidAccount
                    || resp == AuthenticationResponse.ServerError)
                {
                    MessageBox.Show(EnumFriendlyName<AuthenticationResponse>.GetString(resp));
                    return;
                }
                
                if (resp == AuthenticationResponse.WrongPassword)
                {
                    password = string.Empty;
                    passwordPrompt = Localizer.GetString("MessageWrongPwrdEnterAgain");
                    continue;
                }
                if (editedUser != null) break;
            }
            
            // показать данные пользователя
            tbEmail.Text = editedUser.Email;
            tbLogin.Text = editedUser.Login;
            tbPatronym.Text = editedUser.Patronym;
            tbName.Text = editedUser.Name;
            tbSurname.Text = editedUser.Surname;
            tbPhone1.Text = editedUser.Phone1;
            tbPhone2.Text = editedUser.Phone2;
            tbDescription.Text = editedUser.Description;
            tbPassword.Text = editedUser.Password;
            ShowSignallerOptions(editedUser);

            // получить дополнительные данные о пользователе
            try
            {
                var userInfoExSource =
                    new UserInfoExCache(TradeSharpAccountStatistics.Instance.proxy, TerminalEnvironment.FileCacheFolder);
                var info = userInfoExSource.GetUserInfo(editedUser.ID);
                if (info != null)
                {
                    bigAvatarPanel.BackgroundImage = info.AvatarBig;
                    smallAvatarPanel.BackgroundImage = info.AvatarSmall;
                    aboutRichTextBox.Text = info.About;
                    ContactListUtils.UnpackContacts(info.Contacts, contactsListView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localizer.GetString("TitleServerError"),
                    Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Info("Ошбика при чтении дополнительной информации о пользователе", ex);
            }
        }

        private void ShowSignallerOptions(PlatformUser user)
        {
            workerLoadManagerServices.RunWorkerAsync(user);
            SwitchLoadingUserInfoProgressBar(true);
        }

        private void SwitchLoadingUserInfoProgressBar(bool isShown)
        {
            progressBarManager.Visible = isShown;
            timerManagerSignalsLoadingProcess.Enabled = isShown;
        }

        private void BtnCompleteClick(object sender, EventArgs e)
        {
            // проверить введенные данные
            if (!CheckData()) return;

            // зарегить счет и пользователя?
            if (mode == UserControlMode.New)
                RegisterNewAccount();
            else 
            // редактировать существующие?
            EditExisting();
        }

        private void EditExisting()
        {
            var login = AccountStatus.Instance.Login;
            if (string.IsNullOrEmpty(login)) return;
            if (string.IsNullOrEmpty(oldPassword)) return;
            if (editedUser == null) return;
            if (editedAccount == null) return;

            // собрать данные
            var user = new PlatformUser
            {
                Email = tbEmail.Text,
                Login = tbLogin.Text,
                Name = tbName.Text,
                Surname = tbSurname.Text,
                Patronym = tbPatronym.Text,
                Phone1 = tbPhone1.Text,
                Phone2 = tbPhone2.Text,
                Description = tbDescription.Text,
                Password = tbPassword.Text
            };
            var maxLeverage = tbMaxLeverage.Text.ToFloatUniform();

            bool loginIsBusy;
            var resp = MainForm.serverProxyTrade.proxy.ModifyUserAndAccount(login, oldPassword,
                                                                              user, editedAccount.ID, maxLeverage, out loginIsBusy);
            if (loginIsBusy)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageLoginInUse"), user.Login));
                return;
            }

            if (resp == AuthenticationResponse.AccountInactive ||
                resp == AuthenticationResponse.InvalidAccount ||
                resp == AuthenticationResponse.WrongPassword ||
                resp == AuthenticationResponse.ServerError)
            {
                var msg = Localizer.GetString("MessageUnableToUpdate") + " - " +
                    EnumFriendlyName<AuthenticationResponse>.GetString(resp);
                MessageBox.Show(msg, Localizer.GetString("TitleError"));
            }

            // обновить дополнительные данные о пользователе
            try
            {
                var userInfoExSource =
                    new UserInfoExCache(TradeSharpAccountStatistics.Instance.proxy, TerminalEnvironment.FileCacheFolder);
                var info = new UserInfoEx
                    {
                        Id = editedUser.ID,
                        AvatarBig = bigAvatarPanel.BackgroundImage as Bitmap,
                        AvatarSmall = smallAvatarPanel.BackgroundImage as Bitmap,
                        About = aboutRichTextBox.Text,
                        Contacts = ContactListUtils.PackContacts(contactsListView)
                    };
                userInfoExSource.SetUserInfo(info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localizer.GetString("TitleServerError"),
                    Localizer.GetString("TitleError"));
                Logger.Info("Ошбика при записи дополнительной информации о пользователе", ex);
                return;
            }
            MessageBox.Show(Localizer.GetString("MessageDataSuccessfulyUpdated"),
                Localizer.GetString("TitleConfirmation"),
                MessageBoxButtons.OK, 
                MessageBoxIcon.Asterisk);
        }

        private void RegisterNewAccount()
        {
            var user = new PlatformUser
            {
                Email = tbEmail.Text,
                Login = tbLogin.Text,
                Name = tbName.Text,
                Surname = tbSurname.Text,
                Patronym = tbPatronym.Text,
                Phone1 = tbPhone1.Text,
                Phone2 = tbPhone2.Text,
                Description = tbDescription.Text
            };

            // передать запрос на регистрацию на сервер
            var depoSz = cbDepoSize.Text.Replace(" ", "").ToInt();
            var requestError = string.Empty;
            try
            {
                var status = TradeSharpAccount.Instance.proxy.RegisterAccount(user, cbCurrency.Text, depoSz, 
                    tbMaxLeverage.Text.ToDecimalUniformSafe() ?? 0,
                    EnablePasswordOnRegistration ? tbPassword.Text : string.Empty, AppConfig.GetBooleanParam("AutoSignIn", false));
                if (status != AccountRegistrationStatus.OK)
                    requestError = EnumFriendlyName<AccountRegistrationStatus>.GetString(status);
            }
            catch (Exception ex)
            {
                requestError = Localizer.GetString("TitleServerError");
                Logger.Error("Ошибка в RegisterAccount", ex);
            }

            if (!string.IsNullOrEmpty(requestError))
            {
                MessageBox.Show(requestError, 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (onRegistred != null) onRegistred(false);
                return;
            }

            MainForm.Instance.AddMessageToStatusPanelSafe(DateTime.Now,
                string.Format(
                    Localizer.GetString("MessageUserRegisteredFmt"), 
                    tbLogin.Text,
                    EnablePasswordOnRegistration ? ", " + Localizer.GetString("MessageUserRegisteredPasswordPart") + " " + tbPassword.Text : ""));

            var msg = EnablePasswordOnRegistration
                ? Localizer.GetString("MessageRegistrationCompleted")
                : string.Format(Localizer.GetString("MessageRegistrationCompletedEmailFmt"),
                    Environment.NewLine, tbEmail.Text);
            MessageBox.Show(msg, 
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            Registred = true;
            if (onRegistred != null) onRegistred(true);
        }

        private bool CheckData()
        {
            var errors = new StringBuilder();
            // проверить заполнение полей           
            if (string.IsNullOrEmpty(tbEmail.Text))
                errors.AppendLine(Localizer.GetString("MessageEmailErrorEmpty"));
            else
                try
                {
                    new MailAddress(tbEmail.Text);
                }
                catch (FormatException)
                {
                    errors.AppendLine(Localizer.GetString("MessageEmailErrorWrongFormat"));
                }
            var login = tbLogin.Text;
            if (login.Length < PlatformUser.LoginLenMin)
                errors.AppendLine(string.Format(Localizer.GetString("MessageUserNameErrorShortFmt"), PlatformUser.LoginLenMin));
            else
                if (login.Length > PlatformUser.LoginLenMax)
                    errors.AppendLine(string.Format(Localizer.GetString("MessageUserNameErrorLongFmt"),
                        PlatformUser.LoginLenMax));

            if (!PlatformUser.CheckLoginSpelling(login))
                errors.AppendLine(Localizer.GetString("MessageUserNameErrorBadSymbols"));

            if (mode == UserControlMode.New)
            {
                // проверить размер депо
                var depoSz = cbDepoSize.Text.Replace(" ", "").ToIntSafe();
                if (!depoSz.HasValue)
                    errors.AppendLine(Localizer.GetString("MessageDepoVolumeMustBeInt"));

                var depoVolume = depoSz ?? 1000;
                if (depoVolume < 1000)
                    errors.AppendLine(Localizer.GetString("MessageDepoVolumeMustBe1000"));
                else if (depoVolume > 5000000)
                    errors.AppendLine(Localizer.GetString("MessageDepoVolumeMustBe5000"));
            }

            if (mode == UserControlMode.Edit || EnablePasswordOnRegistration)
            {
                // проверить пароль
                if (tbPassword.Text.Length < 8)
                    errors.AppendLine(
                        string.Format(Localizer.GetString("MessagePasswordTooShortFmt"), 8));
                else if (tbPassword.Text.Length > 25)
                    errors.AppendLine(string.Format(Localizer.GetString("MessagePasswordTooLongFmt"), 25));
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            // проверить капчу
            var dlg = new CaptchaDialog(Localizer.GetString("MessageEnterConfirmationCode"), 
                true, false, 4);
            if (dlg.ShowDialog() != DialogResult.OK) return false;
            return true;
        }

        private Bitmap SelectBitmapFromFile()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return null;
            try
            {
                var stream = dialog.OpenFile();
                using (stream)
                {
                    return new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Localizer.GetString("MessageErrorReading") + 
                    " " + dialog.FileName + "\n" + ex, 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return null;
            }
        }
        
        private Bitmap FitBitmap(Bitmap bmp, Size size)
        {
            if (bmp.Width > size.Width || bmp.Height > size.Height)
            {
                // пропорционально уменьшаем изображение
                var scaleX = size.Width / (bmp.Width + 0.0);
                var scaleY = size.Height / (bmp.Height + 0.0);
                var scale = Math.Min(scaleX, scaleY);
                return new Bitmap(bmp, (int)(bmp.Width * scale), (int)(bmp.Height * scale));
            }
            return bmp;
        }

        private void SelectBigAvatarButtonClick(object sender, EventArgs e)
        {
            var bmp = SelectBitmapFromFile();
            if (bmp == null)
                return;
            bigAvatarPanel.BackgroundImage = FitBitmap(bmp, bigAvatarPanel.Size);
            if (smallAvatarPanel.BackgroundImage == null)
                smallAvatarPanel.BackgroundImage = FitBitmap(bmp, smallAvatarPanel.Size);
        }

        private void SelectSmallAvatarButtonClick(object sender, EventArgs e)
        {
            var bmp = SelectBitmapFromFile();
            if (bmp == null)
                return;
            smallAvatarPanel.BackgroundImage = FitBitmap(bmp, smallAvatarPanel.Size);
            if (bigAvatarPanel.BackgroundImage == null)
                bigAvatarPanel.BackgroundImage = FitBitmap(bmp, bigAvatarPanel.Size);
        }

        private void ClearBigAvatarButtonClick(object sender, EventArgs e)
        {
            bigAvatarPanel.BackgroundImage = null;
        }

        private void ClearSmallAvatarButtonClick(object sender, EventArgs e)
        {
            smallAvatarPanel.BackgroundImage = null;
        }

        private void AddContactButtonClick(object sender, EventArgs e)
        {
            var form = new ContactInputForm();
            var items = new List<object>
                {
                    new DropDownItem("Facebook", "facebook"),
                    new DropDownItem("Google+", "googleplus"),
                    new DropDownItem("Email", "email"),
                    new DropDownItem("MailRu", "mailru"),
                    new DropDownItem(Localizer.GetString("TitleOdnoklassniki"), "odnoklassniki"),
                    new DropDownItem("Twitter", "twitter"),
                    new DropDownItem(Localizer.GetString("TitleVKontakte"), "vk"),
                    new DropDownItem("Skype", "skype")
                };
            var blank = new DropDownItem(string.Empty, string.Empty);
            form.SetItems(items, blank.Property(p => p.Text), blank.Property(p => p.ImageKey), socialNetworksImageList);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            var selectedItem = (DropDownItem) form.SelectedItem;
            if (selectedItem == null)
                return;
            contactsListView.Items.Add(new ListViewItem(form.InputText, selectedItem.ImageKey));
        }

        private void DeleteContactButtonClick(object sender, EventArgs e)
        {
            var items = contactsListView.SelectedItems;
            foreach (ListViewItem item in items)
                contactsListView.Items.Remove(item);
        }

        private void ContactsListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            deleteContactButton.Enabled = contactsListView.SelectedItems.Count != 0;
        }

        private void LoadManagerServicesDataSync(object sender, DoWorkEventArgs ea)
        {
            var user = (PlatformUser) ea.Argument;
            Wallet userWallet = null;
            List<PaidService> srvList = null;
            try
            {
                srvList = TradeSharpWalletManager.Instance.proxy.GetUserOwnerPaidServices(user.ID);
                userWallet = TradeSharpWalletManager.Instance.proxy.GetUserWallet(
                    CurrentProtectedContext.Instance.MakeProtectedContext(), user.Login);
            }
            catch (Exception ex)
            {
                Logger.Error("ShowSignallerOptions.GetUserOwnerPaidServices()", ex);
            }

            ea.Result = new Cortege2<Wallet, List<PaidService>>(userWallet, srvList);
        }

        private void WorkerLoadManagerServicesOnRunWorkerCompleted(object sender, 
            RunWorkerCompletedEventArgs ea)
        {
            SwitchLoadingUserInfoProgressBar(false);

            var resultTyped = (Cortege2<Wallet, List<PaidService>>) ea.Result;
            var srvList = resultTyped.b;
            var userWallet = resultTyped.a;

            if (srvList == null || /*srvList.Count == 0 || */userWallet == null) return;

            // показать данные по подписке
            gridServiceSignal.rows.Clear();
            var signalServices = srvList.Where(s => s.ServiceType == PaidServiceType.Signals).ToList();
            gridServiceSignal.DataBind(signalServices);
            btnMakeSignalService.Text = signalServices.Count > 0 
                ? buttonTitleDeregisterSignalService : buttonTitleRegisterSignalService;

            // ... включая ПАММ
            var pammServices = srvList.Where(s => s.ServiceType == PaidServiceType.PAMM).ToList();
            gridServicePAMM.DataBind(pammServices);
            btnMakePAMM.Text = pammServices.Count > 0
                                   ? buttonTitleDeregisterPammService
                                   : buttonTitleRegisterPammService;
        }

        private void TimerManagerSignalsLoadingProcessTick(object sender, EventArgs e)
        {
            var val = progressBarManager.Value + progressBarManager.Step;
            if (val > progressBarManager.Maximum) val = 0;
            progressBarManager.Value = val;
        }

        /// <summary>
        /// выставить услугу - торговые сигналы
        /// </summary>
        private void BtnMakeSignalServiceClick(object sender, EventArgs e)
        {
            // отключить сигнальный сервис
            if (gridServiceSignal.rows.Count > 0)
            {
                var service = (PaidService) gridServiceSignal.rows[0].ValueObject;
                if (MessageBox.Show(Localizer.GetString("MessageTurnOffSignalService") + "?",
                    Localizer.GetString("TitleConfirmation"), 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
                DeregisterTradeSignals(service);
                return;
            }

            // зарегистрировать сервис сигналов?
            if (new NewTradeSignalForm().ShowDialog() == DialogResult.OK)
                ShowSignallerOptions(editedUser);
        }

        /// <summary>
        /// отключить услугу
        /// </summary>
        private void DeregisterTradeSignals(PaidService service)
        {
            if (service == null)
                return;
            
            // отписаться от услуги
            WalletError error;
            try
            {
                TradeSharpWalletManager.Instance.proxy.DisableService(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    service.Id, out error);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("TradeSharpWalletManager.DisableService({0}) error: {1}", service.Id, ex);
                error = WalletError.CommonError;
            }

            if (error == WalletError.OK)
            {
                MessageBox.Show(Localizer.GetString("MessageServiceIsTurnedOff"), 
                    Localizer.GetString("TitleConfirmation"),
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                ShowSignallerOptions(editedUser);
            }
            else
                MessageBox.Show(EnumFriendlyName<WalletError>.GetString(error),
                    Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnMakePAMMClick(object sender, EventArgs e)
        {
            // включить ПАММ
            if (gridServicePAMM.rows.Count == 0)
            {
                // выбрать реальный счет пользователя
                var accountSelected = UserServiceRegistrator.SelectTerminalUserAccount(true);
                if (accountSelected == null) return;
                var dlg = new OpenPAMMForm(accountSelected.Currency);
                if (dlg.ShowDialog() == DialogResult.Cancel) return;
                if (!dlg.EnablePAMM) return;
                var service = new PaidService
                    {
                        AccountId = accountSelected.ID,
                        Currency = accountSelected.Currency,
                        ServiceType = PaidServiceType.PAMM,
                        serviceRates = dlg.ServiceRates,
                        Comment = Localizer.GetString("TitlePAMMAccount") + " #" + accountSelected.ID
                    };
                if (!UserServiceRegistrator.RegisterOrUpdateService(service))
                    return;
                ShowSignallerOptions(editedUser);
                return;
            }

            // отключить ПАММ-сервис
            var servicePAMM = (PaidService) gridServicePAMM.rows[0].ValueObject;
            DeregisterTradeSignals(servicePAMM);
        }
    }
}
