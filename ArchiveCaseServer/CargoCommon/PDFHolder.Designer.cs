namespace L3.Cargo.Common
{
    partial class PDFHolder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFHolder));
            this.pdfWindowLeft = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.pdfWindowLeft)).BeginInit();
            this.SuspendLayout();
            // 
            // pdfWindowLeft
            // 
            this.pdfWindowLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfWindowLeft.Enabled = true;
            this.pdfWindowLeft.Location = new System.Drawing.Point(0, 0);
            this.pdfWindowLeft.Name = "pdfWindowLeft";
            this.pdfWindowLeft.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("pdfWindowLeft.OcxState")));
            this.pdfWindowLeft.Size = new System.Drawing.Size(622, 574);
            this.pdfWindowLeft.TabIndex = 1;
            // 
            // PDFHolder1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.pdfWindowLeft);
            this.Name = "PDFHolder";
            this.Size = new System.Drawing.Size(622, 574);
            ((System.ComponentModel.ISupportInitialize)(this.pdfWindowLeft)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxAcroPDFLib.AxAcroPDF pdfWindowLeft;
    }
}
