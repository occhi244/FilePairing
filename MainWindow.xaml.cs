using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Path = System.IO.Path;

namespace FilePairing
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
		}


		/// <summary>
		/// メイン処理
		/// </summary>
		private void MainWindow_OnContentRendered(object sender, EventArgs e)
		{
			// Input folders
			var folderWindow = new FolderSelectWindow
			{
				Owner = this
			};
			if (folderWindow.ShowDialog() != true)
			{
				Close();
				return;
			}

			// Pairing
			var pairingWindow = new FilePairingWindow
			{
				Owner = this,
				MainPath = folderWindow.ViewModel.MainFolderName,
				SubPath = folderWindow.ViewModel.SubFolderName
			};
			if (pairingWindow.ShowDialog() != true)
			{
				Close();
				return;
			}
			GC.Collect();

			// Set filenames
			var filenameWindow = new FilenameInputWindow
			{
				Owner = this,
				MainPath = pairingWindow.MainPath,
				SubPath = pairingWindow.SubPath
			};
			if (filenameWindow.ShowDialog() != true)
			{
				Close();
				return;
			}

			RenameFiles(filenameWindow.ViewModel.MainFilename, filenameWindow.ViewModel.SubFilename, pairingWindow.ViewModel.MainViewFiles);
			Close();
		}


		/// <summary>
		/// ファイル名変更
		/// </summary>
		/// <param name="mainFilename">メイン・ベースファイル名</param>
		/// <param name="subFilename">サブ・ベースファイル名</param>
		/// <param name="pairDataList">ペアデータのリスト</param>
		private static void RenameFiles(string mainFilename, string subFilename, Collection<PairData> pairDataList)
		{
			var count = 0;
			var digit = pairDataList.Count.ToString().Length;

			foreach (var pairData in pairDataList)
			{
				if (string.IsNullOrEmpty(pairData.SubFile)) continue;

				var suffix = $"{++count}".PadLeft(digit, '0');

				Rename(pairData.MainFile, mainFilename, suffix);
				Rename(pairData.SubFile, subFilename, suffix);
			}
		}


		/// <summary>
		/// ファイル名変更
		/// </summary>
		/// <param name="source">変更元ファイル名 (フルパス)</param>
		/// <param name="baseName">変更先 ベース名</param>
		/// <param name="count">シーケンス番号</param>
		private static void Rename(string source, string baseName, string count)
		{
			var path = Path.GetDirectoryName(source);
			var ext = Path.GetExtension(source);

			File.Move(source, $@"{path}\{baseName}-{count}{ext}");
		}
	}
}
