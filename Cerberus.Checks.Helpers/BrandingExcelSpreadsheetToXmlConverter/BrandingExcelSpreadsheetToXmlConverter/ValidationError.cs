using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    class ValidationError
    {
        public ValidationError(string description, ILocation location)
        {
            Description = description;
            Location = location;
        }
        public string Description;
        public ILocation
            Location;

        public override string ToString()
        {
            return Description;
        }
    }
}
