using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Path = System.IO.Path;

namespace FilePairing
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 適用除外ファイルのプリフィックス
        /// </summary>
        public const string ExclFilePrefix = "除外_";



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
		private async void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            // Pairing window
            var pairingWindow = new FilePairingWindow
            {
                Owner = this
            };

            // Load if work file exists
			if (File.Exists(FilePairingWindow.WorkFilename))
            {
				if (MessageBox.Show("作業中のデータがあります。読み込みますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var stream = File.OpenRead(FilePairingWindow.WorkFilename))
                    {
                        pairingWindow.ViewModel = await JsonSerializer.DeserializeAsync<MainViewModel>(stream);
					}
                }
            }

            // Input Before/After directory
            if (pairingWindow.ViewModel == null)
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

                pairingWindow.MainPath = folderWindow.ViewModel.MainFolderName;
                pairingWindow.SubPath = folderWindow.ViewModel.SubFolderName;
            }

            // Run file pairing
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
				MatchingFileCollection = pairingWindow.ViewModel.MatchingViewFiles
			};
			if (filenameWindow.ShowDialog() != true)
			{
				Close();
				return;
			}

            // Run rename
			RenameFiles(filenameWindow.ViewModel.MainFilename, filenameWindow.ViewModel.SubFilename, pairingWindow.ViewModel);

            // Delete working files
            File.Delete(FilePairingWindow.WorkFilename);

			Close();
		}



        /// <summary>
        /// ファイル名変更
        /// </summary>
        /// <param name="mainBaseFilename">メイン・ベースファイル名</param>
        /// <param name="subBaseFilename">サブ・ベースファイル名</param>
        /// <param name="viewModel">ファイルペアリングのデータ</param>
		private static void RenameFiles(string mainBaseFilename, string subBaseFilename, MainViewModel viewModel)
        {
			var pairedTempBasename = $"{DateTime.Now:yyyyMMddHHmmss}.$temp";

            var tempFilenameLists = RenameToTemp(viewModel.MatchingViewFiles, pairedTempBasename);
			var mainTempFilenames = tempFilenameLists[0];
            var subTempFilenames = tempFilenameLists[1];

            var exclTempBasename = $"{DateTime.Now:yyyyMMddHHmmss}.$excl";
            var mainExclFilenames = RenameToTemp(viewModel.MainViewFiles, exclTempBasename);
            var subExclFilenames = RenameToTemp(viewModel.SubViewFiles, exclTempBasename);

			RenameFromTemp(mainTempFilenames, pairedTempBasename, mainBaseFilename);
            RenameFromTemp(subTempFilenames, pairedTempBasename, subBaseFilename);

			RenameFromTemp(mainExclFilenames, exclTempBasename, $"{ExclFilePrefix}{mainBaseFilename}");
            RenameFromTemp(subExclFilenames, exclTempBasename, $"{ExclFilePrefix}{subBaseFilename}");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="pairDataList"></param>
		/// <param name="tempBaseName"></param>
		/// <returns></returns>
        private static List<string>[] RenameToTemp(ICollection<PairData> pairDataList, string tempBaseName)
        {
            var count = 0;
            var digit = pairDataList.Count.ToString().Length;
            var mainTempFilenames = new List<string>(pairDataList.Count*2);
            var subTempFilenames = new List<string>(pairDataList.Count*2);

            foreach (var pairData in pairDataList)
            {
                var fileSequence = $"{++count}".PadLeft(digit, '0');

                mainTempFilenames.AddRange(RenameToTemp(pairData.MainFileSet, fileSequence, tempBaseName));
				subTempFilenames.AddRange(RenameToTemp(pairData.SubFileSet, fileSequence, tempBaseName));
            }

            return new[] { mainTempFilenames, subTempFilenames };
        }


        /// <summary>
        /// ファイル名変更 (一時ファイル名)
        /// </summary>
        /// <param name="sourceFileSet">変更するファイルセット</param>
        /// <param name="count">シーケンスカウント</param>
        /// <param name="tempBaseName">テンポラリファイル・ベース名</param>
        /// <returns>renamedFilenameList</returns>
        public static string[] RenameToTemp(ImageFileSet sourceFileSet, string count, string tempBaseName)
        {
            if (string.IsNullOrEmpty(sourceFileSet.PrimaryFile))
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();

            var path = Path.GetDirectoryName(sourceFileSet.PrimaryFile);
            var ext = Path.GetExtension(sourceFileSet.PrimaryFile);

            result.Add(Rename(sourceFileSet.PrimaryFile, $@"{path}\{tempBaseName}-{count}{ext}"));

            var suffix = 0;
            result.AddRange(sourceFileSet.SuffixFileList.Select(suffixFile => Rename(suffixFile, $@"{path}\{tempBaseName}-{count}_{++suffix}{ext}")));

            return result.ToArray();
        }



		/// <summary>
		/// 一時ファイル名に変更する
		/// </summary>
		/// <param name="filenames"></param>
		/// <param name="tempBaseName"></param>
		/// <returns></returns>
		private static List<string> RenameToTemp(ICollection<string> filenames, string tempBaseName)
        {
            var result = new List<string>(filenames.Count);

			var count = 0;
            var digit = filenames.Count.ToString().Length;

            result.AddRange(
                from filename in filenames
                let path = Path.GetDirectoryName(filename)
                let ext = Path.GetExtension(filename)
                let fileSequence = $"{++count}".PadLeft(digit, '0')
                select Rename(filename, $@"{path}\{tempBaseName}_{count}{ext}"));

            return result;
        }



		/// <summary>
		/// ファイル名を変更する
		/// </summary>
		/// <param name="fromFilename">ソースファイル名</param>
		/// <param name="toFilename">ターゲットファイル名</param>
		/// <returns>ターゲットファイル名</returns>
        private static string Rename(string fromFilename, string toFilename)
        {
            File.Move(fromFilename, toFilename);

            return toFilename;
        }


		/// <summary>
		/// Rename フェーズ2
		/// </summary>
		/// <param name="fromFilenames">リネーム元ファイル名リスト</param>
		/// <param name="tempBasename">リプレース元テキスト</param>
		/// <param name="toBasename">リプレース先テキスト</param>
		private static void RenameFromTemp(List<string> fromFilenames, string tempBasename, string toBasename)
        {
            foreach (var fromFilename in fromFilenames)
            {
                File.Move(fromFilename, fromFilename.Replace(tempBasename, toBasename));
            }
        }
	}
}
