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

			RenameFiles(filenameWindow.ViewModel.MainFilename, filenameWindow.ViewModel.SubFilename, pairingWindow.ViewModel.MatchingViewFiles);
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

                RenameToTemp(mainTempFilenames, pairData.MainFileSet, fileSequence);
                RenameToTemp(subTempFilenames, pairData.SubFileSet, fileSequence);
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
		/// <param name="renamedFilenameList">変更されたファイル名のリスト</param>
		/// <param name="sourceFileSet">変更するファイルセット</param>
		/// <param name="count">シーケンスカウント</param>
		/// <returns>renamedFilenameList</returns>
        public static List<string> RenameToTemp(List<string> renamedFilenameList, ImageFileSet sourceFileSet, string count)
        {
            var path = Path.GetDirectoryName(sourceFileSet.PrimaryFile);
            var ext = Path.GetExtension(sourceFileSet.PrimaryFile);
            var tempFilename = $@"{path}\{TempBaseName}-{count}{ext}";

            if (string.IsNullOrEmpty(sourceFileSet.PrimaryFile)) return renamedFilenameList;

			File.Move(sourceFileSet.PrimaryFile, tempFilename);
			renamedFilenameList.Add(tempFilename);

            var suffix = 0;
            foreach (var suffixFile in sourceFileSet.SuffixFileCollection)
            {
				tempFilename = $@"{path}\{TempBaseName}-{count}_{++suffix}{ext}";

                File.Move(suffixFile, tempFilename);

				renamedFilenameList.Add(tempFilename);
            }

			return renamedFilenameList;
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
