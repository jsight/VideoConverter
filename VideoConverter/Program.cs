using System;
using System.Net;
using System.Windows.Forms;

using Facebook;

namespace VideoConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FacebookClient.SetDefaultHttpWebRequestFactory(url => {
                var request = new HttpWebRequestWrapper((HttpWebRequest)WebRequest.Create(url));
                request.Timeout = 1000 * 60 * 60;
                return request;
            });

            Application.Run(new FormMainConverter());
        }
    }
}
