
namespace SAM.Analytical.Revit.Addin.Forms
{
    partial class SimulateForm
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
            this.Button_Cancel = new System.Windows.Forms.Button();
            this.Button_OK = new System.Windows.Forms.Button();
            this.Label_OutputDirectory = new System.Windows.Forms.Label();
            this.TextBox_OutputDirectory = new System.Windows.Forms.TextBox();
            this.Button_OutputDirectory = new System.Windows.Forms.Button();
            this.Button_WeatherData = new System.Windows.Forms.Button();
            this.TextBox_WeatherData = new System.Windows.Forms.TextBox();
            this.Label_WeatherData = new System.Windows.Forms.Label();
            this.ComboBox_SolarCalculationMethod = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CheckBox_UnmetHours = new System.Windows.Forms.CheckBox();
            this.TextBox_ProjectName = new System.Windows.Forms.TextBox();
            this.Label_ProjectName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Cancel.Location = new System.Drawing.Point(495, 313);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.Button_Cancel.TabIndex = 0;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = true;
            this.Button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
            // 
            // Button_OK
            // 
            this.Button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_OK.Location = new System.Drawing.Point(414, 313);
            this.Button_OK.Name = "Button_OK";
            this.Button_OK.Size = new System.Drawing.Size(75, 28);
            this.Button_OK.TabIndex = 1;
            this.Button_OK.Text = "OK";
            this.Button_OK.UseVisualStyleBackColor = true;
            this.Button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // Label_OutputDirectory
            // 
            this.Label_OutputDirectory.AutoSize = true;
            this.Label_OutputDirectory.Location = new System.Drawing.Point(12, 73);
            this.Label_OutputDirectory.Name = "Label_OutputDirectory";
            this.Label_OutputDirectory.Size = new System.Drawing.Size(114, 17);
            this.Label_OutputDirectory.TabIndex = 2;
            this.Label_OutputDirectory.Text = "Output directory:";
            // 
            // TextBox_OutputDirectory
            // 
            this.TextBox_OutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_OutputDirectory.Location = new System.Drawing.Point(12, 96);
            this.TextBox_OutputDirectory.Name = "TextBox_OutputDirectory";
            this.TextBox_OutputDirectory.Size = new System.Drawing.Size(512, 22);
            this.TextBox_OutputDirectory.TabIndex = 3;
            // 
            // Button_OutputDirectory
            // 
            this.Button_OutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_OutputDirectory.Location = new System.Drawing.Point(530, 96);
            this.Button_OutputDirectory.Name = "Button_OutputDirectory";
            this.Button_OutputDirectory.Size = new System.Drawing.Size(40, 23);
            this.Button_OutputDirectory.TabIndex = 4;
            this.Button_OutputDirectory.Text = "...";
            this.Button_OutputDirectory.UseVisualStyleBackColor = true;
            this.Button_OutputDirectory.Click += new System.EventHandler(this.Button_OutputDirectory_Click);
            // 
            // Button_WeatherData
            // 
            this.Button_WeatherData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_WeatherData.Location = new System.Drawing.Point(530, 161);
            this.Button_WeatherData.Name = "Button_WeatherData";
            this.Button_WeatherData.Size = new System.Drawing.Size(40, 23);
            this.Button_WeatherData.TabIndex = 7;
            this.Button_WeatherData.Text = "...";
            this.Button_WeatherData.UseVisualStyleBackColor = true;
            this.Button_WeatherData.Click += new System.EventHandler(this.Button_WeatherData_Click);
            // 
            // TextBox_WeatherData
            // 
            this.TextBox_WeatherData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_WeatherData.Location = new System.Drawing.Point(12, 161);
            this.TextBox_WeatherData.Name = "TextBox_WeatherData";
            this.TextBox_WeatherData.ReadOnly = true;
            this.TextBox_WeatherData.Size = new System.Drawing.Size(512, 22);
            this.TextBox_WeatherData.TabIndex = 6;
            // 
            // Label_WeatherData
            // 
            this.Label_WeatherData.AutoSize = true;
            this.Label_WeatherData.Location = new System.Drawing.Point(12, 138);
            this.Label_WeatherData.Name = "Label_WeatherData";
            this.Label_WeatherData.Size = new System.Drawing.Size(100, 17);
            this.Label_WeatherData.TabIndex = 5;
            this.Label_WeatherData.Text = "Weather Data:";
            // 
            // ComboBox_SolarCalculationMethod
            // 
            this.ComboBox_SolarCalculationMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox_SolarCalculationMethod.FormattingEnabled = true;
            this.ComboBox_SolarCalculationMethod.Location = new System.Drawing.Point(12, 226);
            this.ComboBox_SolarCalculationMethod.Name = "ComboBox_SolarCalculationMethod";
            this.ComboBox_SolarCalculationMethod.Size = new System.Drawing.Size(214, 24);
            this.ComboBox_SolarCalculationMethod.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 203);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Solar Calculation Method:";
            // 
            // CheckBox_UnmetHours
            // 
            this.CheckBox_UnmetHours.AutoSize = true;
            this.CheckBox_UnmetHours.Location = new System.Drawing.Point(293, 228);
            this.CheckBox_UnmetHours.Name = "CheckBox_UnmetHours";
            this.CheckBox_UnmetHours.Size = new System.Drawing.Size(113, 21);
            this.CheckBox_UnmetHours.TabIndex = 11;
            this.CheckBox_UnmetHours.Text = "Unmet Hours";
            this.CheckBox_UnmetHours.UseVisualStyleBackColor = true;
            // 
            // TextBox_ProjectName
            // 
            this.TextBox_ProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_ProjectName.Location = new System.Drawing.Point(12, 31);
            this.TextBox_ProjectName.Name = "TextBox_ProjectName";
            this.TextBox_ProjectName.Size = new System.Drawing.Size(512, 22);
            this.TextBox_ProjectName.TabIndex = 13;
            // 
            // Label_ProjectName
            // 
            this.Label_ProjectName.AutoSize = true;
            this.Label_ProjectName.Location = new System.Drawing.Point(12, 8);
            this.Label_ProjectName.Name = "Label_ProjectName";
            this.Label_ProjectName.Size = new System.Drawing.Size(97, 17);
            this.Label_ProjectName.TabIndex = 12;
            this.Label_ProjectName.Text = "Project Name:";
            // 
            // SimulateForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(582, 353);
            this.Controls.Add(this.TextBox_ProjectName);
            this.Controls.Add(this.Label_ProjectName);
            this.Controls.Add(this.CheckBox_UnmetHours);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ComboBox_SolarCalculationMethod);
            this.Controls.Add(this.Button_WeatherData);
            this.Controls.Add(this.TextBox_WeatherData);
            this.Controls.Add(this.Label_WeatherData);
            this.Controls.Add(this.Button_OutputDirectory);
            this.Controls.Add(this.TextBox_OutputDirectory);
            this.Controls.Add(this.Label_OutputDirectory);
            this.Controls.Add(this.Button_OK);
            this.Controls.Add(this.Button_Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimulateForm";
            this.ShowInTaskbar = false;
            this.Text = "Simulate";
            this.Load += new System.EventHandler(this.SimulateForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Button_Cancel;
        private System.Windows.Forms.Button Button_OK;
        private System.Windows.Forms.Label Label_OutputDirectory;
        private System.Windows.Forms.TextBox TextBox_OutputDirectory;
        private System.Windows.Forms.Button Button_OutputDirectory;
        private System.Windows.Forms.Button Button_WeatherData;
        private System.Windows.Forms.TextBox TextBox_WeatherData;
        private System.Windows.Forms.Label Label_WeatherData;
        private System.Windows.Forms.ComboBox ComboBox_SolarCalculationMethod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CheckBox_UnmetHours;
        private System.Windows.Forms.TextBox TextBox_ProjectName;
        private System.Windows.Forms.Label Label_ProjectName;
    }
}