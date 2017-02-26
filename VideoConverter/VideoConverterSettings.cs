using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VideoConverter.Properties;

namespace VideoConverter
{
    public partial class VideoConverterSettings : Form
    {
        public VideoConverterSettings()
        {
            InitializeComponent();
            InitMyComponents();
        }

        private void InitMyComponents()
        {
            txtUsername.Text = Settings.Default.FtpUser;
            txtPassword.Text = Settings.Default.FtpPassword;
            txtVideoManagerUrl.Text = Settings.Default.VideoManagerUrl;
            txtAudioManagerUrl.Text = Settings.Default.AudioManagerUrl;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Settings.Default.FtpUser = txtUsername.Text;
            Settings.Default.FtpPassword = txtPassword.Text;
            Settings.Default.VideoManagerUrl = txtVideoManagerUrl.Text;
            Settings.Default.AudioManagerUrl = txtAudioManagerUrl.Text;
            Settings.Default.Save();
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
