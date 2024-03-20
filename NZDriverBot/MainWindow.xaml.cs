using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NZDriverBot.Common;
using NZDriverBot.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;
using System.Net.Http;
using System.Text;
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
        private List<OverseasConversion> outstandingTests = new();
        private Reserve reserve = new();
        private DateTime selectedDate;
        private int checkTimes = 0;

        private IWebDriver driver;

        const string BASE_URL = "https://online.nzta.govt.nz/";


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
#if !DEBUG
            service.HideCommandPromptWindow = true;
#endif
            service.LogPath = "program.log";

            ChromeOptions options = new ChromeOptions();
#if DEBUG
            string path = Path.Combine(Environment.CurrentDirectory, @"..\\..\\..\\chrome-win64\", "chrome.exe");
#else
            string path = Path.Combine(Directory.GetCurrentDirectory(), @".\chrome-win64\", "chrome.exe");
            options.AddArgument("--headless");
#endif
            options.BinaryLocation = path;
            options.AddArguments("--disable-extensions"); // to disable extension
            options.AddArguments("--disable-notifications"); // to disable notification
            options.AddArguments("--disable-application-cache"); // to disable cache
            options.AddArguments("--verbose"); // Enable verbose logging
            options.AddArgument("userAgent=Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0");
            options.AddArgument("C:\\driver.log");
            options.AddArgument("--log-level=ALL");

            options.SetLoggingPreference(LogType.Browser, LogLevel.All);
            options.SetLoggingPreference(LogType.Client, LogLevel.All);
            options.SetLoggingPreference(LogType.Driver, LogLevel.All);
            options.SetLoggingPreference(LogType.Profiler, LogLevel.All);
            options.SetLoggingPreference(LogType.Server, LogLevel.All);

            new DriverManager().SetUpDriver(new ChromeConfig());

            driver = new ChromeDriver(service, options);
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

            if (bookingFromDatePicker.Equals(string.Empty) || bookingToDatePicker.Equals(string.Empty))
            {
                outputMessage += bookingDatePickerErrorContent;
            }


            if (outputMessage != "")
            {
                MessageBox.Show(outputMessage);
                return;
            }
            button.IsEnabled = false;
            resultTxt.Text = "Preparing the runtime environment...";
            _ = Login();
        }

        private async Task Login()
        {
            try
            {
                driver.Url = "https://online.nzta.govt.nz/licence-test/identification";
                //driver.Navigate().GoToUrl("https://online.nzta.govt.nz/licence-test/identification");
                Console.WriteLine("God is running for " + driver.Title);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h1[normalize-space()='Book a practical driver licence test']")));
                wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. AB123456']"))).SendKeys(licenseNumberTxt.Text);
                wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. 470']"))).SendKeys(licenseVersionTxt.Text);
                wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. Smith']"))).SendKeys(nameTxt.Text);
                wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='e.g. 24-03-1981']"))).SendKeys(birthDatePicker.SelectedDate!.Value.ToString("dd-MM-yyyy"));

                var buttonContinue = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@id='btnContinue']")));
                if (buttonContinue.Displayed)
                    buttonContinue.Click();
                Thread.Sleep(5000);

                IWebElement? errorElement = null;
                errorElement = wait.Until<IWebElement>(d =>
                {
                    try
                    {
                        return d.FindElement(By.XPath("//h3[normalize-space()='Full Car (Class 1)']"));
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }


                });

                if (errorElement == null)
                {
                    MessageBox.Show("Error: It looks like you've entered the wrong details. Please close software and try again.");
                    button.IsEnabled = true;
                    return;
                }


                Thread.Sleep(100);

                await GetAuthenticationAsync();
                await GetEligibilityAsync();
                await GetBookingsAsync();
                if (bookings.Count > 0)
                {
                    MessageBox.Show("You already have a booking, please cancel it manually first.");
                    return;
                }
                await GetDLBookingAsync();
                outstandingTests = await GetOverseasConversionAsync();
                await GetNewLicenceClassAsync();

                string result = string.Empty;
                List<Slot> availableSlots = new();
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(50));
                    checkTimes++;
                    resultTxt.Text = "Please wait, checking for available slots " + checkTimes + (checkTimes > 1 ? " times" : " time") + " for you...";
                    if (checkTimes % 8 == 0)
                    {
                        driver.Navigate().Refresh();
                    }

                    var availableSitesOnDays = await CheckAvailableSiteAsync(bookingFromDatePicker.SelectedDate!.Value.ToString("dd/MM/yyyy"), bookingToDatePicker.SelectedDate!.Value.ToString("dd/MM/yyyy"));
                    foreach (var site in availableSitesOnDays!)
                    {
                        selectedDate = site.slotDate;
                        availableSlots = await GetSiteListAsync(site.siteId, selectedDate.ToString("dd/MM/yyyy"));

                        foreach (var slot in availableSlots)
                        {
                            if (timeComboBox.SelectedIndex == 0)
                            {
                                if (slot.isMorning)
                                {
                                    //MessageBox.Show("Congratulations! Found available slot " + slot.siteId + "in the Morning!");
                                    result = await BookAndConfirm(slot);
                                    break;
                                }
                                else { continue; }
                            }
                            else if (timeComboBox.SelectedIndex == 1)
                            {
                                if (!slot.isMorning)
                                {
                                    //MessageBox.Show("Congratulations! Found available slot " + slot.siteId + "in the Afternoon!");
                                    result = await BookAndConfirm(slot);
                                    break;
                                }
                                else { continue; }
                            }
                        }

                        if (!string.IsNullOrEmpty(result))
                        {
                            break;
                        }
                    }


                } while (availableSlots.Count <= 0 && string.IsNullOrEmpty(result));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login Error: " + ex.Message);
                button.IsEnabled = true;
            }
        }


        //first fetch authentication api.
        private async Task<string?> GetAuthenticationAsync()
        {
            string url = BASE_URL + "/api/authentication";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("authentication=" + responseContent);
            return responseContent;
        }

        private async Task<string?> GetEligibilityAsync()
        {
            string url = BASE_URL + "/api/licence-test/eligibility";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("eligibility=" + responseContent);
            return responseContent;
        }


        private async Task<List<Booking>> GetBookingsAsync()
        {
            string url = BASE_URL + "/api/licence-test/bookings";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("bookings=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            bookings = obj["bookings"].ToObject<List<Booking>>();
            return bookings!;
        }

        private async Task<string?> GetDLBookingAsync()
        {
            string url = BASE_URL + "/api/managedcontent/DL/DL-Booking";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("DLBooking=" + responseContent);
            return responseContent;
        }

        private async Task<List<OverseasConversion>?> GetOverseasConversionAsync()
        {
            string url = BASE_URL + "/api/licence-test/eligibility/OverseasConversion";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("OverseasConversion=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            outstandingTests = obj["outstandingTests"].ToObject<List<OverseasConversion>>();
            return outstandingTests;
        }

        private async Task<string?> GetNewLicenceClassAsync()
        {
            string url = BASE_URL + "/api/licence-test/eligibility/NewLicenceClass";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("NewLicenceClass=" + responseContent);
            return responseContent;
        }

        private async Task<List<SlotAvailability>?> CheckAvailableSiteAsync(string fromDate, string toDate)
        {
            string url = BASE_URL + "/api/licence-test/slots/availability/Class1F?siteId=" + selectedSiteId + "&dateFrom=" + fromDate + "&dateTo=" + toDate;
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("AvailableSite=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var list = obj["slotAvailability"].ToObject<List<SlotAvailability>>();
            return list;
        }

        private async Task<List<Slot>?> GetSiteListAsync(string siteId, string date)
        {
            string url = BASE_URL + "/api/licence-test/slots/Class1F/" + siteId + "?slotDate=" + date;
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("SiteList=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var availableSlots = obj["slots"].ToObject<List<Slot>>();
            return availableSlots;
        }

        private async Task<Reserve?> PostReserveAsync(Slot slot)
        {
            string url = BASE_URL + "/api/licence-test/bookings/reserve";
            var bodyObject = new ReserveBody
            {
                applicationId = outstandingTests.FirstOrDefault().applicationId.ToString(),
                applicationType = outstandingTests.FirstOrDefault().applicationType,
                hasAdvancedDriverCertificate = "false",
                isReschedule = bookings.Count > 0,
                licenceClass = outstandingTests.FirstOrDefault().entitlementValue,
                siteId = slot.siteId,
                stage = outstandingTests.FirstOrDefault()?.stage ?? "",
                testType = outstandingTests.FirstOrDefault()?.testType ?? "",
                when = slot.startDateTime!.ToString("yyyy-MM-ddTHH:mm:ss")
            };
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Post, url, JsonConvert.SerializeObject(bodyObject));
            Console.WriteLine("Reserve=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            reserve = obj.ToObject<Reserve>();
            return reserve;
        }

        private async Task<BookingDetail?> GetBookingByIdAsync()
        {
            string url = BASE_URL + "/api/licence-test/bookings/" + reserve.bookingId + "?$expand=$application";//"?$expand=$fee";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("BookingById=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var bookingDetail = obj["booking"].ToObject<BookingDetail>();
            return bookingDetail;
        }

        private async Task<Contact?> GetContactAsync()
        {
            string url = BASE_URL + "/api/driver/contact";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("GetContact=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var contact = obj["driver"].ToObject<Contact>();
            return contact;
        }

        private async Task GetPingAsync()
        {
            string url = BASE_URL + "/api/authentication/PING";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Get, url, "");
            Console.WriteLine("GetPing=" + responseContent);
            return;
        }

        private async Task<string> PostConfirmAsync()
        {
            string url = BASE_URL + "/api/licence-test/bookings/reserve/" + reserve.bookingId + "/confirm";
            string? responseContent = await SendHttpRequestAsync(HttpMethod.Post, url, "");
            Console.WriteLine("PostConfirm=" + responseContent);
            JObject obj = JObject.Parse(responseContent!);
            var result = obj["bookingId"].ToString();
            return result;
        }


        private async Task<string?> SendHttpRequestAsync(HttpMethod type, string url, string requestBody)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(type, url);

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
                request.Headers.Add("Cookie", cookieString);

                if (!string.IsNullOrEmpty(requestBody))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }

                try
                {
                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseBody);
                        return responseBody;
                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        MessageBox.Show("Request failed with status code: " + response.StatusCode);
                        button.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendHttpRequestAsync Error: " + ex.Message);
                    MessageBox.Show(ex.Message);
                    button.IsEnabled = true;
                }
            }
            return null;
        }

        private async Task<string> BookAndConfirm(Slot slot)
        {
            await PostReserveAsync(slot);
            var bookingDetail = await GetBookingByIdAsync();
            var contract = await GetContactAsync();
            var result = await PostConfirmAsync();
            if (result != null)
            {
                string emailSubject = "🎉 NZ Driver Bot: Application Reserved Successfully 🎉";
                string emailBody = $@"
                        <!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Your Email Subject</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    background-color: #fff;
                                    padding: 20px;
                                    border-radius: 5px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                }}
                                h1 {{
                                    color: #333;
                                }}
                                p {{
                                    color: #666;
                                    line-height: 1.6;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <h1>Congratulations!</h1>
                                <p>Application {bookingDetail!.id} was reserved successfully!</p>
                                <p>Test type: {bookingDetail.test}</p>
                                <p>Site: {bookingDetail.site}</p>
                                <p>Address: {bookingDetail.address}</p>
                                <p>Test time: {bookingDetail.date.ToString("yyyy-MM-dd HH:mm:ss")}</p>
                            </div>
                        </body>
                        </html>
                    ";


                // 发送邮件给用户
                EMailHelper.SentEmailAsync(contract.emailAddress, emailSubject, emailBody);

                MessageBox.Show($"Congratulations! Application {bookingDetail.id} was reserved successfully!\r\nTest type:{bookingDetail.test}\r\nSite:{bookingDetail.site}\r\nAddress:{bookingDetail.address}\r\nTest time:{bookingDetail.date.ToString("yyyy-MM-dd HH:mm:ss")}");
                button.IsEnabled = true;
            }
            return result;
        }
    }
}