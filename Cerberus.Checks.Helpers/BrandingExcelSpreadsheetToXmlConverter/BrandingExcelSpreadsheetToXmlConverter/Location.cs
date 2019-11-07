using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    interface ILocation
    {
        void Goto();
        string Description { get; }

    }
    class ExcelLocation : ILocation
    {
        private Range cellLocation;
        public ExcelLocation(Range cell)
        {
            cellLocation = cell;
        }
        #region ILocation Members

        public void Goto()
        {
            cellLocation.Activate();
        }


        public string Description
        {
            get
            {
                return String.Format("Row: {0}, column: {1}", cellLocation.Row, cellLocation.Column);
            }
        }

        #endregion
        public override string ToString()
        {
            return Description;
        }
    }
}
