using System;
using System.Windows.Forms;
using Candlechart.Chart;
using Candlechart.Core;

namespace Candlechart
{
    partial class CandleChartControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CandleChartControl));
            this.chart = new Candlechart.Chart.ChartControl();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemObjectsDlg = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemScaleIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemScaleOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemIndicatorsDlg = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemNewOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSaveImage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.menucategorySignal = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemForecastForSite = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemPublishForecast = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSignalTextMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemOrdersIndi = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSyncQuotes = new System.Windows.Forms.ToolStripMenuItem();
            this.chartScrollBar = new Candlechart.Core.ChartScrollBar();
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart
            // 
            this.chart.BackColor = System.Drawing.SystemColors.Control;
            this.chart.CacheMode = Candlechart.Chart.ChartCacheMode.NoCache;
            this.chart.ContextMenuStrip = this.contextMenu;
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.EndTime = new System.DateTime(((long)(0)));
            this.chart.ItemTextBox = null;
            this.chart.Location = new System.Drawing.Point(0, 0);
            this.chart.Name = "chart";
            this.chart.NeedDealData = true;
            this.chart.Owner = null;
            this.chart.Size = new System.Drawing.Size(599, 479);
            this.chart.StartTime = new System.DateTime(((long)(0)));
            this.chart.Symbol = "";
            this.chart.TabIndex = 1;
            this.chart.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ChartKeyPress);
            this.chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ChartMouseDown);
            this.chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ChartMouseMove);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemObjectsDlg,
            this.menuitemScaleIn,
            this.menuitemScaleOut,
            this.menuitemIndicatorsDlg,
            this.menuitemNewOrder,
            this.menuitemSaveImage,
            this.menuitemGoTo,
            this.menuitemScripts,
            this.menucategorySignal,
            this.menuitemOrdersIndi,
            this.menuitemSyncQuotes});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(182, 268);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuOpening);
            // 
            // menuitemObjectsDlg
            // 
            this.menuitemObjectsDlg.Name = "menuitemObjectsDlg";
            this.menuitemObjectsDlg.Size = new System.Drawing.Size(181, 22);
            this.menuitemObjectsDlg.Tag = "TitleObjectsMenu";
            this.menuitemObjectsDlg.Text = "Объекты...";
            this.menuitemObjectsDlg.Click += new System.EventHandler(this.OnMenuItemObjects);
            // 
            // menuitemScaleIn
            // 
            this.menuitemScaleIn.Name = "menuitemScaleIn";
            this.menuitemScaleIn.Size = new System.Drawing.Size(181, 22);
            this.menuitemScaleIn.Tag = "TitleScalePlus";
            this.menuitemScaleIn.Text = "Масштаб +";
            this.menuitemScaleIn.Click += new System.EventHandler(this.MenuitemScaleInClick);
            // 
            // menuitemScaleOut
            // 
            this.menuitemScaleOut.Name = "menuitemScaleOut";
            this.menuitemScaleOut.Size = new System.Drawing.Size(181, 22);
            this.menuitemScaleOut.Tag = "TitleScaleMinus";
            this.menuitemScaleOut.Text = "Масштаб -";
            this.menuitemScaleOut.Click += new System.EventHandler(this.MenuitemScaleOutClick);
            // 
            // menuitemIndicatorsDlg
            // 
            this.menuitemIndicatorsDlg.Name = "menuitemIndicatorsDlg";
            this.menuitemIndicatorsDlg.Size = new System.Drawing.Size(181, 22);
            this.menuitemIndicatorsDlg.Tag = "TitleIndicatorsMenu";
            this.menuitemIndicatorsDlg.Text = "Индикаторы...";
            this.menuitemIndicatorsDlg.Click += new System.EventHandler(this.MenuitemIndicatorsDlgClick);
            // 
            // menuitemNewOrder
            // 
            this.menuitemNewOrder.Name = "menuitemNewOrder";
            this.menuitemNewOrder.Size = new System.Drawing.Size(181, 22);
            this.menuitemNewOrder.Tag = "TitleNewOrderMenu";
            this.menuitemNewOrder.Text = "Новый ордер...";
            this.menuitemNewOrder.Click += new System.EventHandler(this.MenuitemNewOrderClick);
            // 
            // menuitemSaveImage
            // 
            this.menuitemSaveImage.Name = "menuitemSaveImage";
            this.menuitemSaveImage.Size = new System.Drawing.Size(181, 22);
            this.menuitemSaveImage.Tag = "TitleSaveChartMenu";
            this.menuitemSaveImage.Text = "Сохранить график...";
            this.menuitemSaveImage.Click += new System.EventHandler(this.MenuitemSaveImageClick);
            // 
            // menuitemGoTo
            // 
            this.menuitemGoTo.Name = "menuitemGoTo";
            this.menuitemGoTo.Size = new System.Drawing.Size(181, 22);
            this.menuitemGoTo.Tag = "TitleJumpMenu";
            this.menuitemGoTo.Text = "Переход...";
            this.menuitemGoTo.Click += new System.EventHandler(this.MenuitemGoToClick);
            // 
            // menuitemScripts
            // 
            this.menuitemScripts.Name = "menuitemScripts";
            this.menuitemScripts.Size = new System.Drawing.Size(181, 22);
            this.menuitemScripts.Tag = "TitleScripts";
            this.menuitemScripts.Text = "Скрипты";
            this.menuitemScripts.Visible = false;
            // 
            // menucategorySignal
            // 
            this.menucategorySignal.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemForecastForSite,
            this.menuitemPublishForecast,
            this.menuitemSignalTextMessage});
            this.menucategorySignal.Name = "menucategorySignal";
            this.menucategorySignal.Size = new System.Drawing.Size(181, 22);
            this.menucategorySignal.Tag = "TitleSignals";
            this.menucategorySignal.Text = "Сигналы";
            // 
            // menuitemForecastForSite
            // 
            this.menuitemForecastForSite.Name = "menuitemForecastForSite";
            this.menuitemForecastForSite.Size = new System.Drawing.Size(168, 22);
            this.menuitemForecastForSite.Tag = "TitleForecastOnSiteMenu";
            this.menuitemForecastForSite.Text = "Прогноз на сайт...";
            this.menuitemForecastForSite.Click += new System.EventHandler(this.MenuitemPublishForecastClick);
            // 
            // menuitemPublishForecast
            // 
            this.menuitemPublishForecast.Name = "menuitemPublishForecast";
            this.menuitemPublishForecast.Size = new System.Drawing.Size(168, 22);
            this.menuitemPublishForecast.Tag = "TitleForecastMenu";
            this.menuitemPublishForecast.Text = "Прогноз...";
            this.menuitemPublishForecast.Click += new System.EventHandler(this.MenuitemMakeTradeSignalClick);
            // 
            // menuitemSignalTextMessage
            // 
            this.menuitemSignalTextMessage.Name = "menuitemSignalTextMessage";
            this.menuitemSignalTextMessage.Size = new System.Drawing.Size(168, 22);
            this.menuitemSignalTextMessage.Tag = "TitleMessageMenu";
            this.menuitemSignalTextMessage.Text = "Сообщение...";
            this.menuitemSignalTextMessage.Click += new System.EventHandler(this.MenuitemSignalTextMessageClick);
            // 
            // menuitemOrdersIndi
            // 
            this.menuitemOrdersIndi.Name = "menuitemOrdersIndi";
            this.menuitemOrdersIndi.Size = new System.Drawing.Size(181, 22);
            this.menuitemOrdersIndi.Tag = "TitleOrdersMenu";
            this.menuitemOrdersIndi.Text = "Ордера...";
            this.menuitemOrdersIndi.Click += new System.EventHandler(this.IndicatorOrdersMenuClick);
            // 
            // menuitemSyncQuotes
            // 
            this.menuitemSyncQuotes.Name = "menuitemSyncQuotes";
            this.menuitemSyncQuotes.Size = new System.Drawing.Size(181, 22);
            this.menuitemSyncQuotes.Tag = "TitleSynchronizeQuotesMenuShort";
            this.menuitemSyncQuotes.Text = "Синхр. котировки...";
            this.menuitemSyncQuotes.Click += new System.EventHandler(this.MenuitemSyncQuotesClick);
            // 
            // chartScrollBar
            // 
            this.chartScrollBar.Chart = this.chart;
            this.chartScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.chartScrollBar.LargeChange = 99;
            this.chartScrollBar.Location = new System.Drawing.Point(0, 479);
            this.chartScrollBar.Name = "chartScrollBar";
            this.chartScrollBar.Size = new System.Drawing.Size(599, 17);
            this.chartScrollBar.TabIndex = 0;
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "ico_table_view.png");
            this.imageListGlypth.Images.SetKeyName(1, "icon_eur.png");
            this.imageListGlypth.Images.SetKeyName(2, "gear_16.png");
            this.imageListGlypth.Images.SetKeyName(3, "ico_save.png");
            this.imageListGlypth.Images.SetKeyName(4, "ico_script.png");
            this.imageListGlypth.Images.SetKeyName(5, "ico_signal.png");
            this.imageListGlypth.Images.SetKeyName(6, "ico_zoom_in.png");
            this.imageListGlypth.Images.SetKeyName(7, "ico_zoom_out.png");
            this.imageListGlypth.Images.SetKeyName(8, "ico_indicators.png");
            this.imageListGlypth.Images.SetKeyName(9, "ico_red_down_arrow.png");
            this.imageListGlypth.Images.SetKeyName(10, "ico_find.png");
            this.imageListGlypth.Images.SetKeyName(11, "ico_patch.png");
            this.imageListGlypth.Images.SetKeyName(12, "ico delete.png");
            // 
            // CandleChartControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chart);
            this.Controls.Add(this.chartScrollBar);
            this.Name = "CandleChartControl";
            this.Size = new System.Drawing.Size(599, 496);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        
        #endregion

        private ChartScrollBar chartScrollBar;
        public ChartControl chart;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuitemObjectsDlg;
        private System.Windows.Forms.ToolStripMenuItem menuitemScaleIn;
        private System.Windows.Forms.ToolStripMenuItem menuitemScaleOut;
        private System.Windows.Forms.ToolStripMenuItem menuitemIndicatorsDlg;
        private System.Windows.Forms.ToolStripMenuItem menuitemNewOrder;
        private System.Windows.Forms.ToolStripMenuItem menuitemSaveImage;
        private ToolStripMenuItem menuitemGoTo;
        private ToolStripMenuItem menuitemScripts;
        private ToolStripMenuItem menucategorySignal;
        private ToolStripMenuItem menuitemForecastForSite;
        private ToolStripMenuItem menuitemPublishForecast;
        private ToolStripMenuItem menuitemSignalTextMessage;
        private ToolStripMenuItem menuitemOrdersIndi;
        private ImageList imageListGlypth;
        private ToolStripMenuItem menuitemSyncQuotes;
    }
}
