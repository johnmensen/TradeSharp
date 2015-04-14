namespace TradeSharp.Client.Forms
{
    partial class QuoteTableForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuoteTableForm));
            this.quoteTable = new TradeSharp.Client.Controls.QuoteTableControl();
            this.SuspendLayout();
            // 
            // quoteTable
            // 
            this.quoteTable.CellHeight = 18;
            this.quoteTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.quoteTable.Location = new System.Drawing.Point(0, 0);
            this.quoteTable.Name = "quoteTable";
            this.quoteTable.Size = new System.Drawing.Size(292, 266);
            this.quoteTable.TabIndex = 1;
            // 
            // QuoteTableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.quoteTable);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuoteTableForm";
            this.Text = "Котировки";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuoteTableFormFormClosing);
            this.Load += new System.EventHandler(this.QuoteTableFormLoad);
            this.ResizeEnd += new System.EventHandler(this.QuoteTableFormResizeEnd);
            this.Move += new System.EventHandler(this.QuoteTableFormMove);
            this.ResumeLayout(false);

        }

        #endregion

        private TradeSharp.Client.Controls.QuoteTableControl quoteTable;

    }
}