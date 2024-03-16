using AngleSharp.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NZDriverBot.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Site = NZDriverBot.Models.Site;

namespace NZDriverBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Area> areas = new();
        private List<Site> sites = new();
        private string selectedSiteId = string.Empty;
        private List<Booking> bookings = new();


        private IWebDriver driver;


        public MainWindow()
        {
            InitializeComponent();
            LoadAreas();
            Setup();
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            driver.Close();
            driver.Quit();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Owner = this;
            aboutDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutDialog.ShowDialog();
        }

        private void Setup()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            ChromeOptions options = new ChromeOptions();
#if DEBUG
            string path = Path.Combine(Environment.CurrentDirectory, @"..\\..\\..\\chrome-win64\", "chrome.exe");
#else
            string path = Path.Combine(Directory.GetCurrentDirectory(), @".\\chrome-win64\", "chrome.exe"); //获取应用程序的当前工作目录
            options.AddArgument("--headless");
#endif
            options.BinaryLocation = path;
            options.AddArguments("--disable-extensions"); // to disable extension
            options.AddArguments("--disable-notifications"); // to disable notification
            options.AddArguments("--disable-application-cache"); // to disable cache
            //options.SetLoggingPreference(OpenQA.Selenium.LogType.Server, LogLevel.All);
            options.AddArgument("userAgent=Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0");


            new DriverManager().SetUpDriver(new ChromeConfig());

            driver = new ChromeDriver(service, options);
            driver.Url = "https://online.nzta.govt.nz/licence-test/identification";
            Console.WriteLine("God is running for " + driver.Title);
        }


        private void LoadAreas()
        {
            string jsonText = File.ReadAllText("Resources//areas.json");
            areas = JsonConvert.DeserializeObject<List<Area>>(jsonText);
            foreach (var area in areas)
            {
                AreaComboBox.Items.Add(area.Name);
            }
        }

        private void LoadSites(string locationId)
        {
            string jsonText = File.ReadAllText("Resources//sites.json");
            var _sites = JsonConvert.DeserializeObject<List<Site>>(jsonText);
            sites.Clear();
            foreach (var site in _sites)
            {
                if (site.LocationId.Equals(locationId))
                {
                    sites.Add(site);
                    SiteComboBox.Items.Add(site.Description);
                }
            }
        }



        private void AreaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocationComboBox.Items.Clear();
            SiteComboBox.Items.Clear();
            int selectedIndex = AreaComboBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var selectedArea = areas[selectedIndex];
                foreach (var location in selectedArea.Locations)
                {
                    LocationComboBox.Items.Add(location.Name);
                }
            }
        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SiteComboBox.Items.Clear();
            int selectedIndex = AreaComboBox.SelectedIndex;
            int selectedLocationIndex = LocationComboBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedLocationIndex >= 0)
            {
                var selectedArea = areas[selectedIndex];
                var selectedLocation = selectedArea.Locations[selectedLocationIndex];
                LoadSites(selectedLocation.Id);
            }
        }

        private void SiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Here you can handle the selection change of SiteComboBox if needed
            int selectedSiteIndex = SiteComboBox.SelectedIndex;
            if (selectedSiteIndex >= 0)
            {
                var selectedSite = sites[selectedSiteIndex];
                selectedSiteId = selectedSite.Id;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (InternetConnetedStatusHelper.GetNetConStatus("http://www.google.com") == 1)
            {
                MessageBox.Show("Need network support, please check your computer network connection.");
                return;
            }
            var outputMessage = "";
            var siteErrorContent = "Please select a site.\n\r";
            var licenseErrorContent = "Please input a valid NZ License Number.\n\r";
            var licenseVersionErrorContent = "Please input a valid NZ License Version.\n\r";
            var nameTxtErrorContent = "Please input your Last Name.\n\r";
            var birthDatePickerErrorContent = "Please select your Date of Birth.\n\r";
            var bookingDatePickerErrorContent = "Please select a date for testing.\n\r";

            if (licenseNumberTxt.Text.Equals(""))
            {
                outputMessage += licenseErrorContent;
            }

            if (licenseVersionTxt.Text.Equals(""))
            {
                outputMessage += licenseVersionErrorContent;
            }

            if (nameTxt.Text.Equals(""))
            {
                outputMessage += nameTxtErrorContent;
            }

            if (birthDatePicker.SelectedDate.Equals(""))
            {
                outputMessage += birthDatePickerErrorContent;
            }

            if (selectedSiteId.Equals(string.Empty))
            {
                outputMessage += siteErrorContent;
            }

            if (bookingDatePicker.Equals(string.Empty))
            {
                outputMessage += bookingDatePickerErrorContent;
            }


            if (outputMessage != "")
            {
                MessageBox.Show(outputMessage);
                return;
            }
            Login();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 隐藏加载状态
            LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async void Login()
        {
            
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h1[normalize-space()='Book a practical driver licence test']")));
            wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. AB123456']"))).SendKeys(licenseNumberTxt.Text);
            wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. 470']"))).SendKeys(licenseVersionTxt.Text);
            wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. Smith']"))).SendKeys(nameTxt.Text);
            wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. 24-03-1981']"))).SendKeys(birthDatePicker.SelectedDate!.Value.ToString("dd-MM-yyyy"));
            var button = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='btnContinue']")));
            if (button.Displayed)
                button.Click();
            Thread.Sleep(5000);

            //IWebElement payElement = wait.Until(ExpectedConditions.ElementExists(By.XPath("//dt[normalize-space()='Fees payable:']")));

            var errorElement = wait.Until<IWebElement>(d =>
            {
                var element = d.FindElement(By.XPath("//span[normalize-space()='contact us.']"));
                if (element != null)
                    return element;
                else
                    return null;
            });

            if (errorElement.Displayed)
            {
                MessageBox.Show("Error: " + "It looks like you've entered the wrong details.Please check the information you've entered in each of the fields and try again.");
                return;
            }

            var nextButton = wait.Until<IWebElement>(d =>
            {
                var rButton = d.FindElement(By.XPath("//button[@id='btnContinue']")).Text.Contains("Reschedule");
                var bButton = d.FindElement(By.XPath("//button[@id='btnContinue']")).Text.Contains("Book");
                if (rButton || bButton)
                    //return ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='btnContinue']"));
                    return d.FindElement(By.XPath("//button[@id='btnContinue']"));
                else
                    return null;
            });
            if (nextButton != null)
                nextButton.Click();
            else
            {
                MessageBox.Show("Internal Error! Please report to yu2@me.com");
                return;
            }

            List<Slot> availableSlots = new();
            Task loadingTask = Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    //loadingWindow.Owner = this;
                    //loadingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    //loadingWindow.ShowDialog();
                    // 显示加载状态
                    LoadingOverlay.Visibility = System.Windows.Visibility.Visible;
                });
            });

            await Task.Delay(100); // 等待一段时间确保 loadingWindow 已经显示

            await GetAuthenticationAsync();
            await GetEligibilityAsync();
            var bookInfo = await GetBookingsAsync();
            await GetDLBookingAsync();
            await GetOverseasConversionAsync();
            await GetNewLicenceClassAsync();

            
            do
            {
                availableSlots = await GetSiteListAsync(bookingDatePicker.SelectedDate!.Value.ToString("dd/MM/yyyy"));

                // 如果没有可用的时间槽，等待一分钟后继续尝试
                if (availableSlots!.Count <= 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            } while (availableSlots.Count <= 0);

            if (availableSlots.Count > 0)
            {
                MessageBox.Show("Congratulations! Found " + availableSlots.Count.ToString() + "available slots!");
                // 在 UI 线程上关闭 loadingWindow
                Dispatcher.Invoke(() =>
                {
                    //loadingWindow.Close();
                    LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
                });
            }

            await loadingTask; // 等待 loadingWindow 关闭
        }

        //first fetch authentication api.
        private async Task<string?> GetAuthenticationAsync()
        {
            string url = "https://online.nzta.govt.nz/api/authentication";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("authentication=" + responseContent);
            return responseContent;
        }

        private async Task<string?> GetEligibilityAsync()
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/eligibility";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("eligibility=" + responseContent);
            return responseContent;
        }


        private async Task<List<Booking>> GetBookingsAsync()
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/bookings";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("bookings=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            bookings = obj["bookings"].ToObject<List<Booking>>();
            return bookings!;
        }

        private async Task<string?> GetDLBookingAsync()
        {
            string url = "https://online.nzta.govt.nz/api/managedcontent/DL/DL-Booking";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("DLBooking=" + responseContent);
            return responseContent;
        }

        private async Task<string?> GetOverseasConversionAsync()
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/eligibility/OverseasConversion";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("OverseasConversion=" + responseContent);
            return responseContent;
        }

        private async Task<string?> GetNewLicenceClassAsync()
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/eligibility/NewLicenceClass";
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("NewLicenceClass=" + responseContent);
            return responseContent;
        }

        private async Task<List<SlotAvailability>?> CheckAvailableSiteAsync(string fromDate, string toDate)
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/slots/availability/Class1F?siteId="+selectedSiteId+"&dateFrom="+fromDate+"&dateTo="+toDate;
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("AvailableSite=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var list = obj["slotAvailability"].ToObject<List<SlotAvailability>>();
            return list;
        }

        private async Task<List<Slot>?> GetSiteListAsync(string date)
        {
            string url = "https://online.nzta.govt.nz/api/licence-test/slots/Class1F/"+ selectedSiteId + "?slotDate="+date;
            string? responseContent = await SendHttpRequestAsync(url);
            Console.WriteLine("SiteList=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var availableSlots = obj["slots"].ToObject<List<Slot>>();
            return availableSlots;
        }


        private async Task<string?> SendHttpRequestAsync(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // 创建 HttpRequestMessage 对象
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // 添加自定义的 header
                request.Headers.Add("Host", "online.nzta.govt.nz");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Referer", url);
                request.Headers.Add("api-version", "1.0");
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("Pragma", "no-cache");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Sec-Fetch-Dest", "empty");
                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("TE", "trailers");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Sec-GPC", "1");

                var cookies = driver.Manage().Cookies.AllCookies;
                string cookieString = "";
                foreach (var cookie in cookies)
                {
                    Console.WriteLine(cookie.Name + " : " + cookie.Value);
                    cookieString += cookie.Name + "=" + cookie.Value + ";";
                }
                //cookieString += "ApplicationCookie=CfDJ8GP7WLeouNFHlEgjVfPM7m5WP5WGItTe5l3MOWsLf4s-jbKd5BxTXQrQQYyFRx4ALli1h1hISM-RieetEWR2GbhJ0Gy9ZRjpcICqYK9lOvWnDS0VVMaQcXthpls2DdUl5qVchlE4be6sL3DVzxPwsdLnxiiMAExgIXg930tPSSG1Rp7FhJ7WPmW7atBpg5kLVT10cLmA4CY4UBV_gCV4tDlpbJTHx2zXZopdR7lzz0O34JT12Ir3F0G-YI4h-nDa2od7nYkAuHQRzHfOl5g0UnBuh_NJfSf3ZbOKrnNn7MKYIS12vO4uq70IJtkMJ05RAAXBfq5Gbl8-iUuyhHYLUAdmx_f1quj64mO-IvgZ-WagD_eXxdqaYMwumd876gXgUZpPhFVT43JGBGlVCh15ckAvyJQkMmpH_xZGUO4WOjlxHMkKbc3nednyW7LAhxjn0Q7fZ0C9RbAlvfVPLpKmc2juVFMPciG7d4VIAIieC1X8epNFLUx8GVSFPFuW3zhjUYmMYZ_lOdfsavR5ibwYSdwkCUdZW5IiNf7_yMw1Jb7k0yhdA7BTe5gYft0QNBnax7QWr5M6jf9tyseQCKOEdVxruNZf4wNp6Yo08A96JX0Kh3yujKGyJ3Q5-gJEde6EOGL8D_aOgalWWdMXG6H3b4s";
                request.Headers.Add("Cookie", cookieString);

                try
                {
                    // 发送请求并等待响应
                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 处理成功的响应
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseBody);
                        return responseBody;
                    }
                    else
                    {
                        // 处理错误响应
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        MessageBox.Show("Request failed with status code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常情况
                    Console.WriteLine("Error: " + ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
            return null;
        }
    }
}