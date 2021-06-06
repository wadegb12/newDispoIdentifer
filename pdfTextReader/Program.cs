using System;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using pdfTextReader.Common;

namespace pdfTextReader
{
    class Program
    {
        static WebDriver webDriver = new WebDriver(@"C:\Users\kylen\source\repos\wadegb12\newDispoIdentifier\ChomeDriver", @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\2021-06-06");

        static void Main(string[] args)
        {

            Tools.DispensaryDownloader.DownloadNewDispensaries(webDriver);
            Globals.PrefixDateToLatestFile(webDriver);

            Globals.LogIt("Setting Local Variables");

            var directory = @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\";

            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var directoryYesterday = directory + yesterday + @"\";
            var yesterdaysDisposFile = directoryYesterday + yesterday + " Licensed Dispos.txt";

            var dateToday = Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            var directoryToday = directory + dateToday + @"\";
            var dispoListFile = directoryToday + dateToday + " omma_dispensaries_list.pdf";
            var todaysDisposFile = directoryToday + dateToday + " Licensed Dispos.txt";
            var newDisposFile = directoryToday + dateToday + " New Dispos.txt";
            var errorLotFile = @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\Log\error_log.txt";

            try
            {
                PdfReader pdfReader = new PdfReader(dispoListFile);
                var todaysDispos = FindCompanies(pdfReader);
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

        static List<Dispensary> FindCompanies(PdfReader pdfReader)
        {
            Globals.LogIt("Reading all dispensaries from PDF.");
            var allCompanies = new List<Dispensary>();

            for(int i = 1; i<= pdfReader.NumberOfPages; i++)
            {
                var page = new Page(i);
                var curBottom = page.bottomLine;
                
                for(int x = 1; x <= page.GetNumRows(); x++)
                {
                    var top = curBottom + page.GetRowHeight(x);

                    if (NeedsNonFirstPageRowOffset(i, x))
                    {
                        top += 20;
                    }
                    else if(NeedsFirstPageRowOffset(i, x))
                    {
                        top += 10;
                    }

                    var dispo = ReadCompanyInfo(pdfReader, i, page, curBottom, top);
                    if(dispo.LicenseNum != "") allCompanies.Add(dispo);

                    curBottom = top;
                }
            }
            return allCompanies;
        }

        static bool NeedsFirstPageRowOffset(int page, int row)
        {
            return page == 1 && row == 9;
        }

        static bool NeedsNonFirstPageRowOffset(int page, int row)
        {
            return page != 1 && row == 8;
        }

        static Dispensary ReadCompanyInfo(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            var nameColSplitText = GetNameColText(pdfReader, i, page, curBottom, top);
            var name = nameColSplitText[0].Trim();
            var tradeName = "";
            if (nameColSplitText.Length > 1) tradeName = nameColSplitText[1].Trim();

            var licenseNum = GetLicenseColText(pdfReader, i, page, curBottom, top);
            var email = GetEmailColText(pdfReader, i, page, curBottom, top);
            var phoneNum = GetPhoneColText(pdfReader, i, page, curBottom, top);

            var city = GetCityColText(pdfReader, i, page, curBottom, top);
            var zip = GetZipColText(pdfReader, i, page, curBottom, top);
            var county = GetCountyColText(pdfReader, i, page, curBottom, top);

            return new Dispensary(name, tradeName, licenseNum, email, phoneNum, city, zip, county);
        }

        static string[] GetNameColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            var nameColText = GetColumnText(pdfReader, i, page.lName, curBottom, page.rName, top).Replace("\n", "");
            return nameColText.Split(new string[] { " Trade Name: " }, StringSplitOptions.None);
        }

        static string GetLicenseColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lLicNum, curBottom, page.rLicNum, top).Replace("\n", "").Trim();
        }

        static string GetEmailColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lEmail, curBottom, page.rEmail, top).Replace("\n", "").Trim();
        }

        static string GetPhoneColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lPhone, curBottom, page.rPhone, top).Replace("\n", "").Trim();
        }

        static string GetCityColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lCity, curBottom, page.rCity, top).Replace("\n", "").Trim();
        }

        static string GetZipColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lZip, curBottom, page.rZip, top).Replace("\n", "").Trim();
        }

        static string GetCountyColText(PdfReader pdfReader, int i, Page page, int curBottom, int top)
        {
            return GetColumnText(pdfReader, i, page.lCounty, curBottom, page.rCounty, top).Replace("\n", "").Trim();
        }

        static string GetColumnText(PdfReader pdfReader, int page, float llx, float lly, float urx, float ury)
        { 
            // reminder, parameters are in points, and 1 in = 2.54 cm = 72 points
            var rect = new iTextSharp.text.Rectangle(llx, lly, urx, ury);

            RenderFilter[] filter = { new RegionTextRenderFilter(rect) };

            ITextExtractionStrategy strategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter);

            var text = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

            return text;
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
