namespace VideoConverter
{
    partial class AudioForm
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
            this.label6 = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.btnSelectInput = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.dateTimePickerServiceDate = new System.Windows.Forms.DateTimePicker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.txtServiceName = new System.Windows.Forms.TextBox();
            this.lblProgressText = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPreacher = new System.Windows.Forms.TextBox();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 93);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 63;
            this.label6.Text = "Reference:";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(3, 13);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(78, 13);
            this.lblSource.TabIndex = 43;
            this.lblSource.Text = "Input MP3 File:";
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(92, 10);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(363, 20);
            this.txtInputFile.TabIndex = 10;
            // 
            // btnSelectInput
            // 
            this.btnSelectInput.Location = new System.Drawing.Point(461, 8);
            this.btnSelectInput.Name = "btnSelectInput";
            this.btnSelectInput.Size = new System.Drawing.Size(75, 23);
            this.btnSelectInput.TabIndex = 15;
            this.btnSelectInput.Text = "Select...";
            this.btnSelectInput.UseVisualStyleBackColor = true;
            this.btnSelectInput.Click += new System.EventHandler(this.btnSelectInput_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(461, 176);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 60;
            this.btnConvert.Text = "Upload!";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // dateTimePickerServiceDate
            // 
            this.dateTimePickerServiceDate.Location = new System.Drawing.Point(92, 117);
            this.dateTimePickerServiceDate.MinDate = new System.DateTime(2013, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerServiceDate.Name = "dateTimePickerServiceDate";
            this.dateTimePickerServiceDate.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerServiceDate.TabIndex = 50;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(92, 176);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 23);
            this.progressBar1.TabIndex = 700;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 55;
            this.label2.Text = "Service Date:";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(92, 157);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(51, 13);
            this.lblProgress.TabIndex = 500;
            this.lblProgress.Text = "Progress:";
            // 
            // txtServiceName
            // 
            this.txtServiceName.Location = new System.Drawing.Point(92, 38);
            this.txtServiceName.Name = "txtServiceName";
            this.txtServiceName.Size = new System.Drawing.Size(363, 20);
            this.txtServiceName.TabIndex = 20;
            // 
            // lblProgressText
            // 
            this.lblProgressText.AutoSize = true;
            this.lblProgressText.Location = new System.Drawing.Point(149, 157);
            this.lblProgressText.Name = "lblProgressText";
            this.lblProgressText.Size = new System.Drawing.Size(87, 13);
            this.lblProgressText.TabIndex = 510;
            this.lblProgressText.Text = "Placeholder Text";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 52;
            this.label1.Text = "Service Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 65;
            this.label3.Text = "Preacher:";
            // 
            // txtPreacher
            // 
            this.txtPreacher.Location = new System.Drawing.Point(92, 64);
            this.txtPreacher.Name = "txtPreacher";
            this.txtPreacher.Size = new System.Drawing.Size(363, 20);
            this.txtPreacher.TabIndex = 30;
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(92, 90);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(363, 20);
            this.txtReference.TabIndex = 40;
            // 
            // AudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.txtPreacher);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.txtInputFile);
            this.Controls.Add(this.btnSelectInput);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.dateTimePickerServiceDate);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.txtServiceName);
            this.Controls.Add(this.lblProgressText);
            this.Controls.Add(this.label1);
            this.Name = "AudioForm";
            this.Size = new System.Drawing.Size(543, 218);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Button btnSelectInput;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.DateTimePicker dateTimePickerServiceDate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.Label lblProgressText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPreacher;
        private System.Windows.Forms.TextBox txtReference;
    }
}
