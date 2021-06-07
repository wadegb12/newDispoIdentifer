using System;
using System.Collections.Generic;
using pdfTextReader.Common;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace pdfTextReader.Services
{
    class DispensaryListReader
    {
        public string FileToReadFrom;
        public Logger Logger;
        public DispensaryListReader(string fileToReadFrom, Logger logger)
        {
            FileToReadFrom = fileToReadFrom;
            Logger = logger;
        }

        public List<Dispensary> FindCompanies()
        {
            Logger.WriteLog("Reading all dispensaries from PDF.");
            PdfReader pdfReader = new PdfReader(FileToReadFrom);
            var allCompanies = new List<Dispensary>();

            for (int i = 1; i <= pdfReader.NumberOfPages; i++)
            {
                var page = new Page(i);
                var curBottom = page.bottomLine;

                for (int x = 1; x <= page.GetNumRows(); x++)
                {
                    var top = curBottom + page.GetRowHeight(x);

                    if (NeedsNonFirstPageRowOffset(i, x))
                    {
                        top += 20;
                    }
                    else if (NeedsFirstPageRowOffset(i, x))
                    {
                        top += 10;
                    }

                    var dispo = ReadCompanyInfo(pdfReader, i, page, curBottom, top);
                    if (dispo.LicenseNum != "") allCompanies.Add(dispo);

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
    }
}
