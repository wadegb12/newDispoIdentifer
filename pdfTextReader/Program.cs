using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using pdfTextReader.Common;
using pdfTextReader.Services;

namespace pdfTextReader
{
    class Program
    {
        private static bool IsWadesLocal = true;

        //static WebDriver webDriver = new WebDriver(@"C:\Users\kylen\source\repos\wadegb12\newDispoIdentifier\ChomeDriver", @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\2021-06-06");
        //static WebDriver webDriver = new WebDriver(@"C:\Users\kylen\source\repos\wadegb12\newDispoIdentifier\ChomeDriver", @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\2021-06-06");

        static void Main(string[] args)
        {

            //Tools.DispensaryDownloader.DownloadNewDispensaries(webDriver);
            //Globals.PrefixDateToLatestFile(webDriver);

            Globals.LogIt("Setting Local Variables");

            var directory = @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\";
            
            if(IsWadesLocal)
            {
                directory = @"C:\Users\wadeb\Documents\Development\DispensaryIdentifier\newDispoOutput\";
            }

            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var directoryYesterday = directory + yesterday + @"\";
            var yesterdaysDisposFile = directoryYesterday + yesterday + " Licensed Dispos.txt";

            var dateToday = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            var directoryToday = directory + dateToday + @"\";
            var dispoListFile = directoryToday + dateToday + " omma_dispensaries_list.pdf";
            var todaysDisposFile = directoryToday + dateToday + " Licensed Dispos.txt";
            var newDisposFile = directoryToday + dateToday + " New Dispos.txt";
            var errorLotFile = directory + @"\Log\error_log.txt";

            try
            {
                var dispoListReader = new DispensaryListReader(dispoListFile);
                var todaysDispos = dispoListReader.FindCompanies();
                OutputTodaysCompanies(todaysDispos, todaysDisposFile);

                var yesterdaysDispos = ReadInYesterdaysDispos(yesterdaysDisposFile);
                var yesterdaysDisposHashset = CreateNameHashSetFromDispensaryList(yesterdaysDispos);

                var newDispos = IdentifyNewDispos(todaysDispos, yesterdaysDisposHashset);
                OutputNewDispos(newDispos, newDisposFile);
            }
            catch(Exception e)
            {
                string errorMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd") + "] --- " + e.Message.ToString();
                WriteOutputToFile(errorMessage, errorLotFile);
            }
        }

        

        

        static void OutputTodaysCompanies(List<Dispensary> companies, string outputFileName)
        {
            Globals.LogIt("Creating JSON list of all dispensaries from today.");
            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
               outputFile.WriteLine(new JavaScriptSerializer().Serialize(companies));
            }
        }

        static void OutputNewDispos(List<Dispensary> newDispos, string outputFileName)
        {
            Globals.LogIt("Saving new dispensaries to text file.");
            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
                foreach (var dispo in newDispos)
                {
                    outputFile.WriteLine(dispo.ToString());
                }
            }
        }

        static void WriteOutputToFile(string error, string outputFileName)
        {
            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
                outputFile.WriteLine(error);
            }
        }

        static List<Dispensary> ReadInYesterdaysDispos(string fileName)
        {
            List<Dispensary> dispos = new List<Dispensary>();
            Globals.LogIt("Reading yesterdays dispensaries from history.");
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                dispos = JsonConvert.DeserializeObject<List<Dispensary>>(json);
            }

            return dispos;
        }

        static HashSet<string> CreateNameHashSetFromDispensaryList(List<Dispensary> dispensaries)
        {
            var dispoNameHashSet = new HashSet<string>();

            Globals.LogIt("Creating hashset for list of previous run dispensaries.");
            foreach (var dispo in dispensaries)
            {
                dispoNameHashSet.Add(dispo.LicenseNum);
            }

            return dispoNameHashSet;
        }

        static List<Dispensary> IdentifyNewDispos(List<Dispensary> todaysDispos, HashSet<string> yesterdaysDispos)
        {
            var newDispos = new List<Dispensary>();

            Globals.LogIt("Identifying todays new dispensaries.");

            foreach (var dispo in todaysDispos)
            {
                if (!yesterdaysDispos.Contains(dispo.LicenseNum))
                {
                    newDispos.Add(dispo);
                }
            }

            return newDispos;
        }
    }
}
