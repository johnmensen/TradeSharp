namespace TradeSharp.Client.BL.Script
{
    partial class TriggerSetupDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TriggerSetupDialog));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageQuote = new System.Windows.Forms.TabPage();
            this.cbQuote = new TradeSharp.Util.CheckedListBoxColorableDraggable();
            this.panelQuoteTop = new System.Windows.Forms.Panel();
            this.lblQuote = new System.Windows.Forms.Label();
            this.tabPageOrder = new System.Windows.Forms.TabPage();
            this.cbOrder = new TradeSharp.Util.CheckedListBoxColorableDraggable();
            this.tabPageFormula = new System.Windows.Forms.TabPage();
            this.rtbVariables = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFormula = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageEmpty = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageQuote.SuspendLayout();
            this.panelQuoteTop.SuspendLayout();
            this.tabPageOrder.SuspendLayout();
            this.tabPageFormula.SuspendLayout();
            this.tabPageEmpty.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 231);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(284, 31);
            this.panelBottom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(115, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(3, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageEmpty);
            this.tabControl.Controls.Add(this.tabPageQuote);
            this.tabControl.Controls.Add(this.tabPageOrder);
            this.tabControl.Controls.Add(this.tabPageFormula);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(284, 231);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageQuote
            // 
            this.tabPageQuote.Controls.Add(this.cbQuote);
            this.tabPageQuote.Controls.Add(this.panelQuoteTop);
            this.tabPageQuote.Location = new System.Drawing.Point(4, 22);
            this.tabPageQuote.Name = "tabPageQuote";
            this.tabPageQuote.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageQuote.Size = new System.Drawing.Size(276, 205);
            this.tabPageQuote.TabIndex = 0;
            this.tabPageQuote.Text = "Котировки";
            this.tabPageQuote.UseVisualStyleBackColor = true;
            // 
            // cbQuote
            // 
            this.cbQuote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbQuote.DraggingEnabled = false;
            this.cbQuote.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbQuote.FormattingEnabled = true;
            this.cbQuote.Location = new System.Drawing.Point(3, 24);
            this.cbQuote.MarkPadding = 2;
            this.cbQuote.MarkSize = 9;
            this.cbQuote.Name = "cbQuote";
            this.cbQuote.RowColors = null;
            this.cbQuote.Size = new System.Drawing.Size(270, 178);
            this.cbQuote.TabIndex = 2;
            // 
            // panelQuoteTop
            // 
            this.panelQuoteTop.Controls.Add(this.lblQuote);
            this.panelQuoteTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelQuoteTop.Location = new System.Drawing.Point(3, 3);
            this.panelQuoteTop.Name = "panelQuoteTop";
            this.panelQuoteTop.Size = new System.Drawing.Size(270, 21);
            this.panelQuoteTop.TabIndex = 1;
            // 
            // lblQuote
            // 
            this.lblQuote.AutoSize = true;
            this.lblQuote.Location = new System.Drawing.Point(5, 5);
            this.lblQuote.Name = "lblQuote";
            this.lblQuote.Size = new System.Drawing.Size(39, 13);
            this.lblQuote.TabIndex = 0;
            this.lblQuote.Text = "любая";
            // 
            // tabPageOrder
            // 
            this.tabPageOrder.Controls.Add(this.cbOrder);
            this.tabPageOrder.Location = new System.Drawing.Point(4, 22);
            this.tabPageOrder.Name = "tabPageOrder";
            this.tabPageOrder.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOrder.Size = new System.Drawing.Size(276, 205);
            this.tabPageOrder.TabIndex = 1;
            this.tabPageOrder.Text = "Ордера";
            this.tabPageOrder.UseVisualStyleBackColor = true;
            // 
            // cbOrder
            // 
            this.cbOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbOrder.DraggingEnabled = false;
            this.cbOrder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbOrder.FormattingEnabled = true;
            this.cbOrder.Location = new System.Drawing.Point(3, 3);
            this.cbOrder.MarkPadding = 2;
            this.cbOrder.MarkSize = 9;
            this.cbOrder.Name = "cbOrder";
            this.cbOrder.RowColors = null;
            this.cbOrder.Size = new System.Drawing.Size(270, 199);
            this.cbOrder.TabIndex = 3;
            // 
            // tabPageFormula
            // 
            this.tabPageFormula.Controls.Add(this.rtbVariables);
            this.tabPageFormula.Controls.Add(this.label2);
            this.tabPageFormula.Controls.Add(this.tbFormula);
            this.tabPageFormula.Controls.Add(this.label1);
            this.tabPageFormula.Location = new System.Drawing.Point(4, 22);
            this.tabPageFormula.Name = "tabPageFormula";
            this.tabPageFormula.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFormula.Size = new System.Drawing.Size(276, 205);
            this.tabPageFormula.TabIndex = 2;
            this.tabPageFormula.Text = "Формула";
            this.tabPageFormula.UseVisualStyleBackColor = true;
            // 
            // rtbVariables
            // 
            this.rtbVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbVariables.Enabled = false;
            this.rtbVariables.Location = new System.Drawing.Point(3, 61);
            this.rtbVariables.Name = "rtbVariables";
            this.rtbVariables.Size = new System.Drawing.Size(270, 141);
            this.rtbVariables.TabIndex = 3;
            this.rtbVariables.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(3, 42);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(3);
            this.label2.Size = new System.Drawing.Size(79, 19);
            this.label2.TabIndex = 2;
            this.label2.Text = "Переменные";
            // 
            // tbFormula
            // 
            this.tbFormula.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbFormula.Location = new System.Drawing.Point(3, 22);
            this.tbFormula.Name = "tbFormula";
            this.tbFormula.Size = new System.Drawing.Size(270, 20);
            this.tbFormula.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(3);
            this.label1.Size = new System.Drawing.Size(57, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Условие";
            // 
            // tabPageEmpty
            // 
            this.tabPageEmpty.Controls.Add(this.label3);
            this.tabPageEmpty.Location = new System.Drawing.Point(4, 22);
            this.tabPageEmpty.Name = "tabPageEmpty";
            this.tabPageEmpty.Size = new System.Drawing.Size(276, 205);
            this.tabPageEmpty.TabIndex = 3;
            this.tabPageEmpty.Text = "Нет";
            this.tabPageEmpty.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Триггер не задан";
            // 
            // TriggerSetupDialog
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "TriggerSetupDialog";
            this.Text = "Триггер";
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageQuote.ResumeLayout(false);
            this.panelQuoteTop.ResumeLayout(false);
            this.panelQuoteTop.PerformLayout();
            this.tabPageOrder.ResumeLayout(false);
            this.tabPageFormula.ResumeLayout(false);
            this.tabPageFormula.PerformLayout();
            this.tabPageEmpty.ResumeLayout(false);
            this.tabPageEmpty.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageQuote;
        private TradeSharp.Util.CheckedListBoxColorableDraggable cbQuote;
        private System.Windows.Forms.Panel panelQuoteTop;
        private System.Windows.Forms.Label lblQuote;
        private System.Windows.Forms.TabPage tabPageOrder;
        private System.Windows.Forms.TabPage tabPageFormula;
        private TradeSharp.Util.CheckedListBoxColorableDraggable cbOrder;
        private System.Windows.Forms.RichTextBox rtbVariables;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageEmpty;
        private System.Windows.Forms.Label label3;
    }
}