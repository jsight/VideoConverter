using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoConverter
{
    public partial class AudioForm : UserControl
    {
        public FormMainConverter MainForm { get; set; }
        private Dictionary<string, Dictionary<string, string>> listInfo;
        private string list;


        public AudioForm()
        {
            InitializeComponent();
            MyInit();
        }

        private void MyInit()
        {
            progressBar1.Visible = false;
            lblProgress.Visible = false;
            lblProgressText.Visible = false;
            lblProgressText.Text = "";

            SetInputsEnabledStatus(false);

            AudioManagerClient audioManagerClient = new AudioManagerClient();
            listInfo = audioManagerClient.GetListInfo();
            if (listInfo != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> listItem in listInfo)
                {
                    this.list = listItem.Key;
                    break;
                }
            }
            SetInputsEnabledStatus(true);
            txtPreacher.Text = "Pastor Frank Bailey";
        }

        private void SetInputsEnabledStatus(bool enabled)
        {
            txtInputFile.Enabled = enabled;
            txtServiceName.Enabled = enabled;
            txtReference.Enabled = enabled;
            dateTimePickerServiceDate.Enabled = enabled;
            btnSelectInput.Enabled = enabled;
            btnConvert.Enabled = enabled;
        }

        private void btnSelectInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                txtInputFile.Text = fileName;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            lblProgressText.Text = "";
            string inputFile = txtInputFile.Text;
            string serviceName = txtServiceName.Text;
            string preacher = txtPreacher.Text;
            string reference = txtReference.Text;
            DateTime serviceDate = dateTimePickerServiceDate.Value;

            if (inputFile == null || inputFile.Trim().Length == 0)
            {
                ShowMessageBox("Must select an input file!");
            }
            else if (!File.Exists(inputFile.Trim()))
            {
                ShowMessageBox("ERROR: Selected input file does not exist!");
            }
            else if (serviceName == null || serviceName.Trim().Length == 0)
            {
                ShowMessageBox("Must select service name!");
            }
            else if (preacher == null || preacher.Trim().Length == 0)
            {
                ShowMessageBox("ERROR: Must select a preacher name!");
            }
            else if (list == null || list.Trim().Length == 0)
            {
                ShowMessageBox("ERROR: List must be specified!");
            }
            else
            {
                FileInfo inputFileInfo = new FileInfo(inputFile);

                progressBar1.Value = 0;
                lblProgress.Visible = true;
                progressBar1.Visible = true;
                lblProgressText.Visible = true;

                Action action = () =>
                {
                    ExecuteUpload(inputFileInfo, serviceName, preacher, reference, serviceDate, list);
                };
                Task task = new Task(action);
                task.Start();
            }
        }

        private void ExecuteUpload(FileInfo inputFileInfo, string serviceName, string preacher, string reference, DateTime serviceDate, string list)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    SetInputsEnabledStatus(false);
                });

                AudioManagerClient client = new AudioManagerClient();
                if (!client.Login())
                {
                    ShowMessageBox("Login to Audio Manager failed! Check credentials in settings.");
                    return;
                }

                this.Invoke((MethodInvoker)delegate
                {
                    lblProgressText.Text = "Beginning File Upload";
                    progressBar1.Value = 0;
                });
                AudioManagerClient.UploadAudioProgress progressCallback = delegate(string filename, long currentProgress, long totalLength)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblProgressText.Text = "Uploading " + filename + " " + currentProgress + "/" + totalLength;
                        int percentage = (int)(((double)currentProgress / (double)totalLength) * 100d);
                        progressBar1.Value = percentage > 100 ? 100 : percentage;
                    });
                };
                string remoteFilename;
                client.UploadVideo(out remoteFilename, inputFileInfo.FullName, list, progressCallback);
                client.Login();
                client.AddAudio(remoteFilename, serviceDate, serviceName, preacher, reference, list);
                this.Invoke((MethodInvoker)delegate
                {
                    ShowMessageBox("Upload Complete!");
                });

            }
            catch (Exception e)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    ShowMessageBox("Upload failed due to: " + e.Message);
                });
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblProgress.Visible = false;
                    progressBar1.Visible = false;
                    lblProgressText.Visible = false;
                    SetInputsEnabledStatus(true);
                });
            }
        }

        private void ShowMessageBox(string txt)
        {
            using (new CenterWinDialog(MainForm))
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show(this, txt);
                    });
                }
                else
                {
                    MessageBox.Show(this, txt);
                }
            }
        }
    }
}
