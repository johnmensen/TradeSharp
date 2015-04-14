namespace TradeSharp.Client.Forms
{
    partial class ManageAccountForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageAccountForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.gridAllAccounts = new FastGrid.FastGrid();
            this.contextMenuStripUpdate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.panelBorder = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.gridSelectedAccount = new System.Windows.Forms.DataGridView();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnReadonlyUser = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnEditUserData = new System.Windows.Forms.Button();
            this.btnBalance = new System.Windows.Forms.Button();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.btnRemindPassword = new System.Windows.Forms.Button();
            this.imageListGlyph = new System.Windows.Forms.ImageList(this.components);
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.contextMenuStripUpdate.SuspendLayout();
            this.panelBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelectedAccount)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnExit);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 338);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(392, 35);
            this.panelBottom.TabIndex = 0;
            // 
            // btnExit
            // 
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(10, 6);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(149, 23);
            this.btnExit.TabIndex = 0;
            this.btnExit.Tag = "TitleExit";
            this.btnExit.Text = "Выйти";
            this.btnExit.UseVisualStyleBackColor = true;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.gridAllAccounts);
            this.splitContainer.Panel1.Controls.Add(this.panelBorder);
            this.splitContainer.Panel1.Controls.Add(this.gridSelectedAccount);
            this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.btnReadonlyUser);
            this.splitContainer.Panel2.Controls.Add(this.btnRegister);
            this.splitContainer.Panel2.Controls.Add(this.btnEditUserData);
            this.splitContainer.Panel2.Controls.Add(this.btnBalance);
            this.splitContainer.Panel2.Controls.Add(this.btnChangePassword);
            this.splitContainer.Panel2.Controls.Add(this.btnRemindPassword);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer.Size = new System.Drawing.Size(392, 338);
            this.splitContainer.SplitterDistance = 200;
            this.splitContainer.TabIndex = 0;
            // 
            // gridAllAccounts
            // 
            this.gridAllAccounts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridAllAccounts.CaptionHeight = 20;
            this.gridAllAccounts.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridAllAccounts.CellHeight = 18;
            this.gridAllAccounts.CellPadding = 5;
            this.gridAllAccounts.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridAllAccounts.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAllAccounts.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAllAccounts.ColorCellFont = System.Drawing.Color.Black;
            this.gridAllAccounts.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridAllAccounts.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridAllAccounts.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridAllAccounts.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridAllAccounts.ContextMenuStrip = this.contextMenuStripUpdate;
            this.gridAllAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAllAccounts.FitWidth = false;
            this.gridAllAccounts.FontAnchoredRow = null;
            this.gridAllAccounts.FontCell = null;
            this.gridAllAccounts.FontHeader = null;
            this.gridAllAccounts.FontSelectedCell = null;
            this.gridAllAccounts.Location = new System.Drawing.Point(5, 106);
            this.gridAllAccounts.MinimumTableWidth = null;
            this.gridAllAccounts.MultiSelectEnabled = false;
            this.gridAllAccounts.Name = "gridAllAccounts";
            this.gridAllAccounts.SelectEnabled = true;
            this.gridAllAccounts.Size = new System.Drawing.Size(190, 227);
            this.gridAllAccounts.StickFirst = false;
            this.gridAllAccounts.StickLast = false;
            this.gridAllAccounts.TabIndex = 3;
            // 
            // contextMenuStripUpdate
            // 
            this.contextMenuStripUpdate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemUpdate});
            this.contextMenuStripUpdate.Name = "contextMenuStripUpdate";
            this.contextMenuStripUpdate.Size = new System.Drawing.Size(125, 26);
            // 
            // menuitemUpdate
            // 
            this.menuitemUpdate.Name = "menuitemUpdate";
            this.menuitemUpdate.Size = new System.Drawing.Size(124, 22);
            this.menuitemUpdate.Text = "Обновить";
            this.menuitemUpdate.Click += new System.EventHandler(this.MenuitemUpdateClick);
            // 
            // panelBorder
            // 
            this.panelBorder.Controls.Add(this.label15);
            this.panelBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBorder.Location = new System.Drawing.Point(5, 85);
            this.panelBorder.Name = "panelBorder";
            this.panelBorder.Size = new System.Drawing.Size(190, 21);
            this.panelBorder.TabIndex = 2;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(5, 3);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(57, 13);
            this.label15.TabIndex = 0;
            this.label15.Tag = "TitleAllAccounts";
            this.label15.Text = "Все счета";
            // 
            // gridSelectedAccount
            // 
            this.gridSelectedAccount.AllowUserToAddRows = false;
            this.gridSelectedAccount.AllowUserToDeleteRows = false;
            this.gridSelectedAccount.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridSelectedAccount.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gridSelectedAccount.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridSelectedAccount.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridSelectedAccount.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSelectedAccount.ColumnHeadersVisible = false;
            this.gridSelectedAccount.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTitle,
            this.colValue});
            this.gridSelectedAccount.ContextMenuStrip = this.contextMenuStripUpdate;
            this.gridSelectedAccount.Dock = System.Windows.Forms.DockStyle.Top;
            this.gridSelectedAccount.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridSelectedAccount.Location = new System.Drawing.Point(5, 5);
            this.gridSelectedAccount.Name = "gridSelectedAccount";
            this.gridSelectedAccount.ReadOnly = true;
            this.gridSelectedAccount.RowHeadersVisible = false;
            this.gridSelectedAccount.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridSelectedAccount.Size = new System.Drawing.Size(190, 80);
            this.gridSelectedAccount.TabIndex = 1;
            // 
            // colTitle
            // 
            this.colTitle.DataPropertyName = "Title";
            this.colTitle.HeaderText = "Заголовок";
            this.colTitle.Name = "colTitle";
            this.colTitle.ReadOnly = true;
            // 
            // colValue
            // 
            this.colValue.DataPropertyName = "Value";
            this.colValue.HeaderText = "Значение";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            // 
            // btnReadonlyUser
            // 
            this.btnReadonlyUser.AccessibleDescription = "00";
            this.btnReadonlyUser.Location = new System.Drawing.Point(3, 121);
            this.btnReadonlyUser.Name = "btnReadonlyUser";
            this.btnReadonlyUser.Size = new System.Drawing.Size(133, 23);
            this.btnReadonlyUser.TabIndex = 5;
            this.btnReadonlyUser.Tag = "TitleViewPassword";
            this.btnReadonlyUser.Text = "Пароль для просмотра";
            this.btnReadonlyUser.UseVisualStyleBackColor = true;
            this.btnReadonlyUser.Click += new System.EventHandler(this.BtnReadonlyUserClick);
            // 
            // btnRegister
            // 
            this.btnRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRegister.Location = new System.Drawing.Point(3, 5);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(133, 23);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Tag = "TitleRegistration";
            this.btnRegister.Text = "Регистрация";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.BtnRegisterClick);
            // 
            // btnEditUserData
            // 
            this.btnEditUserData.AccessibleDescription = "00";
            this.btnEditUserData.Location = new System.Drawing.Point(3, 92);
            this.btnEditUserData.Name = "btnEditUserData";
            this.btnEditUserData.Size = new System.Drawing.Size(133, 23);
            this.btnEditUserData.TabIndex = 3;
            this.btnEditUserData.Tag = "TitleEditData";
            this.btnEditUserData.Text = "Редактировать данные";
            this.btnEditUserData.UseVisualStyleBackColor = true;
            this.btnEditUserData.Click += new System.EventHandler(this.BtnEditUserDataClick);
            // 
            // btnBalance
            // 
            this.btnBalance.AccessibleDescription = "00";
            this.btnBalance.Location = new System.Drawing.Point(3, 177);
            this.btnBalance.Name = "btnBalance";
            this.btnBalance.Size = new System.Drawing.Size(133, 23);
            this.btnBalance.TabIndex = 2;
            this.btnBalance.Tag = "TitleDepositDemoAccount";
            this.btnBalance.Text = "Пополнить демо-счет";
            this.btnBalance.UseVisualStyleBackColor = true;
            this.btnBalance.Click += new System.EventHandler(this.BtnBalanceClick);
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.AccessibleDescription = "00";
            this.btnChangePassword.Location = new System.Drawing.Point(3, 63);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(133, 23);
            this.btnChangePassword.TabIndex = 1;
            this.btnChangePassword.Tag = "TitleChangePassword";
            this.btnChangePassword.Text = "Сменить пароль";
            this.btnChangePassword.UseVisualStyleBackColor = true;
            this.btnChangePassword.Click += new System.EventHandler(this.BtnChangePasswordClick);
            // 
            // btnRemindPassword
            // 
            this.btnRemindPassword.AccessibleDescription = "00";
            this.btnRemindPassword.Location = new System.Drawing.Point(3, 34);
            this.btnRemindPassword.Name = "btnRemindPassword";
            this.btnRemindPassword.Size = new System.Drawing.Size(133, 23);
            this.btnRemindPassword.TabIndex = 0;
            this.btnRemindPassword.Tag = "TitleRemindPassword";
            this.btnRemindPassword.Text = "Напомнить пароль";
            this.btnRemindPassword.UseVisualStyleBackColor = true;
            this.btnRemindPassword.Click += new System.EventHandler(this.BtnRemindPasswordClick);
            // 
            // imageListGlyph
            // 
            this.imageListGlyph.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlyph.ImageStream")));
            this.imageListGlyph.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlyph.Images.SetKeyName(0, "ico refresh.png");
            // 
            // ManageAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 373);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "ManageAccountForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleTradeAccounts";
            this.Text = "Торговые счета";
            this.Load += new System.EventHandler(this.ManageAccountFormLoad);
            this.panelBottom.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.contextMenuStripUpdate.ResumeLayout(false);
            this.panelBorder.ResumeLayout(false);
            this.panelBorder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelectedAccount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ImageList imageListGlyph;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.Button btnRemindPassword;
        private System.Windows.Forms.Button btnBalance;
        private System.Windows.Forms.DataGridView gridSelectedAccount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripUpdate;
        private System.Windows.Forms.ToolStripMenuItem menuitemUpdate;
        private FastGrid.FastGrid gridAllAccounts;
        private System.Windows.Forms.Panel panelBorder;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnEditUserData;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnReadonlyUser;
    }
}