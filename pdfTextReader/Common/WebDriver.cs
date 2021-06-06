using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
namespace pdfTextReader.Common
{
    public class WebDriver : IDisposable
    {
        private ChromeOptions chromeOptions = new ChromeOptions();
        private IWebDriver driver;
        private int explicitSecondsToWait = 9;
        private WebDriverWait defaultWaitTime;

        public WebDriver(string driverLocation, string downloadFolder)
        {
            try
            {

                Proxy proxy = new Proxy { Kind = ProxyKind.System };

                chromeOptions.AddUserProfilePreference("download.default_directory", downloadFolder);
                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "false");
                chromeOptions.AddUserProfilePreference("prompt_for_download", "false");
                chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
                chromeOptions.AddUserProfilePreference("safebrowsing", "enabled");

                chromeOptions.AddArgument("--disable-infobars");
                chromeOptions.AddArgument("disable-extensions");
                chromeOptions.AddArgument("safebrowsing-disable-download-protection");
                chromeOptions.AddArgument("safebrowsing-disable-extension-blacklist");
                chromeOptions.AddArgument("--incognito"); // ensure we never have stored cookies after this session.
                chromeOptions.AddArgument("window-size=1036,780"); // compatibility with running as non-interactive scheduled task
                chromeOptions.AddArgument("--log-level=OFF"); // keep chromedriver console quiet
                chromeOptions.AddArgument("--silent"); // keep chromedriver console quiet

                chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

                chromeOptions.Proxy = proxy; // use system default proxy settings.

                ChromeDriverService service = ChromeDriverService.CreateDefaultService(driverLocation);
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;

                /* Create driver object with given runtime options */
                driver = new ChromeDriver(service, chromeOptions);

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(240); // set web page load timeout of 2 minutes
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(explicitSecondsToWait); // wait up to XX seconds to find element

                // init the default explicit wait object for class. This will wait for 20 seconds.
                defaultWaitTime = new WebDriverWait(driver, TimeSpan.FromSeconds(explicitSecondsToWait));

            }
            catch
            {
                throw new ArgumentException("Failed to create Driver.");
            }

        }
        public string ToDispensaryPage(string URL)
        {
            driver.Url = URL;
            driver.Navigate();
            WaitForJStoLoad(driver);



            return "Success";
        }
        private bool WaitForJStoLoad(IWebDriver driver, int maxWaitSeconds = 10)
        {
            // This is really not working. It does not itterate through all possible conditions
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            if (js.ExecuteScript("return document.readyState").ToString().Equals("complete"))
            {
                Common.Globals.LogIt("Page [" + driver.Title + "] Has Loaded.");
                return true;
            }
            //This loop will rotate for 25 times to check If page Is ready after every 1 second.
            //You can replace your value with 25 If you wants to Increase or decrease wait time.
            int i;
            for (i = 0; i < maxWaitSeconds; i++)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write(".");
                //To check page ready state.
                if (js.ExecuteScript("return document.readyState").ToString().Equals("complete"))
                {
                    Common.Globals.LogIt("");
                    Common.Globals.LogIt("Page : [" + driver.Title + "] Loaded In " + i.ToString() + " seconds.");
                    break;
                }
            }

            if (i >= maxWaitSeconds)
            {
                Common.Globals.LogIt("ERROR: Page Failed to Load [" + driver.Url.ToLower() + "] within " + maxWaitSeconds.ToString() + " seconds.");
                return false;
            }

            return true;
        }

        public string GetDefaultDownloadDirectory()
        {
            string directory = @"C:\Users\kylen\OneDrive\Desktop\DispoFiles\" + DateTime.Now.ToString("yyyy-MM-dd"); 
            return directory;
        }

        public bool CheckCrashedDriver()
        {
            return driver == null;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
