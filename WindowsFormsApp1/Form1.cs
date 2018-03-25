using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class RealyBid : Form
    {
        public RealyBid()
        {
            InitializeComponent();
        }

        private IWebDriver driver;
        private string strUrl = "";
        private Boolean isWorking = false;
        private Thread processThread;
        private List<string> urlCategories = new List<string>();
        private List<string> urlItems = new List<string>();
        private List<string[]> urlItemsDetails = new List<string[]>();
        public int nUrlIndex = 0;
        public int nIndex = 0;
        private void start_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                this.dataGridViewResult.DataSource = null;
                this.dataGridViewResult.Rows.Clear();
                strUrl = tx_url.Text;

                nUrlIndex = Int32.Parse(tx_index.Text);

                processThread = new Thread(getinformation);
                processThread.Start();
                isWorking = true;
                btnStart.Text = "Pause";
                btnStop.Enabled = true;
                btnExport.Enabled = false;
            }
            else if (btnStart.Text == "Pause")
            {
                processThread.Suspend();
                btnStart.Text = "Continue";
                btnExport.Enabled = true;
            }
            else if (btnStart.Text == "Continue")
            {
                processThread.Resume();
                btnStart.Text = "Pause";
                btnExport.Enabled = false;
            }
            else if (btnStart.Text == "Done")
            {
                MessageBox.Show("Please Save Data");
                btnStart.Text = "Start";
            }
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            btnStart.Text = "Start";
            btnStop.Enabled = false;
            btnExport.Enabled = true;
            try
            {
                processThread.Abort();

            }
            catch (Exception) { }
            try
            {
                driver.Quit();
            }
            catch (Exception) { }
        }

        private void getinformation()
        {
            driver = googleChrome();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Size = new Size(1400, 5000);
            driver.Navigate().GoToUrl(strUrl);
            var categories = driver.FindElements(By.XPath("//a[@class='tocCatName catName3']"));
            urlCategories.Clear();
            if (nUrlIndex > categories.Count)
            {
                btnStart.Text = "Start";
                btnStop.Enabled = false;
                btnExport.Enabled = true;
                try
                {
                    processThread.Abort();

                }
                catch (Exception) { }
                try
                {
                    driver.Quit();
                }
                catch (Exception) { }
            }
            for (int i = nUrlIndex-1; i < categories.Count; i++)
            {
                urlCategories.Add(categories[i].GetAttribute("href"));
            }
            for (int i = 0; i < urlCategories.Count; i++)
            {
                newTab(driver, urlCategories[i]);

                Boolean isNext = true;
                do
                {
                    urlItems.Clear();
                    urlItemsDetails.Clear();
                    var items = driver.FindElements(By.XPath("//ol[@class='entryList']//li//span[@class='citation']//a[@target='_blank']"));
                    var itemsTitles = driver.FindElements(By.XPath("//span[@class='articleTitle recTitle']"));
                    var itemsDetails = driver.FindElements(By.XPath("//ol[@class='entryList']//li//span[@class='citation']"));
                    for (int j = 0; j < items.Count; j++)
                    {
                        urlItems.Add(items[j].GetAttribute("href"));
                        string strDetails = itemsDetails[j].Text;
                        strDetails = strDetails.Replace(itemsTitles[j].Text, "");
                        string[] strtemp = Regex.Split(strDetails, " - ");
                        string[] strdetailstemp = new string[4];
                        strdetailstemp[0] = itemsTitles[j].Text;
                        strdetailstemp[1] = strtemp[0];
                        strdetailstemp[1] = strdetailstemp[1].Replace("\r\n", "");
                        strdetailstemp[2] = strtemp[1];
                        strdetailstemp[3] = "";
                        if (strtemp.Length > 2)
                        {
                            for (int k = 2; k < strtemp.Length; k++)
                            {
                                strdetailstemp[3] += strtemp[k] + " ";
                            }
                        }                  
                        urlItemsDetails.Add(strdetailstemp);
                    }
                    for (int j = 0; j < urlItems.Count; j++)
                    {
                        newTab(driver, urlItems[j]);
                        //////////////////////////////////
                        string strGoogleEnter = driver.FindElement(By.XPath("//a[@title='Search on Google Scholar']")).GetAttribute("href");
                        newTab(driver, strGoogleEnter);

                        var captchasFirst = driver.FindElements(By.XPath("//div[@id='gs_captcha_c']"));
                        var captchasurlFirst = driver.Url;
                        while (true)
                        {
                            Thread.Sleep(500);
                            try
                            {
                                if ((captchasFirst.Count != 0) && driver.Url.ToUpper().Equals(captchasurlFirst.ToUpper()))
                                {

                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(1000);
                                if ((captchasFirst.Count != 0) && driver.Url.ToUpper().Equals(captchasurlFirst.ToUpper()))
                                {

                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(500);
                        var captchasFirstAgain = driver.FindElements(By.XPath("//div[@class='rc-anchor rc-anchor-normal rc-anchor-light']"));
                        var captchasurlFirstAgain = driver.Url;
                        while (true)
                        {
                            Thread.Sleep(500);
                            try
                            {
                                if ((captchasFirstAgain.Count != 0) && driver.Url.ToUpper().Equals(captchasurlFirstAgain.ToUpper()))
                                {

                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(500);
                                if ((captchasFirstAgain.Count != 0) && driver.Url.ToUpper().Equals(captchasurlFirstAgain.ToUpper()))
                                {

                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(500);

                        string bookItems = null;
                        try
                        {
                            bookItems = driver.FindElement(By.XPath("(//div[@class='gs_r gs_or gs_scl']//div[@class='gs_ri']//div[@class='gs_fl']//a)[3]")).GetAttribute("href");
                        }
                        catch (Exception) { }
                        if (bookItems != null)
                        {
                            newTab(driver, bookItems);
                            Boolean isNextGoogle = false;
                            //Pass captcha...
                            var captchas = driver.FindElements(By.XPath("//div[@id='gs_captcha_c']"));
                            var captchasurl = driver.Url;
                            while (true)
                            {
                                Thread.Sleep(500);
                                try
                                {
                                    if ((captchas.Count != 0) && driver.Url.ToUpper().Equals(captchasurl.ToUpper()))
                                    {

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(500);
                                    if ((captchas.Count != 0) && driver.Url.ToUpper().Equals(captchasurl.ToUpper()))
                                    {

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            var captchasAgain = driver.FindElements(By.XPath("//div[@class='rc-anchor rc-anchor-normal rc-anchor-light']"));
                            var captchasurlAgain = driver.Url;
                            while (true)
                            {
                                Thread.Sleep(500);
                                try
                                {
                                    if ((captchasAgain.Count != 0) && driver.Url.ToUpper().Equals(captchasurlAgain.ToUpper()))
                                    {
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(500);
                                    if ((captchasAgain.Count != 0) && driver.Url.ToUpper().Equals(captchasurlAgain.ToUpper()))
                                    {

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            Thread.Sleep(500);
                            do
                            {
                                var bookDetails = driver.FindElements(By.XPath("//div[@class='gs_r gs_or gs_scl']//div[@class='gs_ri']//div[@class='gs_a']"));
                                var bookTitles = driver.FindElements(By.XPath("//div[@class='gs_r gs_or gs_scl']//div[@class='gs_ri']//h3[@class='gs_rt']"));
                                for (int k = 0; k < bookDetails.Count; k++)
                                {
                                    string strDetails = bookDetails[k].Text;
                                    string strTitle = bookTitles[k].Text;
                                    //strDetails = strDetails.Replace("- ", "");
                                    var mDetails = Regex.Split(strDetails, " - ");
                                    string strAuthor = mDetails[0] + " ";
                                    string strYear = mDetails[mDetails.Length - 2];
                                    var mYears = strYear.Split(' ');
                                    for (int m = 0; m < mYears.Length - 2; m++)
                                    {
                                        strAuthor += mYears[m] + " ";
                                    }
                                    strYear = mYears[mYears.Length - 1];
                                    string strPublisher = mDetails[mDetails.Length - 1];
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        nIndex++;
                                        dataGridViewResult.ScrollBars = ScrollBars.Vertical | ScrollBars.Horizontal;
                                        dataGridViewResult.Rows.Add(nIndex, strTitle, strAuthor, strYear, strPublisher, 
                                            urlItemsDetails[j][0], urlItemsDetails[j][1], urlItemsDetails[j][2], urlItemsDetails[j][3]);
                                        dataGridViewResult.FirstDisplayedScrollingRowIndex = dataGridViewResult.RowCount - 1;
                                    });
                                }
                                var nextgoogle = driver.FindElements(By.XPath("//td[@align='left']//a"));
                                pageDown(driver);
                                if (nextgoogle.Count != 0)
                                {
                                    isNextGoogle = true;
                                    nextgoogle[0].Click();
                                }
                                else
                                {
                                    isNextGoogle = false;
                                }
                                var captchasSecond = driver.FindElements(By.XPath("//div[@id='gs_captcha_c']"));
                                var captchasurlSecond = driver.Url;
                                while (true)
                                {
                                    Thread.Sleep(500);
                                    try
                                    {
                                        if ((captchasSecond.Count != 0) && driver.Url.ToUpper().Equals(captchasurlSecond.ToUpper()))
                                        {

                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(500);
                                        if ((captchasSecond.Count != 0) && driver.Url.ToUpper().Equals(captchasurlSecond.ToUpper()))
                                        {

                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                var captchasAgainSecond = driver.FindElements(By.XPath("//div[@class='rc-anchor rc-anchor-normal rc-anchor-light']"));
                                var captchasurlAgainSecond = driver.Url;
                                while (true)
                                {
                                    Thread.Sleep(500);
                                    try
                                    {
                                        if ((captchasAgainSecond.Count != 0) && driver.Url.ToUpper().Equals(captchasurlAgainSecond.ToUpper()))
                                        {
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(500);
                                        if ((captchasAgainSecond.Count != 0) && driver.Url.ToUpper().Equals(captchasurlAgainSecond.ToUpper()))
                                        {

                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            } while (isNextGoogle);
                            closeBrowser(driver);
                            Thread.Sleep(1000);
                        }


                        closeBrowser(driver);
                        Thread.Sleep(1000);
                        ////////////////////////////////////
                        closeBrowser(driver);
                    }
                    var nextBtn = driver.FindElements(By.XPath("//span[@onclick='goToNextPage()']"));
                    if (nextBtn.Count != 0)
                    {
                        nextBtn[0].Click();
                        isNext = true;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        isNext = false;
                    }
                } while (isNext);
                Thread.Sleep(1000);
                closeBrowser(driver);
            }
            this.Invoke((MethodInvoker)delegate
            {
                btnStop.Enabled = false;
                btnExport.Enabled = true;
                btnStart.Text = "Done";
            });
            driver.Quit();

        }
        public IWebDriver googleChrome()
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArguments("disable-infobars");               //disable test automation message
            option.AddArguments("--disable-notifications");        //disable notifications
            option.AddArguments("--disable-web-security");         //disable save password windows
            option.AddUserProfilePreference("credentials_enable_service", false);

            option.AddUserProfilePreference("browser.download.manager.showWhenStarting", false);
            option.AddUserProfilePreference("browser.helperApps.neverAsk.saveToDisk", "text/csv");
            option.AddUserProfilePreference("disable-popup-blocking", "true");
            option.AddUserProfilePreference("safebrowsing.enabled", true);
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            IWebDriver driver = new ChromeDriver(driverService, option);
            return driver;
        }

        public void newTab(IWebDriver driver, string url)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open()");
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(500);
        }
        public void Tab(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open()");
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
        }
        public void closeBrowser(IWebDriver driver)
        {
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }
        public void pageDown(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            Thread.Sleep(500);
        }
        public void pageUp(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollTo(0, 0);");
            Thread.Sleep(500);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void export_Click(object sender, EventArgs e)
        {
            // Creating a Excel object.
            Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

            try
            {
                //Getting the location and file name of the excel to save from user.
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "All files (*.*)|*.*|Excel files (*.xlsx)|*.xlsx";
                saveDialog.FilterIndex = 2;

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    worksheet = workbook.ActiveSheet;

                    worksheet.Name = "PhilPapers";

                    int cellRowIndex = 1;
                    int cellColumnIndex = 1;

                    //Loop through each row and read value from each column.
                    for (int i = 0; i <= dataGridViewResult.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridViewResult.Columns.Count; j++)
                        {
                            // Excel index starts from 1,1. As first Row would have the Column headers, adding a condition check.
                            if (i == 0)
                            {
                                worksheet.Cells[cellRowIndex, cellColumnIndex] = dataGridViewResult.Columns[j].HeaderText;
                            }
                            else
                            {
                                worksheet.Cells[cellRowIndex, cellColumnIndex] = dataGridViewResult.Rows[i - 1].Cells[j].Value.ToString();
                            }
                            cellColumnIndex++;
                        }
                        cellColumnIndex = 1;
                        cellRowIndex++;
                    }
                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Export Successful");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                excel.Quit();
                workbook = null;
                excel = null;
            }
        }
    }
}
