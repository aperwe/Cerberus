using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    public partial class ErrorViewer : Form
    {
        public ErrorViewer()
        {
            InitializeComponent();
        }
        private void InitializeListBox(ValidationError[] errors)
        {
            listBoxErrors.DataSource = errors;
        }

        private void listBoxErrors_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxErrors_DoubleClick(sender, e);
        }

        private void listBoxErrors_DoubleClick(object sender, EventArgs e)
        {
            var listBox = (ListBox)sender;
            var selected = listBox.SelectedItem as ValidationError;
            if (selected != null)
            {
                selected.Location.Goto();
            }
        }

        internal void SetUI(string userMessage, bool exportButtonEnabled, ValidationError[] errors)
        {
            labelUserMessage.Text = userMessage;
            buttonExport.Enabled = exportButtonEnabled;
            if (errors != null)
            {
                InitializeListBox(errors);
            } 
        }
        public string SaveXMLPath { get; private set; }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            var result = this.saveFileDialog1.ShowDialog();
            
            this.DialogResult = result;
            if (result == DialogResult.OK)
            {
                SaveXMLPath = this.saveFileDialog1.FileName;
            }
            this.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            var form = (Form)((Button)sender).Parent;
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }

       

       
    }
}
