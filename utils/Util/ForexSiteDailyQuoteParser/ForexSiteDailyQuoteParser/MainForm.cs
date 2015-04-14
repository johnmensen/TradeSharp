using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ForexSiteDailyQuoteParser.CommonClass;
using ForexSiteDailyQuoteParser.Contract;
using ForexSiteDailyQuoteParser.Formatters;

namespace ForexSiteDailyQuoteParser
{
    public partial class MainForm : Form
    {
        private IQuoteParser FirstParser { get; set; }
        private IQuoteParser SecondParser { get; set; }

        private string[] PathFirstParser { get; set; }
        private string[] PathSecondParser { get; set; }

        /// <summary>
        /// Результирующий массив, который будем сохранять в файл
        /// </summary>
        private string[] ListStringMergedQuotes { get; set; }

        QuoteParseManager QuoteParseManager { get; set; }

        private const string OutputFolderName = "OutputFolder";
        private string pathToOutputFolder = string.Empty;
        private string PathToOutputFolder
        {
            get { return pathToOutputFolder; }
            set 
            {
                pathToOutputFolder = value;
                txtSelectedOutputFolder.Text = value;
            }
        }
        private string SolutionFolder { get; set; }

        public MainForm()
        {
            InitializeComponent();

            var solutionFolder = Directory.GetParent(Environment.CurrentDirectory).Parent;
            SolutionFolder = solutionFolder == null ? "" : solutionFolder.FullName;
            PathToOutputFolder = solutionFolder == null ? "" : Path.Combine(solutionFolder.FullName, OutputFolderName);

            InitDropDownListFormat();

            fileDialog.Multiselect = true;
            fileDialog.InitialDirectory = SolutionFolder;
        }

        private void BtnSelectResourceFileClick(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                PathFirstParser = fileDialog.FileNames;
                listResourceFile.Items.Clear();
                listResourceFile.Items.AddRange(PathFirstParser);
            }
        }

        private void BtnSelectOutputFileClick(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                PathSecondParser = fileDialog.FileNames;
                listOutputFile.Items.Clear();
                listOutputFile.Items.AddRange(PathSecondParser);
            }
        }

        private void BtnSelectOutputFolderClick(object sender, EventArgs e)
        {
            var dialogResult = OutputFolderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                PathToOutputFolder = OutputFolderBrowserDialog.SelectedPath;
        }

        private void BtnParseClick(object sender, EventArgs e)
        {
            if (PathFirstParser == null || PathFirstParser.Length == 0) return;
            if (PathSecondParser == null || PathSecondParser.Length == 0) return;

            listErrors.Items.Clear();
            listOutputFormate.Items.Clear();
            listResourceFormate.Items.Clear();

            FirstParser = ddlResourceFormat.SelectedItem as IQuoteParser;
            SecondParser = ddlOutputFormat.SelectedItem as IQuoteParser;
            QuoteParseManager = new QuoteParseManager(FirstParser, SecondParser);

            List<string> resourseSumbol;
            List<string> outputSumbol;
            var errorList = QuoteParseManager.Parse(PathFirstParser.First(), PathSecondParser.First(), out resourseSumbol, out outputSumbol);

            listErrors.Items.AddRange(errorList.ToArray());
            listOutputFormate.Items.AddRange(outputSumbol.ToArray());
            listResourceFormate.Items.AddRange(resourseSumbol.ToArray());

            tabMain.SelectTab("tabParsing");
        }

        private void BtnMargeClick(object sender, EventArgs e)
        {
            listPreview.Items.Clear();

            var listMergedQuote = QuoteParseManager.Merge(listResultContent.Items.Cast<string>().ToList());

            if (rbtnFirstParserSaveFormat.Checked)
                ListStringMergedQuotes = SecondParser.QuoteListToString(listMergedQuote).ToArray();
            if (rbtnSecondParserSaveFormat.Checked)
                ListStringMergedQuotes = FirstParser.QuoteListToString(listMergedQuote).ToArray();

            listPreview.Items.AddRange(ListStringMergedQuotes);
        }

        private void ListResourceFormateSelectedIndexChanged(object sender, EventArgs e)
        {
            var senderListBox = sender as ListBox;
            if (senderListBox == null) return;

            var selectedItem = (string) senderListBox.SelectedItem;

            if (selectedItem != null && listOutputFormate.Items.Contains(selectedItem)
                && listResourceFormate.Items.Contains(selectedItem)
                && !listResultContent.Items.Contains(selectedItem)) listResultContent.Items.Add(selectedItem);
        }

        private void BtnClearListResultContentClick(object sender, EventArgs e)
        {
            listResultContent.Items.Clear();
        }

        private void BtnSaveNewFileClick(object sender, EventArgs e)
        {
            var newFileName = Path.Combine(PathToOutputFolder, txtbxNewFileName.Text + ".txt");
            QuoteParseManager.SaveNewFile(newFileName, ListStringMergedQuotes);
        }

        private void InitDropDownListFormat()
        {
            ddlResourceFormat.Items.Add(new ForexiteParseFormat());
            ddlResourceFormat.Items.Add(new TradeSharpParseFormat());
            ddlResourceFormat.Items.Add(new FiboForexParseFormat());
            ddlOutputFormat.Items.Add(new ForexiteParseFormat());
            ddlOutputFormat.Items.Add(new TradeSharpParseFormat());
            ddlOutputFormat.Items.Add(new FiboForexParseFormat());

            ddlResourceFormat.SelectedIndex = 0;
            ddlOutputFormat.SelectedIndex = 1;
        }
    }
}