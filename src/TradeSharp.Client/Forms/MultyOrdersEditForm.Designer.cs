using TradeSharp.Client.Controls;

namespace TradeSharp.Client.Forms
{
    partial class MultyOrdersEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultyOrdersEditForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.btnWizzardTP = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnWizzardSL = new System.Windows.Forms.Button();
            this.tbTrailing = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTakeprofit = new TradeSharp.Client.Controls.PreValuedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbStoploss = new TradeSharp.Client.Controls.PreValuedTextBox();
            this.tabPageAdditional = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.tbMagic = new TradeSharp.Client.Controls.PreValuedTextBox();
            this.tbCommentRobot = new TradeSharp.Client.Controls.PreValuedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbComment = new TradeSharp.Client.Controls.PreValuedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.gridOrders = new FastGrid.FastGrid();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageAdditional.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 250);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(361, 33);
            this.panelBottom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(119, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(7, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageMain);
            this.tabControl.Controls.Add(this.tabPageAdditional);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(361, 109);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.btnWizzardTP);
            this.tabPageMain.Controls.Add(this.btnWizzardSL);
            this.tabPageMain.Controls.Add(this.tbTrailing);
            this.tabPageMain.Controls.Add(this.label3);
            this.tabPageMain.Controls.Add(this.label2);
            this.tabPageMain.Controls.Add(this.tbTakeprofit);
            this.tabPageMain.Controls.Add(this.label1);
            this.tabPageMain.Controls.Add(this.tbStoploss);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(353, 83);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Основные";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // btnWizzardTP
            // 
            this.btnWizzardTP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWizzardTP.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWizzardTP.ImageIndex = 0;
            this.btnWizzardTP.ImageList = this.imageList;
            this.btnWizzardTP.Location = new System.Drawing.Point(137, 30);
            this.btnWizzardTP.Name = "btnWizzardTP";
            this.btnWizzardTP.Size = new System.Drawing.Size(126, 23);
            this.btnWizzardTP.TabIndex = 7;
            this.btnWizzardTP.Text = "Установить TP";
            this.btnWizzardTP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnWizzardTP.UseVisualStyleBackColor = true;
            this.btnWizzardTP.Click += new System.EventHandler(this.BtnWizzardTpClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico_wizzard.png");
            // 
            // btnWizzardSL
            // 
            this.btnWizzardSL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWizzardSL.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWizzardSL.ImageIndex = 0;
            this.btnWizzardSL.ImageList = this.imageList;
            this.btnWizzardSL.Location = new System.Drawing.Point(137, 4);
            this.btnWizzardSL.Name = "btnWizzardSL";
            this.btnWizzardSL.Size = new System.Drawing.Size(126, 23);
            this.btnWizzardSL.TabIndex = 6;
            this.btnWizzardSL.Text = "Установить SL";
            this.btnWizzardSL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnWizzardSL.UseVisualStyleBackColor = true;
            this.btnWizzardSL.Click += new System.EventHandler(this.BtnWizzardSlClick);
            // 
            // tbTrailing
            // 
            this.tbTrailing.Location = new System.Drawing.Point(62, 58);
            this.tbTrailing.Name = "tbTrailing";
            this.tbTrailing.Size = new System.Drawing.Size(201, 20);
            this.tbTrailing.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Трейлинг";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "TP";
            // 
            // tbTakeprofit
            // 
            this.tbTakeprofit.BoundPropertyName = null;
            this.tbTakeprofit.BoundPropertyTitle = null;
            this.tbTakeprofit.DefaultColor = System.Drawing.Color.Gray;
            this.tbTakeprofit.DefaultValue = null;
            this.tbTakeprofit.Enabled = false;
            this.tbTakeprofit.ForeColor = System.Drawing.Color.Gray;
            this.tbTakeprofit.Location = new System.Drawing.Point(62, 32);
            this.tbTakeprofit.Name = "tbTakeprofit";
            this.tbTakeprofit.Size = new System.Drawing.Size(69, 20);
            this.tbTakeprofit.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SL";
            // 
            // tbStoploss
            // 
            this.tbStoploss.BoundPropertyName = null;
            this.tbStoploss.BoundPropertyTitle = null;
            this.tbStoploss.DefaultColor = System.Drawing.Color.Gray;
            this.tbStoploss.DefaultValue = null;
            this.tbStoploss.Enabled = false;
            this.tbStoploss.ForeColor = System.Drawing.Color.Gray;
            this.tbStoploss.Location = new System.Drawing.Point(62, 6);
            this.tbStoploss.Name = "tbStoploss";
            this.tbStoploss.Size = new System.Drawing.Size(69, 20);
            this.tbStoploss.TabIndex = 0;
            // 
            // tabPageAdditional
            // 
            this.tabPageAdditional.Controls.Add(this.label6);
            this.tabPageAdditional.Controls.Add(this.tbMagic);
            this.tabPageAdditional.Controls.Add(this.tbCommentRobot);
            this.tabPageAdditional.Controls.Add(this.label5);
            this.tabPageAdditional.Controls.Add(this.tbComment);
            this.tabPageAdditional.Controls.Add(this.label4);
            this.tabPageAdditional.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdditional.Name = "tabPageAdditional";
            this.tabPageAdditional.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdditional.Size = new System.Drawing.Size(353, 83);
            this.tabPageAdditional.TabIndex = 1;
            this.tabPageAdditional.Text = "Дополнительные";
            this.tabPageAdditional.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(48, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Magic";
            // 
            // tbMagic
            // 
            this.tbMagic.BoundPropertyName = null;
            this.tbMagic.BoundPropertyTitle = null;
            this.tbMagic.DefaultColor = System.Drawing.Color.Gray;
            this.tbMagic.DefaultValue = null;
            this.tbMagic.ForeColor = System.Drawing.Color.Gray;
            this.tbMagic.Location = new System.Drawing.Point(89, 57);
            this.tbMagic.Name = "tbMagic";
            this.tbMagic.Size = new System.Drawing.Size(69, 20);
            this.tbMagic.TabIndex = 10;
            // 
            // tbCommentRobot
            // 
            this.tbCommentRobot.BoundPropertyName = null;
            this.tbCommentRobot.BoundPropertyTitle = null;
            this.tbCommentRobot.DefaultColor = System.Drawing.Color.Gray;
            this.tbCommentRobot.DefaultValue = null;
            this.tbCommentRobot.ForeColor = System.Drawing.Color.Gray;
            this.tbCommentRobot.Location = new System.Drawing.Point(89, 32);
            this.tbCommentRobot.Name = "tbCommentRobot";
            this.tbCommentRobot.Size = new System.Drawing.Size(169, 20);
            this.tbCommentRobot.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Комм. робота";
            // 
            // tbComment
            // 
            this.tbComment.BoundPropertyName = null;
            this.tbComment.BoundPropertyTitle = null;
            this.tbComment.DefaultColor = System.Drawing.Color.Gray;
            this.tbComment.DefaultValue = null;
            this.tbComment.ForeColor = System.Drawing.Color.Gray;
            this.tbComment.Location = new System.Drawing.Point(89, 6);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(169, 20);
            this.tbComment.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Комментарий";
            // 
            // gridOrders
            // 
            this.gridOrders.CaptionHeight = 20;
            this.gridOrders.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridOrders.CellHeight = 18;
            this.gridOrders.CellPadding = 5;
            this.gridOrders.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridOrders.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridOrders.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridOrders.ColorCellFont = System.Drawing.Color.Black;
            this.gridOrders.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridOrders.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridOrders.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridOrders.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOrders.FitWidth = false;
            this.gridOrders.FontAnchoredRow = null;
            this.gridOrders.FontCell = null;
            this.gridOrders.FontHeader = null;
            this.gridOrders.FontSelectedCell = null;
            this.gridOrders.Location = new System.Drawing.Point(0, 109);
            this.gridOrders.MinimumTableWidth = null;
            this.gridOrders.MultiSelectEnabled = false;
            this.gridOrders.Name = "gridOrders";
            this.gridOrders.SelectEnabled = true;
            this.gridOrders.Size = new System.Drawing.Size(361, 141);
            this.gridOrders.StickFirst = false;
            this.gridOrders.StickLast = false;
            this.gridOrders.TabIndex = 2;
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 1500;
            this.timerUpdate.Tick += new System.EventHandler(this.TimerUpdateTick);
            // 
            // MultyOrdersEditForm
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(361, 283);
            this.Controls.Add(this.gridOrders);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MultyOrdersEditForm";
            this.Text = "Редактирование ордеров";
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageMain.PerformLayout();
            this.tabPageAdditional.ResumeLayout(false);
            this.tabPageAdditional.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TextBox tbTrailing;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageAdditional;
        private System.Windows.Forms.Label label6;
        private PreValuedTextBox tbTakeprofit;
        private PreValuedTextBox tbStoploss;
        private PreValuedTextBox tbMagic;
        private PreValuedTextBox tbCommentRobot;
        private PreValuedTextBox tbComment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private FastGrid.FastGrid gridOrders;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Button btnWizzardTP;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnWizzardSL;
    }
}