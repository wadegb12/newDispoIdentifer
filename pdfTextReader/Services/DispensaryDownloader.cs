using pdfTextReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;



namespace pdfTextReader.Services
{
    class DispensaryDownloader
    {
        static WebDriver wd;


        public static void DownloadNewDispensaries(WebDriver driver)
        {
            wd = driver;

            if (NavigateToPage())
            {

                return;
            }
            else
            {
                throw new Exception("Failed to download dispensary file.");
            }
        }

        static bool NavigateToPage()
        {
            Globals.LogIt("Checking if driver is launched");
            if (wd.CheckCrashedDriver())
            {
                Globals.LogIt("Chrome version is updated past driver version");
            }
            else
            {
                Globals.LogIt("Navigating to Oklahoma.gov dispensaries list");
                string message = wd.ToDispensaryPage("https://oklahoma.gov/content/dam/ok/en/omma/docs/business-lists/omma_dispensaries_list.pdf");
                Thread.Sleep(5000);
                return true;
            }

            return false;
        }
    }
}
