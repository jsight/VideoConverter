using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VideoConverter
{
    public class FacebookVideoUploader
    {
        public string FacebookToken { get; set; }
        public String FacebookPageID { get; set; }

        public void UploadVideo(string filename, string title, string description, VideoManagerClient.UploadVideoProgress progressCallback)
        {
            Console.WriteLine(FacebookPageID);
            FileInfo fileInfo = new FileInfo(filename);
            string videoUrl = "https://graph-video.facebook.com/v2.3/" + FacebookPageID + "/videos";
            var webClient = createWebClient();
            UploaderState state = sendStartRequest(videoUrl, fileInfo.Length);
            uploadChunks(state, videoUrl, fileInfo, progressCallback);
            Console.WriteLine("Upload chunks complete!");
            progressCallback(fileInfo.Name, 0, fileInfo.Length);
            bool success = postVideo(state, videoUrl, fileInfo, title, description);
            Console.WriteLine("Success? " + success);
            progressCallback(fileInfo.Name, fileInfo.Length, fileInfo.Length);
            System.Threading.Thread.Sleep(60000);
        }

        private bool postVideo(UploaderState state, string url, FileInfo fileInfo, string title, string description)
        {
            var values = new Dictionary<string, string>
            {
                { "access_token", FacebookToken },
                { "upload_phase", "finish" },
                { "upload_session_id", state.UploadSessionID },
                { "title", title },
                { "description", description }
            };

            var content = new FormUrlEncodedContent(values);

            var client = createWebClient();
            JObject initialJson = postAndGetJson(client, url, content);
            var success = initialJson["success"].ToObject<string>();
            return "true".Equals(success, StringComparison.CurrentCultureIgnoreCase);
        }

        private void uploadChunks(UploaderState state, string url, FileInfo fileInfo, VideoManagerClient.UploadVideoProgress progressCallback)
        {
            int chunk = 1;
            using (FileStream fileStream = new FileStream(fileInfo.ToString(), FileMode.Open))
            {
                while (true)
                {
                    progressCallback(fileInfo.Name, state.StartOffset, fileInfo.Length);
                    byte[] data = new byte[state.EndOffset - state.StartOffset];
                    int totalRead = 0;
                    while ( totalRead < data.Length )
                    {
                        int bytesRead = fileStream.Read(data, totalRead, data.Length - totalRead);
                        totalRead += bytesRead;
                        Console.WriteLine("Total Read: " + totalRead + ", bytesRead: " + bytesRead);
                    }

                    MultipartFormDataContent form = new MultipartFormDataContent();

                    form.Add(new StringContent(FacebookToken), "access_token");
                    form.Add(new StringContent("transfer"), "upload_phase");
                    form.Add(new StringContent(state.StartOffset.ToString()), "start_offset");
                    form.Add(new StringContent(state.UploadSessionID.ToString()), "upload_session_id");

                    form.Add(new ByteArrayContent(data, 0, data.Length), "video_file_chunk", "chunk" + chunk + ".mp4");

                    var client = createWebClient();

                    JObject initialJson = postAndGetJson(client, url, form);
                    state.StartOffset = initialJson["start_offset"].ToObject<int>();
                    state.EndOffset = initialJson["end_offset"].ToObject<int>();
                    Console.WriteLine("Uploaderstate: " + state.ToString());

                    chunk++;

                    // This indicates that the upload process is complete
                    if (state.StartOffset == state.EndOffset)
                        break;
                }
            }
        }

        private UploaderState sendStartRequest(string videoUrl, long fileSize)
        {
            var values = new Dictionary<string, string>
            {
                { "access_token", FacebookToken },
                { "upload_phase", "start" },
                { "file_size", fileSize.ToString() }
            };

            var content = new FormUrlEncodedContent(values);

            var client = createWebClient();
            JObject initialJson = postAndGetJson(client, videoUrl, content);
            UploaderState uploaderState = new UploaderState();
            uploaderState.VideoID = initialJson["video_id"].ToObject<string>();
            uploaderState.UploadSessionID = initialJson["upload_session_id"].ToObject<string>();
            uploaderState.StartOffset = initialJson["start_offset"].ToObject<int>();
            uploaderState.EndOffset = initialJson["end_offset"].ToObject<int>();
            Console.WriteLine("Uploaderstate: " + uploaderState.ToString());
            return uploaderState;
        }

        private JObject postAndGetJson(HttpClient client, String url, HttpContent content)
        {
            var responseTask = client.PostAsync(url, content);
            responseTask.Wait();

            var responseStringTask = responseTask.Result.Content.ReadAsStringAsync();
            responseStringTask.Wait();
            var responseString = responseStringTask.Result;
            Console.WriteLine("Response: " + responseString);
            JObject json = JObject.Parse(responseString);
            return json;
        }

        private HttpClient createWebClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromHours(2);
            return httpClient;
        }

        private class UploaderState
        {
            public string VideoID { get; set; }
            public string UploadSessionID { get; set; }
            public int StartOffset { get; set; }
            public int EndOffset { get; set; }

            public override string ToString()
            {
                return "[UploaderState(VideoID=" + VideoID + ",UploadSessionID="+UploadSessionID+",StartOffset="+StartOffset+",EndOffset="+EndOffset+")]";
            }
        }
    }
}
