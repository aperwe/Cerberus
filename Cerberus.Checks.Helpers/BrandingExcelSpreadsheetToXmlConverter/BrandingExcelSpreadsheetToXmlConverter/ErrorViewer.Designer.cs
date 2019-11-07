namespace BrandingExcelSpreadsheetToXmlConverter
{
    partial class ErrorViewer
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
            this.listBoxErrors = new System.Windows.Forms.ListBox();
            this.labelUserMessage = new System.Windows.Forms.Label();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelListBoxCaption = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listBoxErrors
            // 
            this.listBoxErrors.FormattingEnabled = true;
            this.listBoxErrors.Location = new System.Drawing.Point(2, 95);
            this.listBoxErrors.Name = "listBoxErrors";
            this.listBoxErrors.Size = new System.Drawing.Size(506, 316);
            this.listBoxErrors.TabIndex = 0;
            this.listBoxErrors.SelectedIndexChanged += new System.EventHandler(this.listBoxErrors_SelectedIndexChanged);
            this.listBoxErrors.DoubleClick += new System.EventHandler(this.listBoxErrors_DoubleClick);
            // 
            // labelUserMessage
            // 
            this.labelUserMessage.AutoSize = true;
            this.labelUserMessage.Location = new System.Drawing.Point(2, 24);
            this.labelUserMessage.Name = "labelUserMessage";
            this.labelUserMessage.Size = new System.Drawing.Size(35, 13);
            this.labelUserMessage.TabIndex = 1;
            this.labelUserMessage.Text = "label1";
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(515, 95);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(92, 35);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(515, 160);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(92, 37);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.AddExtension = false;
            this.saveFileDialog1.Filter = "XML files|*.xml";
            this.saveFileDialog1.Title = "Save branding XML file";
            // 
            // labelListBoxCaption
            // 
            this.labelListBoxCaption.AutoSize = true;
            this.labelListBoxCaption.Location = new System.Drawing.Point(2, 76);
            this.labelListBoxCaption.Name = "labelListBoxCaption";
            this.labelListBoxCaption.Size = new System.Drawing.Size(172, 13);
            this.labelListBoxCaption.TabIndex = 4;
            this.labelListBoxCaption.Text = "Validation errors in the spreadsheet";
            // 
            // ErrorViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 429);
            this.Controls.Add(this.labelListBoxCaption);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.labelUserMessage);
            this.Controls.Add(this.listBoxErrors);
            this.Name = "ErrorViewer";
            this.Text = "ErrorViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxErrors;
        private System.Windows.Forms.Label labelUserMessage;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelListBoxCaption;
    }
}