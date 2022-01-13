using System.IO;
using System.Linq;
using System.Windows;

namespace FilePairing
{
	/// <summary>
	/// FilenameInputWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FilenameInputWindow : Window
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
		/// メインフォルダ
		/// </summary>
		public string MainPath
		{
			get;
			set;
		}


		/// <summary>
		/// サブフォルダ
		/// </summary>
		public string SubPath
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


			ViewModel = new FilenameInputViewModel
			{
				MainFilename = "作業前",
				SubFilename = "作業後"
			};
			DataContext = ViewModel;
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

			if (Directory.GetFiles(MainPath)
				.FirstOrDefault(i => Path.GetFileName(i).StartsWith($"{ViewModel.MainFilename}-")) != null)
			{
				MessageBox.Show($"メインファイルのフォルダに、[{ViewModel.MainFilename}]で始まるファイルが既に存在しています", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (Directory.GetFiles(SubPath)
				.FirstOrDefault(i => Path.GetFileName(i).StartsWith($"{ViewModel.SubFilename}-")) != null)
			{
				MessageBox.Show($"サブファイルのフォルダに、[{ViewModel.SubFilename}]で始まるファイルが既に存在しています", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
