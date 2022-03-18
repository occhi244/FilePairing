using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace FilePairing
{
    /// <summary>
    /// FilenameInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FilenameInputWindow
	{
		/// <summary>
		/// データビューモデル
		/// </summary>
		public FilenameInputViewModel ViewModel
		{
			get;
			set;
		}


		/// <summary>
		/// マッチングファイル
		/// </summary>
        public ICollection<PairData> MatchingFileCollection
        {
            get;
            set;
        }



		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FilenameInputWindow()
		{
			InitializeComponent();
		}


		/// <summary>
		/// コンテキストを初期化
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void FilenameInputWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            FirstFilenames(out var mainFile, out var subFile);

            ViewModel = new FilenameInputViewModel
            {
                MainFilename = ExtractBasename(mainFile, "作業前"),
                SubFilename = ExtractBasename(subFile, "作業後")
			};

            DataContext = ViewModel;
		}


		/// <summary>
		/// マッチング済みのファイル名を得る
		/// </summary>
		/// <param name="mainFile">メインファイル名</param>
		/// <param name="subFile">サブファイル名</param>
        private void FirstFilenames(out string mainFile, out string subFile)
        {
            mainFile = string.Empty;
            subFile = string.Empty;

            foreach (var pairData in MatchingFileCollection)
            {
				if (string.IsNullOrEmpty(mainFile) && !string.IsNullOrEmpty(pairData.MainFile))
                {
                    mainFile = pairData.MainFile;
                }
                if (string.IsNullOrEmpty(subFile) && !string.IsNullOrEmpty(pairData.SubFile))
                {
                    subFile = pairData.SubFile;
                }

                if (!string.IsNullOrEmpty(mainFile) && !string.IsNullOrEmpty(subFile))
                {
                    break;
                }
			}
		}


		/// <summary>
		/// ベースファイル名を得る
		/// </summary>
		/// <param name="fullname"></param>
		/// <param name="whenNotFound"></param>
		/// <returns></returns>
        private static string ExtractBasename(string fullname, string whenNotFound)
        {
            var body = Path.GetFileNameWithoutExtension(fullname);

            var pattern = body.StartsWith(MainWindow.ExclFilePrefix) ? $@"^{MainWindow.ExclFilePrefix}(.+)(_\d+)$" : @"(.+)(-\d+)(_\d+)?$";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var mm = regex.Match(body);

            return (!mm.Success) ? whenNotFound : mm.Groups[1].ToString();
        }



		/// <summary>
		/// OK ボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(ViewModel.MainFilename))
			{
				MessageBox.Show("メインファイル名がセットされていません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (string.IsNullOrEmpty(ViewModel.SubFilename))
			{
				MessageBox.Show("サブファイル名がセットされていません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			DialogResult = true;
			Close();
		}
    }


	/// <summary>
	/// ファイル名データビューモデル
	/// </summary>
	public class FilenameInputViewModel
	{
		public string MainFilename
		{
			get;
			set;
		}

		public string SubFilename
		{
			get;
			set;
		}
	}
}
