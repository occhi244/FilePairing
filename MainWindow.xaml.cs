using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Path = System.IO.Path;

namespace FilePairing
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow
    {
		/// <summary>
		/// 一時ファイル名 ベース
		/// </summary>
        private const string TempBaseName = ".$temp";



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
		/// <param name="mainBaseFilename">メイン・ベースファイル名</param>
		/// <param name="subMainFilename">サブ・ベースファイル名</param>
		/// <param name="pairDataList">ペアデータのリスト</param>
		private static void RenameFiles(string mainBaseFilename, string subMainFilename, Collection<PairData> pairDataList)
		{
			var count = 0;
			var digit = pairDataList.Count.ToString().Length;
            var mainTempFilenames = new List<string>(pairDataList.Count);
            var subTempFilenames = new List<string>(pairDataList.Count);

			foreach (var pairData in pairDataList)
			{
				if (string.IsNullOrEmpty(pairData.SubFile)) continue;

				var fileSequence = $"{++count}".PadLeft(digit, '0');

				mainTempFilenames.Add(RenameToTemp(pairData.MainFile, fileSequence, string.Empty));
				subTempFilenames.Add(RenameToTemp(pairData.SubFile, fileSequence, string.Empty));
			}

            foreach (var filename in mainTempFilenames)
            {
                RenameFromTemp(filename, mainBaseFilename);
            }
            foreach (var filename in subTempFilenames)
            {
                RenameFromTemp(filename, subMainFilename);
            }
		}



		/// <summary>
		/// ファイル名変更 (一時ファイル名)
		/// </summary>
		/// <param name="sourceFilename">変更元ファイル名 (フルパス)</param>
		/// <param name="count">シーケンス番号</param>
		/// <param name="suffix">サフィックス</param>
		/// <returns>リネームされた一時ファイル名</returns>
		private static string RenameToTemp(string sourceFilename, string count, string suffix)
        {
            var path = Path.GetDirectoryName(sourceFilename);
            var ext = Path.GetExtension(sourceFilename);
            var tempFilename = $@"{path}\{TempBaseName}-{count}{suffix}{ext}";

			File.Move(sourceFilename, tempFilename);

            return tempFilename;
        }


		/// <summary>
		/// Rename フェーズ2
		/// </summary>
		/// <param name="tempName">テンポラリ・ファイル名</param>
		/// <param name="toBaseName">テンポラリ・ファイル名を置き換えるベース・ファイル名</param>
		private static void RenameFromTemp(string tempName, string toBaseName)
        {
            var toName = tempName.Replace(TempBaseName, toBaseName);

			File.Move(tempName, toName);
        }
	}
}
