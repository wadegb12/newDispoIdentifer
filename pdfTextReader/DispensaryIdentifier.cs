using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using pdfTextReader.Common;
using pdfTextReader.Services;

namespace pdfTextReader
{
    class DispensaryIdentifier
    {
        public bool IsWadesLocal = true;
        public string RootDirectory;
        public string YesterdaysDisposFile;
        public string TodaysOMMAListFile;
        public string TodaysDisposFile;
        public string TodaysNewDisposFile;
        public Logger Logger;
        public WebDriver WebDriver;

        public DispensaryIdentifier()
        {
            ReadConfigVariables();
        }

        private void ReadConfigVariables()
        {
            RootDirectory = @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\";
            if (IsWadesLocal)
            {
                RootDirectory = @"C:\Users\wadeb\Documents\Development\DispensaryIdentifier\newDispoOutput\";
            }

            Logger = new Logger(RootDirectory + @"Log\error_log.txt");
            Logger.WriteLog("Setting Local Variables");

            //WebDriver = new WebDriver(@"C:\Users\kylen\source\repos\wadegb12\newDispoIdentifier\ChomeDriver", @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\2021-06-06");
            if (IsWadesLocal)
            {
                //WebDriver = new WebDriver(@"C:\Users\wadeb\Downloads\chromedriver_win32", RootDirectory);
            }

            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var directoryYesterday = RootDirectory + yesterday + @"\";
            YesterdaysDisposFile = directoryYesterday + yesterday + " Licensed Dispos.txt";

            var dateToday = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            var directoryToday = RootDirectory + dateToday + @"\";
            TodaysOMMAListFile = directoryToday + dateToday + " omma_dispensaries_list.pdf";
            TodaysDisposFile = directoryToday + dateToday + " Licensed Dispos.txt";
            TodaysNewDisposFile = directoryToday + dateToday + " New Dispos.txt";
        }

        public void Run()
        {
            try
            {
                //Services.DispensaryDownloader.DownloadNewDispensaries(webDriver);
                //Globals.PrefixDateToLatestFile(webDriver);

                var outputService = new OutputService(Logger);
                var dispoListReader = new DispensaryListReader(TodaysOMMAListFile, Logger);

                var todaysDispos = dispoListReader.FindCompanies();
                outputService.OutputTodaysCompanies(todaysDispos, TodaysDisposFile);

                var yesterdaysDispos = ReadInYesterdaysDispos(YesterdaysDisposFile);
                var yesterdaysDisposHashset = CreateNameHashSetFromDispensaryList(yesterdaysDispos);

                var newDispos = IdentifyNewDispos(todaysDispos, yesterdaysDisposHashset);
                outputService.OutputNewDispos(newDispos, TodaysNewDisposFile);
            }
            catch (Exception e)
            {
                Logger.WriteLog(e.ToString());
            }
        }

        public List<Dispensary> ReadInYesterdaysDispos(string fileName)
        {
            Logger.WriteLog("Reading yesterdays dispensaries from history.");
            List<Dispensary> dispos = new List<Dispensary>();
            
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                dispos = JsonConvert.DeserializeObject<List<Dispensary>>(json);
            }

            return dispos;
        }

        public HashSet<string> CreateNameHashSetFromDispensaryList(List<Dispensary> dispensaries)
        {
            Logger.WriteLog("Creating hashset for list of previous run dispensaries.");
            var dispoNameHashSet = new HashSet<string>();

            foreach (var dispo in dispensaries)
            {
                dispoNameHashSet.Add(dispo.LicenseNum);
            }

            return dispoNameHashSet;
        }

        public List<Dispensary> IdentifyNewDispos(List<Dispensary> todaysDispos, HashSet<string> yesterdaysDispos)
        {
            Logger.WriteLog("Identifying todays new dispensaries.");
            var newDispos = new List<Dispensary>();

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
