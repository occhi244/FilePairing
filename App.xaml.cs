using System;
using System.Windows;

namespace FilePairing
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        // Logger
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);


        // Main method
        [STAThreadAttribute()]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Startup += App_Startup;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (!(e.ExceptionObject is Exception exception)) return;

                Logger.Error(exception);
                MessageBox.Show("アプリケーションで例外が発生しました。内容をログファイルに記述し、アプリケーションを終了します。", "システムエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            app.Run();
        }


        /// <summary>
        /// Startup event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow().Show();
        }
    }
}
