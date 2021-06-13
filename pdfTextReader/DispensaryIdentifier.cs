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
        public string RootDirectory;
        public string YesterdaysDisposFile;
        public string TodaysOMMAListFile;
        public string TodaysDisposFile;
        public string TodaysNewDisposFile;
        public string TodaysRemovedDisposFile;
        public Logger Logger;

        public DispensaryIdentifier()
        {
            Globals.SyncApplication();
            ReadConfigVariables();
        }

        private void ReadConfigVariables()
        {
            RootDirectory = Globals.GetBaseDirectory();         

            Logger = new Logger(Globals.GetLogFile());
            Logger.WriteLog("Setting Local Variables");

            YesterdaysDisposFile = Globals.GetYesterdayFolder() + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " Licensed Dispos.txt";
            TodaysDisposFile = Globals.GetDailyFolder() + DateTime.Now.ToString("yyyy-MM-dd") + " Licensed Dispos.txt";

            TodaysOMMAListFile = Globals.GetDailyFolder() + Globals.GetDailyDownloadFile();
            TodaysNewDisposFile = Globals.GetDailyFolder() + DateTime.Now.ToString("yyyy-MM-dd") + " New Dispos.txt";
            TodaysRemovedDisposFile = Globals.GetDailyFolder() + DateTime.Now.ToString("yyyy-MM-dd") + " Removed Dispos.txt";
        }

        public void Run()
        {
            try
            {
                Logger.WriteLog("Downloading todays dispensary list.");
                DispensaryDownloader.DownloadNewDispensaries();

                Logger.WriteLog("Creating output service and initializing the PDF reader.");
                var outputService = new OutputService(Logger);
                var dispoListReader = new DispensaryListReader(TodaysOMMAListFile, Logger);

                Logger.WriteLog("Reading todays dispensaries from download file.");
                var todaysDispos = dispoListReader.FindCompanies();
                Logger.WriteLog("Outputting todays dispensaries to file.");
                outputService.OutputTodaysCompanies(todaysDispos, TodaysDisposFile);

                if(File.Exists(Globals.GetYesterdayFolder() + Globals.GetYesterdayDownloadFile()))
                {
                    Logger.WriteLog("Yesterdays file exist, creating hashset of yesterdays dispensaries.");
                    var yesterdaysDispos = ReadInYesterdaysDispos(YesterdaysDisposFile);
                    var yesterdaysDisposHashset = CreateNameHashSetFromDispensaryList(yesterdaysDispos);

                    Logger.WriteLog("Identifying the new dispensaries and outputting them to file.");
                    var newDispos = IdentifyNewDispos(todaysDispos, yesterdaysDisposHashset);
                    outputService.OutputNewDispos(newDispos, TodaysNewDisposFile);

                    Logger.WriteLog("Identifying the removed dispensaries and outputting them to file.");
                    var todaysDispoHashset = CreateNameHashSetFromDispensaryList(todaysDispos);
                    var removedDispos = IdentifyRemovedDispos(yesterdaysDispos, todaysDispoHashset);
                    outputService.OutputRemovedDispos(removedDispos, TodaysRemovedDisposFile);
                }
                Logger.WriteLog("Application closing.");
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

        public List<Dispensary> IdentifyRemovedDispos(List<Dispensary> yesterdaysDispos, HashSet<string> todaysDispos)
        {
            Logger.WriteLog("Identifying todays removed dispensaries.");
            var removedDispos = new List<Dispensary>();

            foreach (var dispo in yesterdaysDispos)
            {
                if (!todaysDispos.Contains(dispo.LicenseNum))
                {
                    removedDispos.Add(dispo);
                }
            }

            return removedDispos;
        }
    }
}
