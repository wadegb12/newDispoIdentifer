
namespace pdfTextReader
{
    public class Page
    {
        public int PageNum;
        public int rowHeight = 30;
        public int numRows = 15;

        public bool IsFirstPage;
        public int firstPageFirstRowHeight = 35;
        public int firstPageHeight = 30;
        public int firstPageNumRows = 13;
        
        public int bottomLine = 80;

        public int lName = 45;
        public int rName = 280;

        public int lLicNum = 281;
        public int rLicNum = 350;

        public int lEmail = 351;
        public int rEmail = 500;

        public int lPhone = 501;
        public int rPhone = 580;

        public int lCity = 581;
        public int rCity = 650;

        public int lZip = 651;
        public int rZip = 700;

        public int lCounty = 701;
        public int rCounty = 760;

        public Page(int pageNum)
        {
            PageNum = pageNum;
            IsFirstPage = pageNum == 1 ? true : false;
        }

        public int GetNumRows()
        {
            if (IsFirstPage) return firstPageNumRows;

            return numRows;
        }

        public int GetRowHeight(int curRow)
        {
            if (IsFirstPage)
            {
                if (curRow == 1) return firstPageFirstRowHeight;

                return firstPageHeight;
            }

            return rowHeight;
        }

    }
}
