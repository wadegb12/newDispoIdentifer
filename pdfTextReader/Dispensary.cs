
namespace pdfTextReader
{
    public class Dispensary
    {
        public string Name { get; set; }
        public string TradeName { get; set; }
        public string LicenseNum { get; set; }
        public string Email { get; set; }
        public string PhoneNum { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string County { get; set; }

        public Dispensary(string name, string tradeName = "", string licenseNum = "", string email = "", string phoneNum = "", string city = "", string zip = "", string county = "")
        {
            Name = name;
            TradeName = tradeName;
            LicenseNum = licenseNum;
            Email = email;
            PhoneNum = AddSpecialCharsInPhoneNum(phoneNum);
            City = city;
            Zip = zip;
            County = county;
        }

        private string AddSpecialCharsInPhoneNum(string phoneNum)
        {
            if (phoneNum.Length < 7) return phoneNum;

            phoneNum = phoneNum.Insert(6, "-");
            phoneNum = phoneNum.Insert(3, ") ");
            phoneNum = phoneNum.Insert(0, "(");

            return phoneNum;
        }

        public override string ToString()
        {
            return "Name: " + Name + System.Environment.NewLine + "Trade name: " + TradeName + System.Environment.NewLine + "License Number: " + LicenseNum + System.Environment.NewLine +
                "Email: " + Email + System.Environment.NewLine + "PhoneNum: " + PhoneNum + System.Environment.NewLine + "City: " + City + System.Environment.NewLine +
                "Zip: " + Zip + System.Environment.NewLine + "County: " + County + System.Environment.NewLine;
        }

  
    }
}
