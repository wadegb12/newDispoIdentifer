using System;
using System.IO;
using System.Collections.Generic;
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
            

            try
            {
                var outputService = new OutputService();
                var dispoListReader = new DispensaryListReader(dispoListFile);

                var todaysDispos = dispoListReader.FindCompanies();
                outputService.OutputTodaysCompanies(todaysDispos, todaysDisposFile);

                var yesterdaysDispos = ReadInYesterdaysDispos(yesterdaysDisposFile);
                var yesterdaysDisposHashset = CreateNameHashSetFromDispensaryList(yesterdaysDispos);

                var newDispos = IdentifyNewDispos(todaysDispos, yesterdaysDisposHashset);
                outputService.OutputNewDispos(newDispos, newDisposFile);
            }
            catch(Exception e)
            {
                //outputService.WriteOutputToFile(errorMessage);
                //call new logger
            }
        }


        private static List<Dispensary> ReadInYesterdaysDispos(string fileName)
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

        private static HashSet<string> CreateNameHashSetFromDispensaryList(List<Dispensary> dispensaries)
        {
            var dispoNameHashSet = new HashSet<string>();

            Globals.LogIt("Creating hashset for list of previous run dispensaries.");
            foreach (var dispo in dispensaries)
            {
                dispoNameHashSet.Add(dispo.LicenseNum);
            }

            return dispoNameHashSet;
        }

        private static List<Dispensary> IdentifyNewDispos(List<Dispensary> todaysDispos, HashSet<string> yesterdaysDispos)
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
