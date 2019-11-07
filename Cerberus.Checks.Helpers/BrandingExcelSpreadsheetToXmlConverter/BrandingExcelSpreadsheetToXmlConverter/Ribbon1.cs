using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    public partial class Ribbon1 : OfficeRibbon
    {
        public Ribbon1()
        {
            InitializeComponent();
        }

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void buttonValidateAndExport_Click(object sender, RibbonControlEventArgs e)
        {

            try
            {
                //ExecuteOnRange((Range)BrandingExcelSpreadsheetToXmlConverter.Globals.ThisAddIn.Application.Selection, true);
                ExecuteOnRange(((Worksheet)BrandingExcelSpreadsheetToXmlConverter.Globals.ThisAddIn.Application.ActiveWorkbook.ActiveSheet).UsedRange, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical errors when processing branding names in spreadsheet:\n" + ex.Message);
            }
        }

        private static void ExecuteOnRange(Range range, bool export)
        {
            var parser = new ExcelParser();
            parser.ParseExcelRange(range);
            var errors = parser.ValidateRange();

            var form = new ErrorViewer();

            form.SetUI(
                errors.Length > 0 ?
                    "Validation errors found. Click each item to go to the invalid cell in Excel.\nIf you want to proceed to Export anyway, click the button."
                    :
                    "Click Export to proceed with saving the XML file.",
                true,
                errors);
            form.FormClosed += (sender, eventArgs) =>
                {
                    // check if we should export
                    if (!String.IsNullOrEmpty(form.SaveXMLPath))
                    {
                        parser.ExportToXML(form.SaveXMLPath);
                    }

                };
            form.Show();
        }




    }
}
