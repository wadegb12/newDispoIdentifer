using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace pdfTextReader.Services
{
    class OutputService
    {
        public Logger Logger;
        public OutputService(Logger logger)
        {
            Logger = logger;
        }

        public void OutputTodaysCompanies(List<Dispensary> companies, string outputFileName)
        {
            Logger.WriteLog("Creating JSON list of all dispensaries from today.");

            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
                outputFile.WriteLine(new JavaScriptSerializer().Serialize(companies));
            }
        }

        public void OutputNewDispos(List<Dispensary> newDispos, string outputFileName)
        {
            Logger.WriteLog("Saving new dispensaries to text file.");

            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
                foreach (var dispo in newDispos)
                {
                    outputFile.WriteLine(dispo.ToString());
                }
            }
        }
        // KNB : 6/13/2021 : Add outputting the removed dispensaries
        public void OutputRemovedDispos(List<Dispensary> removedDispos, string outputFileName)
        {
            Logger.WriteLog("Saving new dispensaries to text file.");

            using (StreamWriter outputFile = new StreamWriter(outputFileName))
            {
                foreach (var dispo in removedDispos)
                {
                    outputFile.WriteLine(dispo.ToString());
                }
            }
        }
    }
}
