using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class RiskSetupForm : Form, IMdiNonChartWindow
    {
        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.RiskForm; }
        }

        public int WindowInnerTabPageIndex
        {
            get
            {
                return (int) Invoke(new Func<object>(() => tabControl.SelectedIndex));
            }
            set
            {
                if (value < 0 || value >= tabControl.TabPages.Count)
                    return;
                tabControl.SelectedIndex = value;
            }
        }

        private float currentLeverage;

        public event Action<Form> FormMoved;
        
        public event Action<Form> ResizeEnded;

        public RiskSetupForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private bool dontReactOnFieldsUpdating;

        private bool SettingsAreChanged
        {
            set
            {
                if (dontReactOnFieldsUpdating)
                    return;
                btnAccept.Enabled = value;
                btnCancel.Enabled = value;
            }
        }
      
        #region Вкладка Сообщение

        private void CalculateCurrentLeverage()
        {
            if (!AccountStatus.Instance.isAuthorized) return;
            try
            {
                var orders = MarketOrdersStorage.Instance.MarketOrders;
                if (orders == null || orders.Count == 0) return;
                var account = AccountStatus.Instance.AccountData;
                if (account == null) return;

                var lstPos = PositionSummary.GetPositionSummary(orders, account.Currency, (float)account.Balance);
                currentLeverage = lstPos.Count == 0 ? 0 : lstPos.First(p => string.IsNullOrEmpty(p.Symbol)).Leverage;
                tbCurLeverage.Text = currentLeverage.ToString("f3");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CalculateCurrentLeverage()", ex);
            }
        }

        private void UpdateRiskPicture()
        {
            //if (picLeverage.Image == null)
            picLeverage.Image = new Bitmap(picLeverage.Width, picLeverage.Height);
            using (var g = Graphics.FromImage(picLeverage.Image))
            using (var br = new SolidBrush(SystemColors.ButtonFace))
                g.FillRectangle(br, 0, 0, picLeverage.Width, picLeverage.Height);            

            const int padLeft = 45, padRight = 45, padTop = 24, padBotm = 42;
            var wd = picLeverage.Width;
            var ht = picLeverage.Height;
            var cliWd = wd - padLeft - padRight;
            var cliHt = ht - padTop - padBotm;

            var levWarn = tbRiskWarning.Text.ToFloatUniformSafe() ?? 5;
            var levCrit = tbRiskCritical.Text.ToFloatUniformSafe() ?? 10;

            if (levCrit < levWarn)
            {
                var lev = levWarn;
                levWarn = levCrit;
                levCrit = lev;
            }

            var riskMax = currentLeverage > levCrit ? currentLeverage : (levCrit + (levCrit - levWarn)*0.5f);
            var scaleX = cliWd / riskMax;

            var wds = new [] { levWarn*scaleX, levCrit*scaleX, riskMax*scaleX };
            
            using (var g = Graphics.FromImage(picLeverage.Image))
            using (var brushWarn = new SolidBrush(Color.FromArgb(40, 235, 40)))
            using (var brushCrit = new SolidBrush(Color.FromArgb(215, 205, 10)))
            using (var brushCritText = new SolidBrush(Color.FromArgb(195, 185, 10)))
            using (var brushOver = new SolidBrush(Color.FromArgb(235, 30, 15)))
            using (var penOutline = new Pen(Color.Black))
            {
                var brushes = new[] { brushWarn, brushCrit, brushOver };

                var left = (float)padLeft;
                for (var i = 0; i < wds.Length; i++)
                {
                    var right = padLeft + wds[i];
                    g.FillRectangle(brushes[i], left, padTop, (right - left), cliHt);
                    left = right;
                }
                g.DrawRectangle(penOutline, padLeft, padTop, cliWd, cliHt);

                // показать на картинке плечо
                var x = padLeft + currentLeverage*scaleX;
                var points = new []
                    {
                        new PointF(x, padTop), 
                        new PointF(x - 3, padTop - 4), 
                        new PointF(x + 3, padTop - 4)
                    };
                g.DrawPolygon(penOutline, points);
                g.DrawLine(penOutline, x, padTop, x, padTop + cliHt);

                // подписать риск
                var strCritLevrg = Localizer.GetString("TitleAllowableLeverageSmall") + ": " + levWarn.ToString("f2");
                var strOverLevrg = Localizer.GetString("TitleCriticalLeverageSmall") + ": " + levCrit.ToString("f2");

                var fmt = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near};
                g.DrawString(strCritLevrg,
                    Font, brushCritText, padLeft + levWarn * scaleX, padTop + cliHt + 2, fmt);
                var textSz = g.MeasureString(strCritLevrg, Font);
                g.DrawString(strOverLevrg, Font, brushOver, padLeft + levCrit * scaleX, padTop + cliHt + 2 + textSz.Height + 2, fmt);
            }

            picLeverage.Invalidate();
        }

        private void TbRiskWarningTextChanged(object sender, EventArgs e)
        {
            // обновить картинку
            UpdateRiskPicture();
            SettingsAreChanged = true;
        }

        #endregion        

        private void ReadParamsFromConfig()
        {
            dontReactOnFieldsUpdating = true;
            cbMessageOnDealOpening.Checked = UserSettings.Instance.MessageOnEnterExceedLeverage;
            tbRiskWarning.Text = UserSettings.Instance.RiskLeverWarning.ToStringUniform();
            tbRiskCritical.Text = UserSettings.Instance.RiskLeverCritical.ToStringUniform();
            
            riskSetupControl.ShowUserSettings();
            dontReactOnFieldsUpdating = false;            
        }

        private void RiskSettingsAreChanged()
        {
            SettingsAreChanged = true;
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            // сохранить настройки
            UserSettings.Instance.MessageOnEnterExceedLeverage = cbMessageOnDealOpening.Checked;
            var riskWarn = tbRiskWarning.Text.ToFloatUniformSafe();
            if (riskWarn.HasValue) UserSettings.Instance.RiskLeverWarning = riskWarn.Value;
            var riskCrit = tbRiskCritical.Text.ToFloatUniformSafe();
            if (riskCrit.HasValue) UserSettings.Instance.RiskLeverCritical = riskCrit.Value;
            riskSetupControl.ApplyToUserSettings();

            UserSettings.Instance.SaveSettings();
            DialogResult = DialogResult.OK;
        }

        private void TabPageColorValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var errors = new List<string>();

            // TODO: use NumericUpDownControl
            ValidateNumber(errors, Localizer.GetString("TitleHighRiskSmall"), tbRiskWarning.Text, false, 0.01f, 1000, "0.1, 5, 22.5");
            ValidateNumber(errors, Localizer.GetString("TitleUnallowableRiskSmall"), tbRiskWarning.Text, false, 0.01001f, 1000, "0.1, 5, 22.5");

            var riskWarn = tbRiskWarning.Text.ToFloatUniformSafe();
            var riskCrit = tbRiskCritical.Text.ToFloatUniformSafe();
            if (riskWarn.HasValue && riskCrit.HasValue && riskWarn.Value >= riskCrit.Value)
                errors.Add(Localizer.GetString("MessageHighRiskShouldBeLessThanUnacceptable"));

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(", ", errors), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private static void ValidateNumber(List<string> errors, string fieldName,
            string srcText, bool isInteger, float minValue, float maxValue, string typicalValues)
        {
            if (isInteger)
            {
                var val = srcText.ToIntSafe();
                if (!val.HasValue)
                {
                    errors.Add(string.Format(Localizer.GetString("MessageValueTypedWrongFmt"), fieldName) +
                        ". " + Localizer.GetString("MessageValidationCorrectExamples") + " " + typicalValues);
                    return;
                }
                
                if (val.Value < minValue || val.Value > maxValue)
                    errors.Add(string.Format(Localizer.GetString("MessageValueTypedWrongFmt"), fieldName) +
                        ". " + Localizer.GetString("MessageValidationCorrectRangeIs") + " " + minValue + " - " + maxValue);
                return;
            }
            
            var valF = srcText.ToFloatUniformSafe();
            if (!valF.HasValue)
            {
                errors.Add(string.Format(Localizer.GetString("MessageValueTypedWrongFmt"), fieldName) +
                    ". " + Localizer.GetString("MessageValidationCorrectExamples") + " " + typicalValues);
                return;
            }

            if (valF.Value < minValue || valF.Value > maxValue)
                errors.Add(string.Format(Localizer.GetString("MessageValueTypedWrongFmt"), fieldName) +
                    ". " + Localizer.GetString("MessageValidationCorrectRangeIs") + " " + minValue + " - " + maxValue);
        }

        /// <summary>
        /// вернуть сохраненные настройки
        /// </summary>
        private void BtnCancelClick(object sender, EventArgs e)
        {
            SettingsAreChanged = false;
            ReadParamsFromConfig();
        }

        private void RiskSetupFormLoad(object sender, EventArgs e)
        {
            if (DesignMode) return;

            // заполнить поля из настроек
            ReadParamsFromConfig();

            ShowCurrentLeverageOnChart();
            
            riskSetupControl.settingsChanged += RiskSettingsAreChanged;

            // запомнить окошко
            if (MainForm.Instance != null) // 4 winform test
                MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
                    {
                        Window = WindowCode,
                        WindowPos = Location,
                        WindowSize = Size,
                        WindowState = WindowState.ToString()
                    });

            timerUpdateRisk.Enabled = true;
        }

        private void ShowCurrentLeverageOnChart()
        {
            // посчитать текущее плечо
            CalculateCurrentLeverage();

            // показать плечи (риски) на картинке
            UpdateRiskPicture();
        }

        private void CbMessageOnDealOpeningCheckedChanged(object sender, EventArgs e)
        {
            SettingsAreChanged = true;
        }

        private void RiskSetupFormResizeEnd(object sender, EventArgs e)
        {
            if (ResizeEnded != null)
                ResizeEnded(this);
        }

        private void RiskSetupFormMove(object sender, EventArgs e)
        {
            if (FormMoved != null)
                FormMoved(this);
        }

        private void RiskSetupFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        private void TimerUpdateRiskTick(object sender, EventArgs e)
        {
            Invoke(new Action(ShowCurrentLeverageOnChart));
        }        

        private void PicLeverageSizeChanged(object sender, EventArgs e)
        {
            //picLeverage.Image = new Bitmap(picLeverage.Width, picLeverage.Height);
            UpdateRiskPicture();
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
