using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using VideoConverter.Properties;

namespace VideoConverter
{
    class AudioManagerClient
    {
        public delegate void UploadAudioProgress(string filename, long currentProgress, long totalLength);

        private CookieContainer cookieContainer;
        private string currentFilename;
        private UploadAudioProgress currentProgressCallback;

        private string BaseUrl
        {
            get
            {
                string noHash = Settings.Default.AudioManagerUrl.Replace("#", "");
                if (noHash.EndsWith("/"))
                {
                    noHash = noHash.Substring(0, noHash.Length - 1);
                }
                return noHash;
            }
        }

        public AudioManagerClient()
        {
            cookieContainer = new CookieContainer();
        }

        public bool UploadVideo(out string remoteFilename, string localfilename, string listName, UploadAudioProgress progressCallback)
        {
            this.currentFilename = localfilename;
            this.currentProgressCallback = progressCallback;
            remoteFilename = new FileInfo(localfilename).Name;
            return UploadAudioWebsite(localfilename, listName, progressCallback);
        }

        private bool UploadAudioWebsite(string localfilename, string listName, UploadAudioProgress progressCallback)
        {
            FileInfo localFileInfo = new FileInfo(localfilename);
            FtpInfo ftpInfo = GetFtpInfo();
            string ftpHost = ftpInfo.FtpHost;
            string ftpPath = ftpInfo.ListNameToFtpPath[listName];
            string ftpUser = Settings.Default.FtpUser;
            string ftpPassword = Settings.Default.FtpPassword;

            // Get the object used to communicate with the server.
            string ftpUrl = "ftp://" + ftpHost + "/" + ftpPath + localFileInfo.Name;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

            // Copy the contents of the file to the request stream.
            request.ContentLength = localFileInfo.Length;
            using (FileStream sourceStream = new FileStream(localfilename, FileMode.Open, FileAccess.Read))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] buffer = new byte[65536];
                    long totalRead = 0;
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalRead += bytesRead;
                        requestStream.Write(buffer, 0, buffer.Length);
                        progressCallback(localFileInfo.Name, totalRead, localFileInfo.Length);
                    }
                }
            }

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
            response.Close();

            return true;
        }

        public FtpInfo GetFtpInfo()
        {
            try
            {
                string postUrl = BaseUrl + "/Management.php/ftpInfo";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                webRequest.CookieContainer = cookieContainer;

                ASCIIEncoding encoding = new ASCIIEncoding();

                webRequest.Method = "GET";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Dictionary<string, object> infoDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                FtpInfo result = new FtpInfo();
                result.FtpHost = (string)infoDict["ftpHost"];
                Dictionary<string, string> listToPath = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> kvp in infoDict)
                {
                    if (kvp.Key != "ftpHost")
                    {
                        Newtonsoft.Json.Linq.JObject jobject = (Newtonsoft.Json.Linq.JObject)kvp.Value;
                        listToPath[kvp.Key] = (string)jobject["ftpPath"];
                    }
                }
                result.ListNameToFtpPath = listToPath;
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetListInfo()
        {
            try
            {
                string postUrl = BaseUrl + "/Management.php/uploadLocations";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                webRequest.CookieContainer = cookieContainer;

                ASCIIEncoding encoding = new ASCIIEncoding();

                webRequest.Method = "GET";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Dictionary<string, Dictionary<string, string>> result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(responseString);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool AddAudio(string filename, DateTime serviceDate, string serviceName, string preacher, string reference, string listName)
        {
            try
            {
                //$filename = $req->post('filename');
                //$serviceDateStr = intval($req->post('serviceDate'));
                // serviceName
                // preacher
                //$serviceName = $req->post('serviceName');
                //listName
                string postUrl = BaseUrl + "/Management.php/addAudio";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                webRequest.CookieContainer = cookieContainer;

                ASCIIEncoding encoding = new ASCIIEncoding();
                string postData = "filename=" + Uri.EscapeDataString(filename);
                postData += "&serviceDate=" + (long)(serviceDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
                postData += "&serviceName=" + Uri.EscapeDataString(serviceName);
                postData += "&preacher=" + Uri.EscapeDataString(preacher);
                postData += "&reference=" + Uri.EscapeDataString(reference);
                postData += "&listName=" + Uri.EscapeDataString(listName);
                
                byte[] data = encoding.GetBytes(postData);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;

                using (Stream stream = webRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                return int.Parse(result["success"]) == 1;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool Login()
        {
            try
            {
                string username = Settings.Default.FtpUser;
                string password = Settings.Default.FtpPassword;
                string postUrl = BaseUrl + "/Login.php/login";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                webRequest.CookieContainer = cookieContainer;

                ASCIIEncoding encoding = new ASCIIEncoding();
                string postData = "username=" + username;
                postData += "&password=" + password;
                byte[] data = encoding.GetBytes(postData);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;

                using (Stream stream = webRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                return int.Parse(result["success"]) == 1;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    class FtpInfo
    {
        public string FtpHost { get; set; }
        public Dictionary<string, string> ListNameToFtpPath { get; set; }
    }
}
