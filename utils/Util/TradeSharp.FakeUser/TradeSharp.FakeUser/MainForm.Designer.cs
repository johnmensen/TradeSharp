namespace TradeSharp.FakeUser
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.portfolioDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFarmsetsDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFarmSetsDialog = new System.Windows.Forms.OpenFileDialog();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblWorkerProgress = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainerUsers = new System.Windows.Forms.SplitContainer();
            this.tbUserNames = new System.Windows.Forms.RichTextBox();
            this.rtbLogins = new System.Windows.Forms.RichTextBox();
            this.panelCheckLogin = new System.Windows.Forms.Panel();
            this.btnCheckLogins = new System.Windows.Forms.Button();
            this.panelUser = new System.Windows.Forms.Panel();
            this.btnCreateAccount = new System.Windows.Forms.Button();
            this.tbNewAccountOpenTime = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.cbGroup = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCopyIds = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbStartDepo = new System.Windows.Forms.TextBox();
            this.tbCurrency = new System.Windows.Forms.TextBox();
            this.tbSignalCost = new System.Windows.Forms.TextBox();
            this.cbSignallers = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainerRobotFarm = new System.Windows.Forms.SplitContainer();
            this.richTextBoxAccountIds = new System.Windows.Forms.RichTextBox();
            this.panelRobotFarmIds = new System.Windows.Forms.Panel();
            this.btnGetAccountIdsForFarm = new System.Windows.Forms.Button();
            this.btnClearHistoryByAccounts = new System.Windows.Forms.Button();
            this.btnMoveMoneyOnAccounts = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.tbTickerProb = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbRobotsCount = new System.Windows.Forms.TextBox();
            this.tbPortfolioSetsPath = new System.Windows.Forms.TextBox();
            this.btnMakeRobotFarmSettings = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnLoadPortfolio = new System.Windows.Forms.Button();
            this.tabQuote = new System.Windows.Forms.TabPage();
            this.btnMakeDayQuotes = new System.Windows.Forms.Button();
            this.btnLoadFromTradeSharpServer = new System.Windows.Forms.Button();
            this.btnChangeDestinationQuoteFolder = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.tbDestinationQuoteFolder = new System.Windows.Forms.TextBox();
            this.btnLoadQuotes = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.tbQuoteLoadStartYear = new System.Windows.Forms.TextBox();
            this.tabTradeHistory = new System.Windows.Forms.TabPage();
            this.label22 = new System.Windows.Forms.Label();
            this.tbFlipOrdersPercent = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.btnAmendHistory = new System.Windows.Forms.Button();
            this.btnCorrectBalance = new System.Windows.Forms.Button();
            this.btnCorrectEquity = new System.Windows.Forms.Button();
            this.tbTargetCorrectedAmount = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.dpModelStrategiesEnd = new System.Windows.Forms.DateTimePicker();
            this.dpModelStartegiesStart = new System.Windows.Forms.DateTimePicker();
            this.tbStartTestDepo = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.btnTestStrategies = new System.Windows.Forms.Button();
            this.tbWdthProbOnLoss = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tbWithdrawProb = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbSkipLossProb = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tbTrackIntervalMinutes = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btnMakeJson = new System.Windows.Forms.Button();
            this.btnFixBalanceChanges = new System.Windows.Forms.Button();
            this.lblHistoryProgress = new System.Windows.Forms.Label();
            this.btnMakeHistoryOperations = new System.Windows.Forms.Button();
            this.btnPickFarmSetsFile = new System.Windows.Forms.Button();
            this.tbFarmSetsPath = new System.Windows.Forms.TextBox();
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerUsers)).BeginInit();
            this.splitContainerUsers.Panel1.SuspendLayout();
            this.splitContainerUsers.Panel2.SuspendLayout();
            this.splitContainerUsers.SuspendLayout();
            this.panelCheckLogin.SuspendLayout();
            this.panelUser.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRobotFarm)).BeginInit();
            this.splitContainerRobotFarm.Panel1.SuspendLayout();
            this.splitContainerRobotFarm.Panel2.SuspendLayout();
            this.splitContainerRobotFarm.SuspendLayout();
            this.panelRobotFarmIds.SuspendLayout();
            this.tabQuote.SuspendLayout();
            this.tabTradeHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico load.png");
            // 
            // portfolioDialog
            // 
            this.portfolioDialog.DefaultExt = "pxml";
            this.portfolioDialog.Filter = "Роботы (*.pxml)|*.pxml|Все файлы|*.*";
            this.portfolioDialog.FilterIndex = 0;
            this.portfolioDialog.Title = "Настройки роботов";
            // 
            // saveFarmsetsDialog
            // 
            this.saveFarmsetsDialog.DefaultExt = "xml";
            this.saveFarmsetsDialog.FileName = "farmsets.xml";
            this.saveFarmsetsDialog.Filter = "Настройки фермы роботов (*.xml)|*.xml|Все файлы|*.*";
            this.saveFarmsetsDialog.FilterIndex = 0;
            this.saveFarmsetsDialog.Title = "Сохранить настройки фермы роботов";
            // 
            // openFarmSetsDialog
            // 
            this.openFarmSetsDialog.Filter = "Настройки фермы роботов (*.xml)|*.xml|Все файлы|*.*";
            this.openFarmSetsDialog.FilterIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.lblWorkerProgress);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 375);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(602, 36);
            this.panelBottom.TabIndex = 0;
            // 
            // lblWorkerProgress
            // 
            this.lblWorkerProgress.AutoSize = true;
            this.lblWorkerProgress.Location = new System.Drawing.Point(9, 14);
            this.lblWorkerProgress.Name = "lblWorkerProgress";
            this.lblWorkerProgress.Size = new System.Drawing.Size(10, 13);
            this.lblWorkerProgress.TabIndex = 7;
            this.lblWorkerProgress.Text = "-";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabQuote);
            this.tabControl.Controls.Add(this.tabTradeHistory);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(602, 375);
            this.tabControl.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainerUsers);
            this.tabPage1.Controls.Add(this.panelUser);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(594, 349);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Пользователи";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainerUsers
            // 
            this.splitContainerUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerUsers.Location = new System.Drawing.Point(236, 3);
            this.splitContainerUsers.Name = "splitContainerUsers";
            this.splitContainerUsers.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerUsers.Panel1
            // 
            this.splitContainerUsers.Panel1.Controls.Add(this.tbUserNames);
            // 
            // splitContainerUsers.Panel2
            // 
            this.splitContainerUsers.Panel2.Controls.Add(this.rtbLogins);
            this.splitContainerUsers.Panel2.Controls.Add(this.panelCheckLogin);
            this.splitContainerUsers.Size = new System.Drawing.Size(355, 343);
            this.splitContainerUsers.SplitterDistance = 145;
            this.splitContainerUsers.TabIndex = 1;
            // 
            // tbUserNames
            // 
            this.tbUserNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbUserNames.Location = new System.Drawing.Point(0, 0);
            this.tbUserNames.Name = "tbUserNames";
            this.tbUserNames.Size = new System.Drawing.Size(355, 145);
            this.tbUserNames.TabIndex = 2;
            this.tbUserNames.Text = resources.GetString("tbUserNames.Text");
            // 
            // rtbLogins
            // 
            this.rtbLogins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLogins.Location = new System.Drawing.Point(0, 31);
            this.rtbLogins.Name = "rtbLogins";
            this.rtbLogins.Size = new System.Drawing.Size(355, 163);
            this.rtbLogins.TabIndex = 3;
            this.rtbLogins.Text = "RockNRolla\nWilliamsFractal";
            // 
            // panelCheckLogin
            // 
            this.panelCheckLogin.Controls.Add(this.btnCheckLogins);
            this.panelCheckLogin.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCheckLogin.Location = new System.Drawing.Point(0, 0);
            this.panelCheckLogin.Name = "panelCheckLogin";
            this.panelCheckLogin.Size = new System.Drawing.Size(355, 31);
            this.panelCheckLogin.TabIndex = 0;
            // 
            // btnCheckLogins
            // 
            this.btnCheckLogins.Location = new System.Drawing.Point(6, 4);
            this.btnCheckLogins.Name = "btnCheckLogins";
            this.btnCheckLogins.Size = new System.Drawing.Size(126, 23);
            this.btnCheckLogins.TabIndex = 0;
            this.btnCheckLogins.Text = "Проверить логины";
            this.btnCheckLogins.UseVisualStyleBackColor = true;
            this.btnCheckLogins.Click += new System.EventHandler(this.btnCheckLogins_Click);
            // 
            // panelUser
            // 
            this.panelUser.Controls.Add(this.btnCreateAccount);
            this.panelUser.Controls.Add(this.tbNewAccountOpenTime);
            this.panelUser.Controls.Add(this.label15);
            this.panelUser.Controls.Add(this.label5);
            this.panelUser.Controls.Add(this.tbPassword);
            this.panelUser.Controls.Add(this.cbGroup);
            this.panelUser.Controls.Add(this.label4);
            this.panelUser.Controls.Add(this.btnCopyIds);
            this.panelUser.Controls.Add(this.btnCreate);
            this.panelUser.Controls.Add(this.label3);
            this.panelUser.Controls.Add(this.label2);
            this.panelUser.Controls.Add(this.label1);
            this.panelUser.Controls.Add(this.tbStartDepo);
            this.panelUser.Controls.Add(this.tbCurrency);
            this.panelUser.Controls.Add(this.tbSignalCost);
            this.panelUser.Controls.Add(this.cbSignallers);
            this.panelUser.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelUser.Location = new System.Drawing.Point(3, 3);
            this.panelUser.Name = "panelUser";
            this.panelUser.Size = new System.Drawing.Size(233, 343);
            this.panelUser.TabIndex = 0;
            // 
            // btnCreateAccount
            // 
            this.btnCreateAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCreateAccount.Location = new System.Drawing.Point(8, 238);
            this.btnCreateAccount.Name = "btnCreateAccount";
            this.btnCreateAccount.Size = new System.Drawing.Size(144, 23);
            this.btnCreateAccount.TabIndex = 16;
            this.btnCreateAccount.Text = "Открыть счет...";
            this.btnCreateAccount.UseVisualStyleBackColor = true;
            this.btnCreateAccount.Click += new System.EventHandler(this.btnCreateAccount_Click);
            // 
            // tbNewAccountOpenTime
            // 
            this.tbNewAccountOpenTime.Location = new System.Drawing.Point(5, 193);
            this.tbNewAccountOpenTime.Name = "tbNewAccountOpenTime";
            this.tbNewAccountOpenTime.Size = new System.Drawing.Size(222, 20);
            this.tbNewAccountOpenTime.TabIndex = 15;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(5, 216);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(90, 13);
            this.label15.TabIndex = 14;
            this.label15.Text = "время открытия";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 128);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "пароль";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(3, 142);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(80, 20);
            this.tbPassword.TabIndex = 11;
            this.tbPassword.Text = "Trader01";
            // 
            // cbGroup
            // 
            this.cbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroup.FormattingEnabled = true;
            this.cbGroup.Location = new System.Drawing.Point(5, 103);
            this.cbGroup.Name = "cbGroup";
            this.cbGroup.Size = new System.Drawing.Size(222, 21);
            this.cbGroup.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "группа";
            // 
            // btnCopyIds
            // 
            this.btnCopyIds.Location = new System.Drawing.Point(8, 276);
            this.btnCopyIds.Name = "btnCopyIds";
            this.btnCopyIds.Size = new System.Drawing.Size(144, 23);
            this.btnCopyIds.TabIndex = 8;
            this.btnCopyIds.Text = "ID -> буфер обмена";
            this.btnCopyIds.UseVisualStyleBackColor = true;
            this.btnCopyIds.Click += new System.EventHandler(this.btnCopyIds_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(8, 305);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(144, 23);
            this.btnCreate.TabIndex = 7;
            this.btnCreate.Text = "Создать";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(91, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "цена сигнала";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "валюта";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "стартовый депозит";
            // 
            // tbStartDepo
            // 
            this.tbStartDepo.Location = new System.Drawing.Point(5, 25);
            this.tbStartDepo.Name = "tbStartDepo";
            this.tbStartDepo.Size = new System.Drawing.Size(222, 20);
            this.tbStartDepo.TabIndex = 3;
            this.tbStartDepo.Text = "[0:100 100:100]";
            // 
            // tbCurrency
            // 
            this.tbCurrency.Location = new System.Drawing.Point(5, 63);
            this.tbCurrency.Name = "tbCurrency";
            this.tbCurrency.Size = new System.Drawing.Size(222, 20);
            this.tbCurrency.TabIndex = 2;
            this.tbCurrency.Text = "USD";
            // 
            // tbSignalCost
            // 
            this.tbSignalCost.Location = new System.Drawing.Point(5, 167);
            this.tbSignalCost.Name = "tbSignalCost";
            this.tbSignalCost.Size = new System.Drawing.Size(80, 20);
            this.tbSignalCost.TabIndex = 1;
            this.tbSignalCost.Text = "1.25";
            // 
            // cbSignallers
            // 
            this.cbSignallers.AutoSize = true;
            this.cbSignallers.Checked = true;
            this.cbSignallers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSignallers.Location = new System.Drawing.Point(89, 144);
            this.cbSignallers.Name = "cbSignallers";
            this.cbSignallers.Size = new System.Drawing.Size(94, 17);
            this.cbSignallers.TabIndex = 0;
            this.cbSignallers.Text = "сигнальщики";
            this.cbSignallers.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainerRobotFarm);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(594, 349);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Ферма роботов";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainerRobotFarm
            // 
            this.splitContainerRobotFarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRobotFarm.Location = new System.Drawing.Point(3, 3);
            this.splitContainerRobotFarm.Name = "splitContainerRobotFarm";
            this.splitContainerRobotFarm.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRobotFarm.Panel1
            // 
            this.splitContainerRobotFarm.Panel1.Controls.Add(this.richTextBoxAccountIds);
            this.splitContainerRobotFarm.Panel1.Controls.Add(this.panelRobotFarmIds);
            // 
            // splitContainerRobotFarm.Panel2
            // 
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.btnClearHistoryByAccounts);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.btnMoveMoneyOnAccounts);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.label8);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.tbTickerProb);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.label7);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.tbRobotsCount);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.tbPortfolioSetsPath);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.btnMakeRobotFarmSettings);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.label6);
            this.splitContainerRobotFarm.Panel2.Controls.Add(this.btnLoadPortfolio);
            this.splitContainerRobotFarm.Size = new System.Drawing.Size(588, 343);
            this.splitContainerRobotFarm.SplitterDistance = 77;
            this.splitContainerRobotFarm.TabIndex = 0;
            // 
            // richTextBoxAccountIds
            // 
            this.richTextBoxAccountIds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxAccountIds.Location = new System.Drawing.Point(88, 0);
            this.richTextBoxAccountIds.Name = "richTextBoxAccountIds";
            this.richTextBoxAccountIds.Size = new System.Drawing.Size(500, 77);
            this.richTextBoxAccountIds.TabIndex = 1;
            this.richTextBoxAccountIds.Text = resources.GetString("richTextBoxAccountIds.Text");
            // 
            // panelRobotFarmIds
            // 
            this.panelRobotFarmIds.Controls.Add(this.btnGetAccountIdsForFarm);
            this.panelRobotFarmIds.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelRobotFarmIds.Location = new System.Drawing.Point(0, 0);
            this.panelRobotFarmIds.Name = "panelRobotFarmIds";
            this.panelRobotFarmIds.Size = new System.Drawing.Size(88, 77);
            this.panelRobotFarmIds.TabIndex = 0;
            // 
            // btnGetAccountIdsForFarm
            // 
            this.btnGetAccountIdsForFarm.Location = new System.Drawing.Point(3, 3);
            this.btnGetAccountIdsForFarm.Name = "btnGetAccountIdsForFarm";
            this.btnGetAccountIdsForFarm.Size = new System.Drawing.Size(82, 23);
            this.btnGetAccountIdsForFarm.TabIndex = 0;
            this.btnGetAccountIdsForFarm.Text = "Счета...";
            this.btnGetAccountIdsForFarm.UseVisualStyleBackColor = true;
            this.btnGetAccountIdsForFarm.Click += new System.EventHandler(this.btnGetAccountIdsForFarm_Click);
            // 
            // btnClearHistoryByAccounts
            // 
            this.btnClearHistoryByAccounts.Location = new System.Drawing.Point(8, 179);
            this.btnClearHistoryByAccounts.Name = "btnClearHistoryByAccounts";
            this.btnClearHistoryByAccounts.Size = new System.Drawing.Size(200, 23);
            this.btnClearHistoryByAccounts.TabIndex = 9;
            this.btnClearHistoryByAccounts.Text = "Очистить историю счетов";
            this.btnClearHistoryByAccounts.UseVisualStyleBackColor = true;
            this.btnClearHistoryByAccounts.Click += new System.EventHandler(this.btnClearHistoryByAccounts_Click);
            // 
            // btnMoveMoneyOnAccounts
            // 
            this.btnMoveMoneyOnAccounts.Location = new System.Drawing.Point(8, 150);
            this.btnMoveMoneyOnAccounts.Name = "btnMoveMoneyOnAccounts";
            this.btnMoveMoneyOnAccounts.Size = new System.Drawing.Size(200, 23);
            this.btnMoveMoneyOnAccounts.TabIndex = 8;
            this.btnMoveMoneyOnAccounts.Text = "Движения средств по счетам...";
            this.btnMoveMoneyOnAccounts.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(297, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(139, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "торг. актив / вероятность";
            // 
            // tbTickerProb
            // 
            this.tbTickerProb.Location = new System.Drawing.Point(300, 44);
            this.tbTickerProb.Multiline = true;
            this.tbTickerProb.Name = "tbTickerProb";
            this.tbTickerProb.Size = new System.Drawing.Size(118, 200);
            this.tbTickerProb.TabIndex = 6;
            this.tbTickerProb.Text = "EURUSD 27\r\nGBPUSD 12\r\nUSDCAD 10\r\nUSDJPY 13\r\nUSDCHF 6\r\nAUDUSD 6\r\nEURGBP 5\r\nEURCAD " +
    "4\r\nEURJPY 4\r\nEURCHF 3\r\nNZDUSD 4\r\nNZDJPY 3\r\nGBPCHF 3";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(245, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "вероятности / количество роботов в портфеле";
            // 
            // tbRobotsCount
            // 
            this.tbRobotsCount.Location = new System.Drawing.Point(5, 44);
            this.tbRobotsCount.Name = "tbRobotsCount";
            this.tbRobotsCount.Size = new System.Drawing.Size(232, 20);
            this.tbRobotsCount.TabIndex = 4;
            this.tbRobotsCount.Text = "25 20 15 10 10 10 5 3 2";
            // 
            // tbPortfolioSetsPath
            // 
            this.tbPortfolioSetsPath.Location = new System.Drawing.Point(5, 4);
            this.tbPortfolioSetsPath.Name = "tbPortfolioSetsPath";
            this.tbPortfolioSetsPath.Size = new System.Drawing.Size(331, 20);
            this.tbPortfolioSetsPath.TabIndex = 3;
            // 
            // btnMakeRobotFarmSettings
            // 
            this.btnMakeRobotFarmSettings.Location = new System.Drawing.Point(8, 121);
            this.btnMakeRobotFarmSettings.Name = "btnMakeRobotFarmSettings";
            this.btnMakeRobotFarmSettings.Size = new System.Drawing.Size(200, 23);
            this.btnMakeRobotFarmSettings.TabIndex = 2;
            this.btnMakeRobotFarmSettings.Text = "Сформировать настройки роботов";
            this.btnMakeRobotFarmSettings.UseVisualStyleBackColor = true;
            this.btnMakeRobotFarmSettings.Click += new System.EventHandler(this.btnMakeRobotFarmSettings_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(376, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "настройки роботов";
            // 
            // btnLoadPortfolio
            // 
            this.btnLoadPortfolio.ImageIndex = 0;
            this.btnLoadPortfolio.ImageList = this.imageList;
            this.btnLoadPortfolio.Location = new System.Drawing.Point(342, 4);
            this.btnLoadPortfolio.Name = "btnLoadPortfolio";
            this.btnLoadPortfolio.Size = new System.Drawing.Size(28, 23);
            this.btnLoadPortfolio.TabIndex = 0;
            this.btnLoadPortfolio.UseVisualStyleBackColor = true;
            this.btnLoadPortfolio.Click += new System.EventHandler(this.btnLoadPortfolio_Click);
            // 
            // tabQuote
            // 
            this.tabQuote.Controls.Add(this.btnMakeDayQuotes);
            this.tabQuote.Controls.Add(this.btnLoadFromTradeSharpServer);
            this.tabQuote.Controls.Add(this.btnChangeDestinationQuoteFolder);
            this.tabQuote.Controls.Add(this.label10);
            this.tabQuote.Controls.Add(this.tbDestinationQuoteFolder);
            this.tabQuote.Controls.Add(this.btnLoadQuotes);
            this.tabQuote.Controls.Add(this.label9);
            this.tabQuote.Controls.Add(this.tbQuoteLoadStartYear);
            this.tabQuote.Location = new System.Drawing.Point(4, 22);
            this.tabQuote.Name = "tabQuote";
            this.tabQuote.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuote.Size = new System.Drawing.Size(594, 349);
            this.tabQuote.TabIndex = 2;
            this.tabQuote.Text = "Котировки";
            this.tabQuote.UseVisualStyleBackColor = true;
            // 
            // btnMakeDayQuotes
            // 
            this.btnMakeDayQuotes.Location = new System.Drawing.Point(286, 79);
            this.btnMakeDayQuotes.Name = "btnMakeDayQuotes";
            this.btnMakeDayQuotes.Size = new System.Drawing.Size(155, 23);
            this.btnMakeDayQuotes.TabIndex = 8;
            this.btnMakeDayQuotes.Text = "Сформировать дневные";
            this.btnMakeDayQuotes.UseVisualStyleBackColor = true;
            this.btnMakeDayQuotes.Click += new System.EventHandler(this.btnMakeDayQuotes_Click);
            // 
            // btnLoadFromTradeSharpServer
            // 
            this.btnLoadFromTradeSharpServer.Location = new System.Drawing.Point(89, 79);
            this.btnLoadFromTradeSharpServer.Name = "btnLoadFromTradeSharpServer";
            this.btnLoadFromTradeSharpServer.Size = new System.Drawing.Size(191, 23);
            this.btnLoadFromTradeSharpServer.TabIndex = 7;
            this.btnLoadFromTradeSharpServer.Text = "Загрузить с сервера T#";
            this.btnLoadFromTradeSharpServer.UseVisualStyleBackColor = true;
            this.btnLoadFromTradeSharpServer.Click += new System.EventHandler(this.ButtonLoadClick);
            // 
            // btnChangeDestinationQuoteFolder
            // 
            this.btnChangeDestinationQuoteFolder.Location = new System.Drawing.Point(384, 21);
            this.btnChangeDestinationQuoteFolder.Name = "btnChangeDestinationQuoteFolder";
            this.btnChangeDestinationQuoteFolder.Size = new System.Drawing.Size(26, 23);
            this.btnChangeDestinationQuoteFolder.TabIndex = 5;
            this.btnChangeDestinationQuoteFolder.Text = "...";
            this.btnChangeDestinationQuoteFolder.UseVisualStyleBackColor = true;
            this.btnChangeDestinationQuoteFolder.Click += new System.EventHandler(this.btnChangeDestinationQuoteFolder_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(116, 7);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(113, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "Каталог назначения:";
            // 
            // tbDestinationQuoteFolder
            // 
            this.tbDestinationQuoteFolder.Location = new System.Drawing.Point(116, 23);
            this.tbDestinationQuoteFolder.Name = "tbDestinationQuoteFolder";
            this.tbDestinationQuoteFolder.Size = new System.Drawing.Size(262, 20);
            this.tbDestinationQuoteFolder.TabIndex = 3;
            // 
            // btnLoadQuotes
            // 
            this.btnLoadQuotes.Location = new System.Drawing.Point(8, 79);
            this.btnLoadQuotes.Name = "btnLoadQuotes";
            this.btnLoadQuotes.Size = new System.Drawing.Size(75, 23);
            this.btnLoadQuotes.TabIndex = 2;
            this.btnLoadQuotes.Text = "Загрузить котировки";
            this.btnLoadQuotes.UseVisualStyleBackColor = true;
            this.btnLoadQuotes.Click += new System.EventHandler(this.ButtonLoadClick);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Начать с:";
            // 
            // tbQuoteLoadStartYear
            // 
            this.tbQuoteLoadStartYear.Location = new System.Drawing.Point(8, 23);
            this.tbQuoteLoadStartYear.Name = "tbQuoteLoadStartYear";
            this.tbQuoteLoadStartYear.Size = new System.Drawing.Size(78, 20);
            this.tbQuoteLoadStartYear.TabIndex = 0;
            this.tbQuoteLoadStartYear.Text = "2009";
            // 
            // tabTradeHistory
            // 
            this.tabTradeHistory.Controls.Add(this.label22);
            this.tabTradeHistory.Controls.Add(this.tbFlipOrdersPercent);
            this.tabTradeHistory.Controls.Add(this.label21);
            this.tabTradeHistory.Controls.Add(this.btnAmendHistory);
            this.tabTradeHistory.Controls.Add(this.btnCorrectBalance);
            this.tabTradeHistory.Controls.Add(this.btnCorrectEquity);
            this.tabTradeHistory.Controls.Add(this.tbTargetCorrectedAmount);
            this.tabTradeHistory.Controls.Add(this.label20);
            this.tabTradeHistory.Controls.Add(this.dpModelStrategiesEnd);
            this.tabTradeHistory.Controls.Add(this.dpModelStartegiesStart);
            this.tabTradeHistory.Controls.Add(this.tbStartTestDepo);
            this.tabTradeHistory.Controls.Add(this.label19);
            this.tabTradeHistory.Controls.Add(this.btnTestStrategies);
            this.tabTradeHistory.Controls.Add(this.tbWdthProbOnLoss);
            this.tabTradeHistory.Controls.Add(this.label18);
            this.tabTradeHistory.Controls.Add(this.label17);
            this.tabTradeHistory.Controls.Add(this.tbWithdrawProb);
            this.tabTradeHistory.Controls.Add(this.label16);
            this.tabTradeHistory.Controls.Add(this.label13);
            this.tabTradeHistory.Controls.Add(this.tbSkipLossProb);
            this.tabTradeHistory.Controls.Add(this.label12);
            this.tabTradeHistory.Controls.Add(this.label11);
            this.tabTradeHistory.Controls.Add(this.tbTrackIntervalMinutes);
            this.tabTradeHistory.Controls.Add(this.label14);
            this.tabTradeHistory.Controls.Add(this.btnMakeJson);
            this.tabTradeHistory.Controls.Add(this.btnFixBalanceChanges);
            this.tabTradeHistory.Controls.Add(this.lblHistoryProgress);
            this.tabTradeHistory.Controls.Add(this.btnMakeHistoryOperations);
            this.tabTradeHistory.Controls.Add(this.btnPickFarmSetsFile);
            this.tabTradeHistory.Controls.Add(this.tbFarmSetsPath);
            this.tabTradeHistory.Location = new System.Drawing.Point(4, 22);
            this.tabTradeHistory.Name = "tabTradeHistory";
            this.tabTradeHistory.Size = new System.Drawing.Size(594, 349);
            this.tabTradeHistory.TabIndex = 3;
            this.tabTradeHistory.Text = "История торгов";
            this.tabTradeHistory.UseVisualStyleBackColor = true;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(503, 249);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(54, 13);
            this.label22.TabIndex = 34;
            this.label22.Text = "% сделок";
            // 
            // tbFlipOrdersPercent
            // 
            this.tbFlipOrdersPercent.Location = new System.Drawing.Point(463, 245);
            this.tbFlipOrdersPercent.Name = "tbFlipOrdersPercent";
            this.tbFlipOrdersPercent.Size = new System.Drawing.Size(37, 20);
            this.tbFlipOrdersPercent.TabIndex = 33;
            this.tbFlipOrdersPercent.Text = "8";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(391, 248);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(71, 13);
            this.label21.TabIndex = 32;
            this.label21.Text = "перевернуть";
            // 
            // btnAmendHistory
            // 
            this.btnAmendHistory.AccessibleDescription = "d";
            this.btnAmendHistory.Location = new System.Drawing.Point(390, 269);
            this.btnAmendHistory.Name = "btnAmendHistory";
            this.btnAmendHistory.Size = new System.Drawing.Size(168, 23);
            this.btnAmendHistory.TabIndex = 31;
            this.btnAmendHistory.Text = "Приукрасить историю";
            this.btnAmendHistory.UseVisualStyleBackColor = true;
            this.btnAmendHistory.Click += new System.EventHandler(this.btnAmendHistory_Click);
            // 
            // btnCorrectBalance
            // 
            this.btnCorrectBalance.Location = new System.Drawing.Point(182, 244);
            this.btnCorrectBalance.Name = "btnCorrectBalance";
            this.btnCorrectBalance.Size = new System.Drawing.Size(168, 23);
            this.btnCorrectBalance.TabIndex = 30;
            this.btnCorrectBalance.Text = "Корректировать баланс";
            this.btnCorrectBalance.UseVisualStyleBackColor = true;
            this.btnCorrectBalance.Click += new System.EventHandler(this.btnCorrectBalance_Click);
            // 
            // btnCorrectEquity
            // 
            this.btnCorrectEquity.Location = new System.Drawing.Point(8, 244);
            this.btnCorrectEquity.Name = "btnCorrectEquity";
            this.btnCorrectEquity.Size = new System.Drawing.Size(168, 23);
            this.btnCorrectEquity.TabIndex = 29;
            this.btnCorrectEquity.Text = "Корректировать средства";
            this.btnCorrectEquity.UseVisualStyleBackColor = true;
            this.btnCorrectEquity.Click += new System.EventHandler(this.btnCorrectEquity_Click);
            // 
            // tbTargetCorrectedAmount
            // 
            this.tbTargetCorrectedAmount.Location = new System.Drawing.Point(47, 271);
            this.tbTargetCorrectedAmount.Name = "tbTargetCorrectedAmount";
            this.tbTargetCorrectedAmount.Size = new System.Drawing.Size(72, 20);
            this.tbTargetCorrectedAmount.TabIndex = 28;
            this.tbTargetCorrectedAmount.Text = "90..140, d:20";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(7, 274);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(34, 13);
            this.label20.TabIndex = 27;
            this.label20.Text = "цель:";
            // 
            // dpModelStrategiesEnd
            // 
            this.dpModelStrategiesEnd.Location = new System.Drawing.Point(360, 161);
            this.dpModelStrategiesEnd.Name = "dpModelStrategiesEnd";
            this.dpModelStrategiesEnd.Size = new System.Drawing.Size(200, 20);
            this.dpModelStrategiesEnd.TabIndex = 26;
            // 
            // dpModelStartegiesStart
            // 
            this.dpModelStartegiesStart.Location = new System.Drawing.Point(360, 135);
            this.dpModelStartegiesStart.Name = "dpModelStartegiesStart";
            this.dpModelStartegiesStart.Size = new System.Drawing.Size(200, 20);
            this.dpModelStartegiesStart.TabIndex = 25;
            // 
            // tbStartTestDepo
            // 
            this.tbStartTestDepo.Location = new System.Drawing.Point(428, 109);
            this.tbStartTestDepo.Name = "tbStartTestDepo";
            this.tbStartTestDepo.Size = new System.Drawing.Size(72, 20);
            this.tbStartTestDepo.TabIndex = 24;
            this.tbStartTestDepo.Text = "100";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(357, 112);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 13);
            this.label19.TabIndex = 23;
            this.label19.Text = "старт. депо";
            // 
            // btnTestStrategies
            // 
            this.btnTestStrategies.Location = new System.Drawing.Point(360, 198);
            this.btnTestStrategies.Name = "btnTestStrategies";
            this.btnTestStrategies.Size = new System.Drawing.Size(165, 23);
            this.btnTestStrategies.TabIndex = 22;
            this.btnTestStrategies.Text = "Тест стратегий";
            this.btnTestStrategies.UseVisualStyleBackColor = true;
            this.btnTestStrategies.Click += new System.EventHandler(this.btnTestStrategies_Click);
            // 
            // tbWdthProbOnLoss
            // 
            this.tbWdthProbOnLoss.Location = new System.Drawing.Point(90, 105);
            this.tbWdthProbOnLoss.Name = "tbWdthProbOnLoss";
            this.tbWdthProbOnLoss.Size = new System.Drawing.Size(72, 20);
            this.tbWdthProbOnLoss.TabIndex = 21;
            this.tbWdthProbOnLoss.Text = "15 / 85";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(7, 108);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(69, 13);
            this.label18.TabIndex = 20;
            this.label18.Text = "вер. вывода";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(171, 82);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(42, 13);
            this.label17.TabIndex = 19;
            this.label17.Text = "минуту";
            // 
            // tbWithdrawProb
            // 
            this.tbWithdrawProb.Location = new System.Drawing.Point(90, 79);
            this.tbWithdrawProb.Name = "tbWithdrawProb";
            this.tbWithdrawProb.Size = new System.Drawing.Size(72, 20);
            this.tbWithdrawProb.TabIndex = 18;
            this.tbWithdrawProb.Text = "10000";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(7, 82);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 13);
            this.label16.TabIndex = 17;
            this.label16.Text = "выводов в 1 /";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(171, 56);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 16;
            this.label13.Text = "% убытков";
            // 
            // tbSkipLossProb
            // 
            this.tbSkipLossProb.Location = new System.Drawing.Point(90, 53);
            this.tbSkipLossProb.Name = "tbSkipLossProb";
            this.tbSkipLossProb.Size = new System.Drawing.Size(72, 20);
            this.tbSkipLossProb.TabIndex = 15;
            this.tbSkipLossProb.Text = "2";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 56);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(83, 13);
            this.label12.TabIndex = 14;
            this.label12.Text = "инвертировать";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(169, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "файл настроек фермы роботов:";
            // 
            // tbTrackIntervalMinutes
            // 
            this.tbTrackIntervalMinutes.Location = new System.Drawing.Point(489, 50);
            this.tbTrackIntervalMinutes.Name = "tbTrackIntervalMinutes";
            this.tbTrackIntervalMinutes.Size = new System.Drawing.Size(53, 20);
            this.tbTrackIntervalMinutes.TabIndex = 12;
            this.tbTrackIntervalMinutes.Text = "60";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(358, 53);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(125, 13);
            this.label14.TabIndex = 11;
            this.label14.Text = "интервал трека, минут:";
            // 
            // btnMakeJson
            // 
            this.btnMakeJson.Location = new System.Drawing.Point(361, 76);
            this.btnMakeJson.Name = "btnMakeJson";
            this.btnMakeJson.Size = new System.Drawing.Size(181, 23);
            this.btnMakeJson.TabIndex = 10;
            this.btnMakeJson.Text = "Сформировать JSON";
            this.btnMakeJson.UseVisualStyleBackColor = true;
            this.btnMakeJson.Click += new System.EventHandler(this.btnMakeJson_Click);
            // 
            // btnFixBalanceChanges
            // 
            this.btnFixBalanceChanges.Location = new System.Drawing.Point(11, 198);
            this.btnFixBalanceChanges.Name = "btnFixBalanceChanges";
            this.btnFixBalanceChanges.Size = new System.Drawing.Size(165, 23);
            this.btnFixBalanceChanges.TabIndex = 9;
            this.btnFixBalanceChanges.Text = "Верифицировать транзакции";
            this.btnFixBalanceChanges.UseVisualStyleBackColor = true;
            this.btnFixBalanceChanges.Click += new System.EventHandler(this.btnFixBalanceChanges_Click);
            // 
            // lblHistoryProgress
            // 
            this.lblHistoryProgress.AutoSize = true;
            this.lblHistoryProgress.Location = new System.Drawing.Point(8, 224);
            this.lblHistoryProgress.Name = "lblHistoryProgress";
            this.lblHistoryProgress.Size = new System.Drawing.Size(10, 13);
            this.lblHistoryProgress.TabIndex = 8;
            this.lblHistoryProgress.Text = "-";
            // 
            // btnMakeHistoryOperations
            // 
            this.btnMakeHistoryOperations.Location = new System.Drawing.Point(11, 169);
            this.btnMakeHistoryOperations.Name = "btnMakeHistoryOperations";
            this.btnMakeHistoryOperations.Size = new System.Drawing.Size(165, 23);
            this.btnMakeHistoryOperations.TabIndex = 7;
            this.btnMakeHistoryOperations.Text = "Записать в БД";
            this.btnMakeHistoryOperations.UseVisualStyleBackColor = true;
            this.btnMakeHistoryOperations.Click += new System.EventHandler(this.btnMakeHistoryOperations_Click);
            // 
            // btnPickFarmSetsFile
            // 
            this.btnPickFarmSetsFile.Location = new System.Drawing.Point(452, 18);
            this.btnPickFarmSetsFile.Name = "btnPickFarmSetsFile";
            this.btnPickFarmSetsFile.Size = new System.Drawing.Size(31, 23);
            this.btnPickFarmSetsFile.TabIndex = 1;
            this.btnPickFarmSetsFile.Text = "...";
            this.btnPickFarmSetsFile.UseVisualStyleBackColor = true;
            this.btnPickFarmSetsFile.Click += new System.EventHandler(this.btnPickFarmSetsFile_Click);
            // 
            // tbFarmSetsPath
            // 
            this.tbFarmSetsPath.Location = new System.Drawing.Point(8, 20);
            this.tbFarmSetsPath.Name = "tbFarmSetsPath";
            this.tbFarmSetsPath.Size = new System.Drawing.Size(438, 20);
            this.tbFarmSetsPath.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 411);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Name = "MainForm";
            this.Text = "Пользователи Trade#";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainerUsers.Panel1.ResumeLayout(false);
            this.splitContainerUsers.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerUsers)).EndInit();
            this.splitContainerUsers.ResumeLayout(false);
            this.panelCheckLogin.ResumeLayout(false);
            this.panelUser.ResumeLayout(false);
            this.panelUser.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.splitContainerRobotFarm.Panel1.ResumeLayout(false);
            this.splitContainerRobotFarm.Panel2.ResumeLayout(false);
            this.splitContainerRobotFarm.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRobotFarm)).EndInit();
            this.splitContainerRobotFarm.ResumeLayout(false);
            this.panelRobotFarmIds.ResumeLayout(false);
            this.tabQuote.ResumeLayout(false);
            this.tabQuote.PerformLayout();
            this.tabTradeHistory.ResumeLayout(false);
            this.tabTradeHistory.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.OpenFileDialog portfolioDialog;
        private System.Windows.Forms.SaveFileDialog saveFarmsetsDialog;
        private System.Windows.Forms.OpenFileDialog openFarmSetsDialog;
        private System.Windows.Forms.Label lblWorkerProgress;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainerUsers;
        private System.Windows.Forms.RichTextBox tbUserNames;
        private System.Windows.Forms.RichTextBox rtbLogins;
        private System.Windows.Forms.Panel panelCheckLogin;
        private System.Windows.Forms.Button btnCheckLogins;
        private System.Windows.Forms.Panel panelUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.ComboBox cbGroup;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCopyIds;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbStartDepo;
        private System.Windows.Forms.TextBox tbCurrency;
        private System.Windows.Forms.TextBox tbSignalCost;
        private System.Windows.Forms.CheckBox cbSignallers;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainerRobotFarm;
        private System.Windows.Forms.RichTextBox richTextBoxAccountIds;
        private System.Windows.Forms.Panel panelRobotFarmIds;
        private System.Windows.Forms.Button btnGetAccountIdsForFarm;
        private System.Windows.Forms.Button btnMoveMoneyOnAccounts;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbTickerProb;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbRobotsCount;
        private System.Windows.Forms.TextBox tbPortfolioSetsPath;
        private System.Windows.Forms.Button btnMakeRobotFarmSettings;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnLoadPortfolio;
        private System.Windows.Forms.TabPage tabQuote;
        private System.Windows.Forms.Button btnLoadFromTradeSharpServer;
        private System.Windows.Forms.Button btnChangeDestinationQuoteFolder;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbDestinationQuoteFolder;
        private System.Windows.Forms.Button btnLoadQuotes;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbQuoteLoadStartYear;
        private System.Windows.Forms.TabPage tabTradeHistory;
        private System.Windows.Forms.TextBox tbTrackIntervalMinutes;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnMakeJson;
        private System.Windows.Forms.Button btnFixBalanceChanges;
        private System.Windows.Forms.Label lblHistoryProgress;
        private System.Windows.Forms.Button btnMakeHistoryOperations;
        private System.Windows.Forms.Button btnPickFarmSetsFile;
        private System.Windows.Forms.TextBox tbFarmSetsPath;
        private System.Windows.Forms.TextBox tbNewAccountOpenTime;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbSkipLossProb;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnMakeDayQuotes;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbWithdrawProb;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox tbWdthProbOnLoss;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.DateTimePicker dpModelStartegiesStart;
        private System.Windows.Forms.TextBox tbStartTestDepo;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button btnTestStrategies;
        private System.Windows.Forms.DateTimePicker dpModelStrategiesEnd;
        private System.Windows.Forms.Button btnCorrectBalance;
        private System.Windows.Forms.Button btnCorrectEquity;
        private System.Windows.Forms.TextBox tbTargetCorrectedAmount;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button btnClearHistoryByAccounts;
        private System.Windows.Forms.Button btnAmendHistory;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tbFlipOrdersPercent;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button btnCreateAccount;        
    }
}

