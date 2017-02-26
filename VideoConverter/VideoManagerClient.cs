using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using Newtonsoft.Json;
using VideoConverter.Properties;

namespace VideoConverter
{
    class VideoManagerClient
    {
        private CookieContainer cookieContainer;
        private string currentFilename;
        private string youtubeVideoID;
        private UploadVideoProgress currentProgressCallback;
        private string ftpHost;
        private string ftpPath;

        private string BaseUrl
        {
            get
            {
                string noHash = Settings.Default.VideoManagerUrl.Replace("#", "");
                if (noHash.EndsWith("/"))
                {
                    noHash = noHash.Substring(0, noHash.Length - 1);
                }
                return noHash;
            }
        }

        private string FtpHost
        {
            get
            {
                if (ftpHost == null)
                {
                    Login();
                    GetFtpInfo();
                }
                return ftpHost;
            }
        }

        private string FtpPath
        {
            get
            {
                if (ftpPath == null)
                {
                    Login();
                    GetFtpInfo();
                }
                if (!ftpPath.EndsWith("/"))
                {
                    ftpPath += "/";
                }
                return ftpPath;
            }
        }

        public VideoManagerClient()
        {
            cookieContainer = new CookieContainer();
        }

        public Dictionary<string, string> GetFtpInfo()
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

                Dictionary<string, string> infoDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                ftpHost = infoDict["ftpHost"];
                ftpPath = infoDict["ftpPath"];
                return infoDict;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public delegate void UploadVideoProgress(string filename, long currentProgress, long totalLength);
        public bool UploadVideo(UploadMode uploadMode, out string remoteFilename, string localfilename, string serviceName, string reference, UploadVideoProgress progressCallback)
        {
            this.currentFilename = localfilename;
            this.currentProgressCallback = progressCallback;
            if (uploadMode == UploadMode.Youtube)
            {
                Task<bool> uploadVideoTask = UploadVideoYoutube(localfilename, serviceName, reference, progressCallback);
                uploadVideoTask.Wait();
                remoteFilename = "youtube:" + this.youtubeVideoID;
                return uploadVideoTask.Result;
            }
            else if (uploadMode == UploadMode.Website)
            {
                remoteFilename = new FileInfo(localfilename).Name;
                return UploadVideoWebsite(localfilename, serviceName, false, 0, progressCallback);
            }
            else
            {
                remoteFilename = new FileInfo(localfilename).Name;
                return true;
            }
        }

        private async Task<Boolean> UploadVideoYoutube(string localfilename, string serviceName, string reference, UploadVideoProgress progressCallback)
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });
            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = serviceName;
            video.Snippet.Description = reference;
            video.Snippet.Tags = new string[] { "Northside Baptist Church", "Moncks Corner" };
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/listItem
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public"; // "unlisted" or "private" or "public"
            var filePath = localfilename; // Replace with path to actual movie file.

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                IUploadProgress uploadProgress = videosInsertRequest.Upload();
                bool result = uploadProgress.Status != UploadStatus.Failed;
                if (result)
                {
                    this.youtubeVideoID = videosInsertRequest.ResponseBody.Id;
                }
                return result;
            }
        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            FileInfo localFileInfo = new FileInfo(this.currentFilename);

            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    currentProgressCallback(localFileInfo.Name, progress.BytesSent, localFileInfo.Length);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            FileInfo localFileInfo = new FileInfo(this.currentFilename);
            currentProgressCallback(this.currentFilename, localFileInfo.Length, localFileInfo.Length);
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
        }

        private bool UploadVideoWebsite(string localfilename, string serviceName, bool resume, int attemptCount, UploadVideoProgress progressCallback)
        {
            if (attemptCount > 5)
                throw new Exception("Unable to upload file");

            FileInfo localFileInfo = new FileInfo(localfilename);
            long expectedFileSize = localFileInfo.Length;
            long actualFtpServerFileSize = 0;

            string ftpUser = Settings.Default.FtpUser;
            string ftpPassword = Settings.Default.FtpPassword;

            // Get the object used to communicate with the server.
            string ftpUrl = "ftp://" + FtpHost + "/" + FtpPath + localFileInfo.Name;

            if (resume)
            {
                FtpWebRequest sizeCheckRequest = (FtpWebRequest)WebRequest.Create(ftpUrl);
                sizeCheckRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                sizeCheckRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)sizeCheckRequest.GetResponse();
                actualFtpServerFileSize = response.ContentLength;
                if (actualFtpServerFileSize >= expectedFileSize)
                    return true;
            }

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            if (resume)
                request.Method = WebRequestMethods.Ftp.AppendFile;
            else
                request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

            // Copy the contents of the file to the request stream.
            request.ContentLength = localFileInfo.Length - actualFtpServerFileSize;
            try
            {
                using (FileStream sourceStream = new FileStream(localfilename, FileMode.Open, FileAccess.Read))
                {
                    if (resume)
                    {
                        sourceStream.Seek(actualFtpServerFileSize, SeekOrigin.Begin);
                    }
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        byte[] buffer = new byte[65536];
                        long totalRead = 0;
                        int bytesRead;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalRead += bytesRead;
                            requestStream.Write(buffer, 0, buffer.Length);
                            requestStream.Flush();
                            progressCallback(localFileInfo.Name, totalRead, localFileInfo.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                UploadVideoWebsite(localfilename, serviceName, true, attemptCount + 1, progressCallback);
            }

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to close the connection after upload is complete!");
            }

            return true;
        }

        public bool UpdateVideo(string filename, DateTime serviceDate, string serviceName, string scriptureReference, bool visible)
        {
            try
            {
                //$filename = $req->post('filename');
                //$serviceDateStr = intval($req->post('serviceDate'));
                //$serviceName = $req->post('serviceName');
                //$visible = $req->post('visible');
                string postUrl = BaseUrl + "/Management.php/updateVideo";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                webRequest.CookieContainer = cookieContainer;

                ASCIIEncoding encoding = new ASCIIEncoding();
                string postData = "filename=" + Uri.EscapeDataString(filename);
                postData += "&serviceDate=" + (long)(serviceDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
                postData += "&serviceName=" + Uri.EscapeDataString(serviceName);
                postData += "&scriptureReference=" + Uri.EscapeDataString(scriptureReference);
                if (visible)
                {
                    postData += "&visible=on";
                }
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
                this.cookieContainer = new CookieContainer();
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
}
