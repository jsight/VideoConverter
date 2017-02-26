using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoConverter
{
    public partial class FormMainConverter : Form
    {
        public FormMainConverter()
        {
            InitializeComponent();
            MyInit();
        }

        private void MyInit()
        {
            VideoForm videoForm = new VideoForm();
            videoForm.MainForm = this;
            AudioForm audioForm = new AudioForm();
            audioForm.MainForm = this;
            tabVideo.Controls.Add(videoForm);
            tabAudio.Controls.Add(audioForm);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                AboutBox1 aboutBox1 = new AboutBox1();
                aboutBox1.ShowDialog(this);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                VideoConverterSettings settings = new VideoConverterSettings();
                settings.ShowDialog(this);
            }
        }
    }
}
