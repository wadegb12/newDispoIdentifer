using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace pdfTextReader.Common
{
    class Globals
    {
        // KNB : 6/6/21 : Accpet string and post to console window.
        public static void LogIt(string logMessage) { Console.WriteLine("["+DateTime.Now.ToString()+ "] --- " + logMessage); }

        // KNB : 6/6/Prefix the date to the newest file in the path.
        public static void PrefixDateToLatestFile(WebDriver driver)
        {
            Globals.LogIt("Appending Date to newest file in the download directory.");
            DirectoryInfo downloadDirectory = new DirectoryInfo(driver.GetDefaultDownloadDirectory());
            FileInfo newestFile = downloadDirectory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();

            string newName = DateTime.Now.ToString("yyyy-MM-dd") + " " + newestFile.Name;

            newestFile.MoveTo(driver.GetDefaultDownloadDirectory() + "\\" + newName);

            return;
        }


    }
}
