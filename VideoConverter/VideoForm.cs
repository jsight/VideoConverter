using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoConverter
{
    public partial class VideoForm : UserControl
    {
        const string WEBSITE_COMMAND_TEMPLATE_FILE = "CommandTemplate.txt";
        const string YOUTUBE_COMMAND_TEMPLATE_FILE = "CommandTemplate_YouTube.txt";

        public FormMainConverter MainForm { get; set; }

        public VideoForm()
        {
            InitializeComponent();
            MyInit();
        }

        private void MyInit()
        {
            if (Properties.Settings.Default.LastOutputDirectory != null)
            {
                txtOutputDir.Text = Properties.Settings.Default.LastOutputDirectory;
            }
            progressBar1.Visible = false;
            lblProgress.Visible = false;
            lblProgressText.Visible = false;
            lblProgressText.Text = "";
            checkboxFullUpload.Checked = true;
            checkBoxYouTubeUpload.Checked = true;
            checkboxFacebook.Checked = facebookSettingsAvailable();
        }

        private bool facebookSettingsAvailable()
        {
            var facebookToken = Properties.Settings.Default.FacebookToken;
            if (facebookToken == null || facebookToken.Trim() == "")
                return false;

            var facebookPageID = Properties.Settings.Default.FacebookPageID;
            if (facebookPageID == null || facebookPageID.Trim() == "")
                return false;

            return true;
        }

        private void btnSelectInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TS Files (*.ts)|*.ts|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                txtInputFIle.Text = fileName;
            }
        }

        private void btnSelectOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string dirName = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.LastOutputDirectory = dirName;
                Properties.Settings.Default.Save();

                txtOutputDir.Text = dirName;
            }
        }

        private void setControlsEnabled(bool val)
        {
            txtInputFIle.Enabled = val;
            txtOutputDir.Enabled = val;
            btnSelectOutput.Enabled = val;
            btnSelectInput.Enabled = val;
            btnConvert.Enabled = val;
            txtServiceName.Enabled = val;
            txtScriptureReference.Enabled = val;
            dateTimePicker1.Enabled = val;

            numericSkipMinutesWebsite.Enabled = val;
            numericSkipSecondsWebsite.Enabled = val;
            checkboxFullUpload.Enabled = val;

            numericSkipMinutesYouTube.Enabled = val;
            numericSkipSecondsYouTube.Enabled = val;
            checkBoxYouTubeUpload.Enabled = val;
            checkboxFacebook.Enabled = val;
        }

        private void ShowMessageBox(string txt)
        {
            using (new CenterWinDialog(MainForm))
            {
                MessageBox.Show(this, txt);
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            lblProgressText.Text = "";
            string inputFile = txtInputFIle.Text;
            string outputDir = txtOutputDir.Text;
            string serviceName = txtServiceName.Text;
            string scriptureReference = txtScriptureReference.Text;
            DateTime serviceDate = dateTimePicker1.Value;

            if (inputFile == null || inputFile.Trim().Length == 0)
            {
                ShowMessageBox("Must select an input file!");
            }
            else if (!File.Exists(inputFile.Trim()))
            {
                ShowMessageBox("ERROR: Selected input file does not exist!");
            }
            else if (outputDir == null || outputDir.Trim().Length == 0)
            {
                ShowMessageBox("Must select an output directory!");
            }
            else if (!Directory.Exists(outputDir))
            {
                ShowMessageBox("ERROR: Output directory does not exist!");
            }
            else if (serviceName == null || serviceName.Trim().Length == 0)
            {
                ShowMessageBox("ERROR: Service Name must be specified!");
            }
            else
            {
                FileInfo inputFileInfo = new FileInfo(inputFile);
                FileInfo outputFileInfo = new FileInfo(inputFileInfo.FullName.Replace(inputFileInfo.Extension, ".mp4"));
                string outputFile = txtOutputDir.Text + @"\" + outputFileInfo.Name;
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                progressBar1.Value = 0;
                lblProgress.Visible = true;
                progressBar1.Visible = true;
                lblProgressText.Visible = true;

                Action action = () =>
                {
                    string errors = "";

                    if (checkboxFullUpload.Checked)
                    {
                        string title = "(Full Service) " + serviceName;
                        string[] tags = new string[] { "Tri-City Baptist Church", "Goose Creek", "Moncks Corner", "Summerville", "full" };
                        string newErrors = ExecuteConversion(YOUTUBE_COMMAND_TEMPLATE_FILE, UploadMode.Youtube, UploadMode.No_Upload, inputFileInfo, outputFile, title, scriptureReference, tags, serviceDate, (int)numericSkipMinutesWebsite.Value, (int)numericSkipSecondsWebsite.Value, true);
                        if (newErrors != "")
                            errors += newErrors + "\n\n";
                    }

                    if (checkBoxYouTubeUpload.Checked)
                    {
                        string outputFileYoutube = txtOutputDir.Text + @"\" + inputFileInfo.Name.Replace(inputFileInfo.Extension, "_ytandfb.mp4");
                        string[] tags = new string[] { "Tri-City Baptist Church", "Goose Creek", "Moncks Corner", "Summerville", "trimmed" };
                        UploadMode uploadMode2 = this.checkboxFacebook.Checked ? UploadMode.Facebook : UploadMode.No_Upload;
                        string newErrors = ExecuteConversion(YOUTUBE_COMMAND_TEMPLATE_FILE, UploadMode.Youtube, uploadMode2, inputFileInfo, outputFileYoutube, serviceName, scriptureReference, tags, serviceDate, (int)numericSkipMinutesYouTube.Value, (int)numericSkipSecondsYouTube.Value, true);
                        if (newErrors != "")
                            errors += newErrors + "\n\n";
                    }

                    if (!checkboxFullUpload.Checked && !checkBoxYouTubeUpload.Checked)
                    {
                        string[] tags = new string[] { };
                        string newErrors = ExecuteConversion(WEBSITE_COMMAND_TEMPLATE_FILE, UploadMode.No_Upload, UploadMode.No_Upload, inputFileInfo, outputFile, serviceName, scriptureReference, tags, serviceDate, (int)numericSkipMinutesYouTube.Value, (int)numericSkipSecondsYouTube.Value, false);
                        if (newErrors != "")
                            errors += newErrors + "\n\n";
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        if (errors != "")
                        {
                            ShowMessageBox("Process completed with errors: \n\n" + errors);
                        }
                        else
                        {
                            ShowMessageBox("Conversion Complete!");
                        }

                        lblProgress.Visible = false;
                        progressBar1.Visible = false;
                        lblProgressText.Visible = false;
                        setControlsEnabled(true);
                    });
                };
                
                Task task = new Task(action);
                task.Start();
            }
        }

        private string ExecuteConversion(string commandTemplateFile, UploadMode uploadMode1, UploadMode uploadMode2, FileInfo inputFileInfo, string outputFile, string serviceName, string scriptureReference, string[] tags, DateTime serviceDate, int skipMinutes, int skipSeconds, bool visible)
        {
            string errors = "";
            int totalSkipSeconds = (60 * skipMinutes) + skipSeconds;
            string commandTemplate = LoadCommandTemplate(commandTemplateFile).Trim();
            string[] commandTemplateArr = commandTemplate.Split(' ');
            List<string> args = new List<string>();
            string command = commandTemplateArr[0];

            foreach (string arg in commandTemplateArr.ToList().Skip(1))
            {
                switch (arg)
                {
                    case "${inputFile}":
                        args.Add(inputFileInfo.FullName);
                        break;
                    case "${outputFile}":
                        args.Add(outputFile);
                        break;
                    case "${skipSeconds}":
                        args.Add("" + totalSkipSeconds);
                        break;
                    default:
                        args.Add(arg);
                        break;
                }
            }
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    setControlsEnabled(false);
                });

                FileInfo outputFileInfo = new FileInfo(outputFile);
                outputFileInfo.Delete();
                VideoManagerClient client = new VideoManagerClient();

                string homePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                using (StreamWriter outWriter = new StreamWriter(homePath + @"\videoconversionout_" + uploadMode1 + ".log"))
                {
                    // Start the child process.
                    Process p = new Process();
                    // Redirect the output stream of the child process.
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.FileName = command;
                    p.StartInfo.Arguments = ArgvToCommandLine(args);
                    outWriter.WriteLine("Arguments: " + p.StartInfo.Arguments);

                    p.Start();

                    // make sure to kill it if we are closed
                    MainForm.FormClosing += delegate(Object sender, FormClosingEventArgs e)
                    {
                        p.Refresh();
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                    };

                    // Do not wait for the child process to exit before
                    // reading to the end of its redirected stream.
                    // p.WaitForExit();
                    // Read the output stream first and then wait.
                    string line;
                    string totalDurationString = null;


                    Regex durationRegex = new Regex(".*Duration: (.*?), ");
                    Regex frameLineRegex = new Regex(".*time=(.*?) .*");

                    int totalSeconds = 0;
                    while ((line = p.StandardError.ReadLine()) != null)
                    {
                        if (line.Contains("Duration: "))
                        {
                            string durationString = durationRegex.Match(line).Groups[1].Value;
                            totalDurationString = durationString;
                            outWriter.WriteLine("DURATION: " + durationString);
                            string[] durationArray = durationString.Split(':');

                            //00:00:49.55
                            float hours = float.Parse(durationArray[0]);
                            float minutes = float.Parse(durationArray[1]);
                            float seconds = float.Parse(durationArray[2]);

                            totalSeconds = (int)(seconds + (minutes * 60) + (hours * 60 * 60));
                        }
                        else if (frameLineRegex.IsMatch(line))
                        {
                            // 00:00:00.30
                            string currentPositionString = frameLineRegex.Match(line).Groups[1].Value;
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblProgressText.Text = currentPositionString + "/" + totalDurationString;
                            });
                            string[] currentPositionArray = currentPositionString.Split(':');
                            float hours = float.Parse(currentPositionArray[0]);
                            float minutes = float.Parse(currentPositionArray[1]);
                            float seconds = float.Parse(currentPositionArray[2]);

                            int currentSeconds = (int)(seconds + (minutes * 60) + (hours * 60 * 60));

                            int currentPercentage = (int)(((double)currentSeconds / (double)totalSeconds) * 100d);
                            outWriter.WriteLine("PERCENTAGE: " + currentPercentage);
                            this.Invoke((MethodInvoker)delegate
                            {
                                progressBar1.Value = currentPercentage > 100 ? 100 : currentPercentage;
                            });
                        }
                        outWriter.WriteLine("LINE: " + line);
                        outWriter.Flush();
                    }
                    while ((line = p.StandardOutput.ReadLine()) != null)
                    {
                        outWriter.WriteLine(line);
                    }
                    p.WaitForExit();
                }

                if (uploadMode1 != UploadMode.No_Upload || uploadMode2 != UploadMode.No_Upload)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblProgressText.Text = "Beginning File Upload";
                        progressBar1.Value = 0;
                    });


                    UploadMode currentUploadMode = uploadMode1;
                    VideoManagerClient.UploadVideoProgress progressCallback = delegate(string filename, long currentProgress, long totalLength)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            lblProgressText.Text = "(" + currentUploadMode + ") Uploading " + filename + " " + currentProgress + "/" + totalLength;
                            int percentage = (int)(((double)currentProgress / (double)totalLength) * 100d);
                            progressBar1.Value = percentage > 100 ? 100 : percentage;
                        });
                    };
                    string remoteFilename;

                    try
                    {
                        if (uploadMode1 != UploadMode.No_Upload)
                        {
                            currentUploadMode = uploadMode1;
                            client.UploadVideo(uploadMode1, out remoteFilename, outputFileInfo.FullName, serviceName, scriptureReference, tags, progressCallback);
                        }
                    }
                    catch (Exception e)
                    {
                        errors += "Conversion for " + uploadMode1 + " failed due to: " + e.Message;
                    }

                    try
                    {
                        if (uploadMode2 != UploadMode.No_Upload)
                        {
                            currentUploadMode = uploadMode2;
                            client.UploadVideo(uploadMode2, out remoteFilename, outputFileInfo.FullName, serviceName, scriptureReference, tags, progressCallback);
                        }
                    }
                    catch (Exception e)
                    {
                        errors += "Conversion for " + uploadMode2 + " failed due to: " + e.Message;
                    }

                    if (uploadMode1 != UploadMode.No_Upload || uploadMode2 != UploadMode.No_Upload)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            lblProgressText.Text = "Upload complete!";
                        });
                    } else
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            lblProgressText.Text = UploadMode.No_Upload + " Video Update Complete!";
                        });
                    }
                }
            }
            catch (Exception e)
            {
                errors += "Conversion for " + uploadMode1 + " failed due to: " + e.Message;
            }
            return errors;
        }

        public static string ArgvToCommandLine(IEnumerable<string> args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in args)
            {
                sb.Append('"');
                // Escape double quotes (") and backslashes (\).
                int searchIndex = 0;
                while (true)
                {
                    // Put this test first to support zero length strings.
                    if (searchIndex >= s.Length)
                    {
                        break;
                    }
                    int quoteIndex = s.IndexOf('"', searchIndex);
                    if (quoteIndex < 0)
                    {
                        break;
                    }
                    sb.Append(s, searchIndex, quoteIndex - searchIndex);
                    EscapeBackslashes(sb, s, quoteIndex - 1);
                    sb.Append('\\');
                    sb.Append('"');
                    searchIndex = quoteIndex + 1;
                }
                sb.Append(s, searchIndex, s.Length - searchIndex);
                EscapeBackslashes(sb, s, s.Length - 1);
                sb.Append(@""" ");
            }
            return sb.ToString(0, Math.Max(0, sb.Length - 1));
        }
        private static void EscapeBackslashes(StringBuilder sb, string s, int lastSearchIndex)
        {
            // Backslashes must be escaped if and only if they precede a double quote.
            for (int i = lastSearchIndex; i >= 0; i--)
            {
                if (s[i] != '\\')
                {
                    break;
                }
                sb.Append('\\');
            }
        }

        private string LoadCommandTemplate(string commandTemplateFilename)
        {
            return System.IO.File.ReadAllText(commandTemplateFilename);
        }
    }
}
