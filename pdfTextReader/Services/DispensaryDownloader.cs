using pdfTextReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;



namespace pdfTextReader.Services
{
    class DispensaryDownloader
    {
        // KNB : 6/12/21 : Simplified to get away from chrome driver and not have to worry about having latest selenium driver
        public static void DownloadNewDispensaries()
        {
            WebClient tempWebClient = new WebClient();
            tempWebClient.Headers.Add("User-Agent: Other");
            tempWebClient.DownloadFile(Globals.GetDispensaryURL(), Globals.GetDailyFolder() + Globals.GetDailyDownloadFile());
            Thread.Sleep(5000);
        }

    }
}
