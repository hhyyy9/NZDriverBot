using System.IO;
using System.Reflection;
using System.Windows;


namespace NZDriverBot
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public string BuildInfo { get; set; }
        public string BuildTime { get; set; }

        public AboutDialog()
        {
            InitializeComponent();
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            BuildInfo = $"Version {version} beta";
            BuildTime = GetBuildTime();

            DataContext = this;
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private string GetBuildTime()
        {
            // 获取程序集的构建时间
            DateTime buildTime = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
            return buildTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
