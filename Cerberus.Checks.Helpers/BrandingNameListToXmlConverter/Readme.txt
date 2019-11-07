This tool converts a flat list of product names into an XML format
as defined by BrandingNamesMatrix.xsd schema.

It is meant to be used to initially seed an XML file with a list of English 
product names delivered from a PM. The actual translation values will be populated
manually by the PM.

Place a list of product names in ListOfENBrandingNames.txt and run the program to 
create a new XML file; then place the file in the correct location for the BrandingTranslationsCheck to pick up.

Project contains the schema itself (BrandingNamesMatrix.xsd) and also an InfoPath
form used to edit the XML file (to add additonal product names and translations):
BrandingNamesMatrixForm.xsn


