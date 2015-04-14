using System;
using System.ComponentModel;
using System.Windows.Forms;
using Candlechart;

namespace TradeSharp.Client.Forms
{
    partial class ChartForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartForm));
            this.chart = new Candlechart.CandleChartControl();
            this.panesTabCtrl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // chart
            // 
            this.chart.ActiveChartTool = Candlechart.CandleChartControl.ChartTool.Cursor;
            this.chart.AdjustObjectColorsOnCreation = true;
            this.chart.AllowDrop = true;
            this.chart.CurrentTemplateName = null;
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.EnableExtendedVisualStyles = false;
            this.chart.ExtraTitle = null;
            this.chart.Location = new System.Drawing.Point(0, 0);
            this.chart.Name = "chart";
            this.chart.Size = new System.Drawing.Size(326, 270);
            this.chart.Symbol = "";
            this.chart.SynchronizationEnabled = false;
            this.chart.TabIndex = 0;
            this.chart.UniqueId = null;
            // 
            // panesTabCtrl
            // 
            this.panesTabCtrl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panesTabCtrl.Location = new System.Drawing.Point(0, 270);
            this.panesTabCtrl.Name = "panesTabCtrl";
            this.panesTabCtrl.SelectedIndex = 0;
            this.panesTabCtrl.Size = new System.Drawing.Size(326, 20);
            this.panesTabCtrl.TabIndex = 1;
            this.panesTabCtrl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TabCtrlMouseDoubleClick);
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 290);
            this.Controls.Add(this.chart);
            this.Controls.Add(this.panesTabCtrl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChartForm";
            this.Text = "StockSymbol";
            this.Activated += new System.EventHandler(this.MdiChildActivated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChartFormFormClosing);
            this.Load += new System.EventHandler(this.ChartFormLoad);
            this.ResizeEnd += new System.EventHandler(this.ChartFormResizeEnd);
            this.Move += new System.EventHandler(this.ChartFormMove);
            this.Resize += new System.EventHandler(this.ChartFormResize);
            this.ResumeLayout(false);

        }

        #endregion

        public CandleChartControl chart;
        private System.Windows.Forms.Panel panelStatus;

    }
}