using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Facebook;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace VideoConverter
{
    public class VideoManagerClient
    {
        private CookieContainer cookieContainer;
        private string currentFilename;
        private string youtubeVideoID;
        private UploadVideoProgress currentProgressCallback;

        public VideoManagerClient()
        {
            cookieContainer = new CookieContainer();
        }

        public delegate void UploadVideoProgress(string filename, long currentProgress, long totalLength);
        public bool UploadVideo(UploadMode uploadMode, out string remoteFilename, string localfilename, string serviceName, string reference, string[] tags, UploadVideoProgress progressCallback)
        {
            this.currentFilename = localfilename;
            this.currentProgressCallback = progressCallback;
            if (uploadMode == UploadMode.Youtube)
            {
                Task<bool> uploadVideoTask = UploadVideoYoutube(localfilename, serviceName, reference, tags, progressCallback);
                uploadVideoTask.Wait();
                remoteFilename = "youtube:" + this.youtubeVideoID;
                return uploadVideoTask.Result;
            } else if (uploadMode == UploadMode.Facebook)
            {
                object result = UploadVideoFacebook(localfilename, serviceName, reference, tags, progressCallback);
                remoteFilename = "facebook:" + localfilename;
                return true;
            }
            else
            {
                remoteFilename = new FileInfo(localfilename).Name;
                return true;
            }
        }

        private bool UploadVideoFacebook(string localfilename, string serviceName, string reference, string[] tags, UploadVideoProgress progressCallback)
        {
            string facebookPageID = Properties.Settings.Default.FacebookPageID;
            string facebookToken = Properties.Settings.Default.FacebookToken;

            FacebookVideoUploader facebookVideoUploader = new FacebookVideoUploader();
            facebookVideoUploader.FacebookPageID = facebookPageID;
            facebookVideoUploader.FacebookToken = facebookToken;

            facebookVideoUploader.UploadVideo(localfilename, serviceName, reference, progressCallback);
            return true;
        }

        private async Task<Boolean> UploadVideoYoutube(string localfilename, string serviceName, string reference, string[] tags, UploadVideoProgress progressCallback)
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
            video.Snippet.Tags = tags;
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
    }
}
