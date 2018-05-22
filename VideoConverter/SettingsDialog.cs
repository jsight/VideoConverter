using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using fb_csharp_sdk_winforms;

namespace VideoConverter
{
    public partial class SettingsDialog : Form
    {
        private const string ExtendedPermissions = "user_about_me,publish_actions,publish_pages,manage_pages";

        public SettingsDialog()
        {
            InitializeComponent();
            textBoxFacebookPageID.Text = Properties.Settings.Default.FacebookPageID;
            textBoxPageToken.Text = Properties.Settings.Default.FacebookToken;
            updateLoginLabel();
        }

        private void updateLoginLabel()
        {
            var currentLoginID = Properties.Settings.Default.FacebookToken;
            if (currentLoginID == null || currentLoginID.Trim() == "")
            {
                //labelCurrentToken.Text = "-- Not Set, Login Required --";
            }
            else
            {
                if (currentLoginID.Length > 30)
                    currentLoginID = currentLoginID.Substring(0, 30) + "...";
                //labelCurrentToken.Text = currentLoginID;
            }
        }

        private void buttonFacebookLogin_Click(object sender, EventArgs e)
        {
            var appID = "1438195436291282";
            var loginDialog = new FacebookLoginDialog(appID, ExtendedPermissions);
            loginDialog.ShowDialog(this);
            if (loginDialog.FacebookOAuthResult != null)
                Properties.Settings.Default.FacebookToken = loginDialog.FacebookOAuthResult.AccessToken;
            updateLoginLabel();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.FacebookPageID = textBoxFacebookPageID.Text;
            Properties.Settings.Default.FacebookToken = textBoxPageToken.Text;
            Properties.Settings.Default.Save();
            Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
