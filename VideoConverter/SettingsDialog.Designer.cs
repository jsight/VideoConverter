namespace VideoConverter
{
    partial class SettingsDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFacebookPageID = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.lblPageToken = new System.Windows.Forms.Label();
            this.textBoxPageToken = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 100;
            this.label1.Text = "Page ID:";
            // 
            // textBoxFacebookPageID
            // 
            this.textBoxFacebookPageID.Location = new System.Drawing.Point(68, 9);
            this.textBoxFacebookPageID.Name = "textBoxFacebookPageID";
            this.textBoxFacebookPageID.Size = new System.Drawing.Size(544, 20);
            this.textBoxFacebookPageID.TabIndex = 200;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(537, 125);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 400;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(456, 125);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 500;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // lblPageToken
            // 
            this.lblPageToken.AutoSize = true;
            this.lblPageToken.Location = new System.Drawing.Point(12, 44);
            this.lblPageToken.Name = "lblPageToken";
            this.lblPageToken.Size = new System.Drawing.Size(69, 13);
            this.lblPageToken.TabIndex = 501;
            this.lblPageToken.Text = "Page Token:";
            // 
            // textBoxPageToken
            // 
            this.textBoxPageToken.Location = new System.Drawing.Point(87, 41);
            this.textBoxPageToken.Name = "textBoxPageToken";
            this.textBoxPageToken.Size = new System.Drawing.Size(525, 20);
            this.textBoxPageToken.TabIndex = 502;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 160);
            this.Controls.Add(this.textBoxPageToken);
            this.Controls.Add(this.lblPageToken);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxFacebookPageID);
            this.Controls.Add(this.label1);
            this.Name = "SettingsDialog";
            this.Text = "SettingsDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFacebookPageID;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label lblPageToken;
        private System.Windows.Forms.TextBox textBoxPageToken;
    }
}