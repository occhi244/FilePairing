using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FilePairing
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FilePairingWindow
	{
		/// <summary>
		/// ビューモデル
		/// </summary>
		public MainViewModel ViewModel
		{
			get;
			set;
		}


		/// <summary>
		/// メインファイル・パス
		/// </summary>
		public string MainPath
		{
			get;
			set;
		}


		/// <summary>
		/// サブファイル・パス
		/// </summary>
		public string SubPath
		{
			get;
			set;
		}


		// Drag&Drop control objects.
		private ListView _sourceListView;


		/// <summary>
		/// Drag mode - Row move (MainFile is selected or not)
		/// default is false
		/// </summary>
		private bool _isRowMove;



        /// <summary>
        /// シフトキーが押されているか
        /// </summary>
        public bool IsShiftKeyPressed =>
            (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
            (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down;




		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FilePairingWindow()
		{
			InitializeComponent();
		}


		/// <summary>
		/// ウィンドウのロード後に、フォルダ選択ダイアログを表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewModel = SetupViewModel(MainPath, SubPath);
            DataContext = ViewModel;
		}



		/// <summary>
		/// ビューモデルのセットアップ
		/// </summary>
		/// <param name="mainPath"></param>
		/// <param name="subPath"></param>
		/// <returns></returns>
        private static MainViewModel SetupViewModel(string mainPath, string subPath)
        {
            var mainFiles = GetImageFiles(mainPath);
            var subFiles = GetImageFiles(subPath);
            var matchedFiles = new List<PairData>();

            const string pattern = @"(.+)(-\d+)(_\d+)?.(jpg|jpeg|png)$";
            var regex = new Regex(pattern,RegexOptions.IgnoreCase);

            //var prevMainFileBasename = string.Empty;
            PairData prevPairData = null;

            foreach (var mainFile in mainFiles)
            {
                var mm = regex.Match(mainFile);
                if (!mm.Success)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(mm.Groups[3].ToString()))
                {
                    var newPairData = new PairData
                    {
                        MainFile = mainFile
                    };

                    var mainFileSeq = mm.Groups[2].ToString();

                    var matchedSubFiles = new List<string>();

                    foreach (var subFile in subFiles)
                    {
                        var ms = regex.Match(subFile);

                        if (!ms.Success)
                        {
                            continue;
                        }

                        var subFileSeq = ms.Groups[2].ToString();

                        if (subFileSeq == mainFileSeq)
                        {
                            if (string.IsNullOrEmpty(ms.Groups[3].ToString()))
                            {
                                newPairData.SubFile = subFile;
                            }
                            else
                            {
                                newPairData.SubFileSet.AddSuffixFile(subFile);
                            }
							matchedSubFiles.Add(subFile);
                        }
                        else if (string.Compare(subFileSeq, mainFileSeq, StringComparison.Ordinal) > 0)
                        {
                            break;
                        }
                    }

                    foreach (var matchedSubFile in matchedSubFiles)
                    {
						subFiles.Remove(matchedSubFile);
					}

                    if (prevPairData != null)
                    {
                        matchedFiles.Add(prevPairData);
                    }

                    prevPairData = newPairData;
                }
				else if (mm.Groups.Count > 2)
                {
                    prevPairData?.MainFileSet.AddSuffixFile(mainFile);
                }
            }

            if (prevPairData != null)
            {
				matchedFiles.Add(prevPairData);
            }

            if (matchedFiles.Count == 0)
            {
                matchedFiles.AddRange(mainFiles.Select(mainFile => new PairData { MainFile = mainFile }));
                mainFiles.Clear();
            }
            else
            {
                foreach (var matchedFile in matchedFiles)
                {
                    mainFiles.Remove(matchedFile.MainFile);
                    foreach (var suffixFile in matchedFile.MainFileSet.SuffixFileCollection)
                    {
                        mainFiles.Remove(suffixFile);
                    }
                }
            }

            return new MainViewModel
            {
                MainViewFiles = new ObservableCollection<string>(mainFiles),
                SubViewFiles = new ObservableCollection<string>(subFiles),
				MatchingViewFiles = new ObservableCollection<PairData>(matchedFiles)
            };
        }



		/// <summary>
		/// 画像ファイルリストを得る
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static List<string> GetImageFiles(string path)
		{
			return Directory.GetFiles(path).Where(i =>
				i.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
				.OrderBy(i => i).ToList();
		}


		/// <summary>
		/// 進捗ダイアログを閉じる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_OnContentRendered(object sender, EventArgs e)
		{
			// IsEnabled = true;
		}



		/// <summary>
		/// マッチング・リストビュー マウスボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MatchingListView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListView sourceListView)) return;
			if (!(e.OriginalSource is Image image)) return;
			if (!(image.DataContext is PairData pd)) return;

			_sourceListView = sourceListView;

			var imageLocalPath = ControlAttachedProperty.GetFilename(image);
			if (pd.MainFile == imageLocalPath)
			{
				_isRowMove = true;
			}
			DragDrop.DoDragDrop(sourceListView, pd, DragDropEffects.Move);
			CleanupDragDrop();
		}


		/// <summary>
		/// マッチング・リストビュー ドロップ処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MatchingListView_OnDrop(object sender, DragEventArgs e)
		{
			if (!(sender is ListView lv)) return;

			if (!(e.OriginalSource is FrameworkElement elementDroppedAt)) return;
			if (!(lv.ContainerFromElement(elementDroppedAt) is ListViewItem listeViewItemDroppedAt)) return;
			if (!(listeViewItemDroppedAt.Content is PairData pairDataDroppedAt)) return;

			if (!(lv.ItemsSource is ObservableCollection<PairData> sourceList)) return;
			if (!(SubListView.ItemsSource is ObservableCollection<string> subFileList)) return;
            if (!(MainListView.ItemsSource is ObservableCollection<string> mainFileList)) return;

			if (e.Data.GetDataPresent(typeof(PairData))) // 他の行からの移動
			{
				var droppedItem = e.Data.GetData(typeof(PairData)) as PairData;

				if (_isRowMove)
				{
					var origIndex = sourceList.IndexOf(pairDataDroppedAt);
					sourceList.Remove(droppedItem);
                    var newIndex = sourceList.IndexOf(pairDataDroppedAt) > origIndex ? origIndex - 1 : origIndex;

					if (!IsShiftKeyPressed)
                    {
                        sourceList.Insert(newIndex, droppedItem);
					}
                    else
                    {
                        pairDataDroppedAt.Merge(droppedItem);
                    }
				}
				else if (droppedItem != pairDataDroppedAt)
				{
					if (!string.IsNullOrEmpty(pairDataDroppedAt.SubFile))
					{
						subFileList.Add(pairDataDroppedAt.SubFile);
					}

					if (droppedItem != null)
					{
						pairDataDroppedAt.SubFile = droppedItem.SubFile;
						droppedItem.SubFile = string.Empty;
					}
				}
			}
			else if (e.Data.GetDataPresent(DataFormats.Text)) // メイン・サブファイルリストからの移動
			{
				var droppedFilename = e.Data.GetData(DataFormats.Text) as string;

                if (_sourceListView == SubListView)
                {
                    if (!string.IsNullOrEmpty(pairDataDroppedAt.SubFile))
                    {
                        if (IsShiftKeyPressed)
                        {
                            pairDataDroppedAt.SubFileSet.AddSuffixFile(droppedFilename);
                        }
                        else
                        {
                            subFileList.Add(pairDataDroppedAt.SubFile);
                            pairDataDroppedAt.SubFile = droppedFilename;
                        }
                    }
                    else
                    {
                        pairDataDroppedAt.SubFile = droppedFilename;
                    }

                    subFileList.Remove(droppedFilename);
				}
                else
                {
                    if (!IsShiftKeyPressed)
                    {
                        ViewModel.MatchingViewFiles.Insert(sourceList.IndexOf(pairDataDroppedAt), new PairData
                        {
                            MainFile = droppedFilename
                        });
                    }
                    else
                    {
                        pairDataDroppedAt.MainFileSet.AddSuffixFile(droppedFilename);
                    }

                    mainFileList.Remove(droppedFilename);
                }
			}

			lv.SelectedItem = pairDataDroppedAt;
		}


		/// <summary>
		/// メインファイルリストのマウスボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void MainListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListView sourceListView)) return;
            if (!(e.OriginalSource is Image image)) return;

            _sourceListView = sourceListView;

            DragDrop.DoDragDrop(sourceListView, image.DataContext, DragDropEffects.Move);

            CleanupDragDrop();
        }



        /// <summary>
        /// サブリストのドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainListView_OnDrop(object sender, DragEventArgs e)
        {
            if (_sourceListView == SubListView) return;
			if (!_isRowMove) return;
			if (!(sender is ListView lv)) return;
            if (!(lv.ItemsSource is ObservableCollection<string> sourceList)) return;

            if (!(e.Data.GetData(typeof(PairData)) is PairData pairData)) return;

            if (ViewModel.MatchingViewFiles.Count == 1 && pairData.MainFileSet.SuffixFileCount == 0)
            {
                MessageBox.Show("すべての行を移動することはできません");
                return;
            }

			if (PopImageFile(pairData.MainFileSet, sourceList))
            {
                ViewModel.MatchingViewFiles.Remove(pairData);

                if (!string.IsNullOrEmpty(pairData.SubFile))
                {
                    ViewModel.SubViewFiles.Add(pairData.SubFile);

                    foreach (var suffixFile in pairData.SubFileSet.ClearSuffixFiles())
                    {
                        ViewModel.SubViewFiles.Add(suffixFile);
                    }
                }
            }

			_isRowMove = false;
        }


		/// <summary>
		/// サブリストのマウスボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SubListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListView sourceListView)) return;
			if (!(e.OriginalSource is Image image)) return;

			_sourceListView = sourceListView;

			DragDrop.DoDragDrop(sourceListView, image.DataContext, DragDropEffects.Move);

			CleanupDragDrop();
		}


		/// <summary>
		/// サブリストのドロップ処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SubListView_OnDrop(object sender, DragEventArgs e)
		{
			if (_sourceListView == SubListView) return;
			if (_isRowMove) return;
			if (!(sender is ListView lv)) return;
			if (!(lv.ItemsSource is ObservableCollection<string> sourceList)) return;

			if (!(e.Data.GetData(typeof(PairData)) is PairData pairData)) return;

            if (PopImageFile(pairData.SubFileSet, sourceList))
            {
                pairData.SubFile = string.Empty;
			}
        }


        private bool PopImageFile(ImageFileSet fileSet, ObservableCollection<string> sourceList)
        {
            if (fileSet.SuffixFileCount > 0)
            {
                if (!IsShiftKeyPressed)
                {
                    sourceList.Add(fileSet.PopSuffixFile());
                    return false;
                }
                foreach (var suffixFile in fileSet.ClearSuffixFiles())
                {
                    sourceList.Add(suffixFile);
                }
            }
            sourceList.Add(fileSet.PrimaryFile);

            return true;
        }



		/// <summary>
		/// ドラッグ・ステータスのクリーンアップ
		/// </summary>
		private void CleanupDragDrop()
		{
			_sourceListView = null;
			_isRowMove = false;

		}


		/// <summary>
		/// 完了ボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CompButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}


		/// <summary>
		/// マス移動 対象ファイル設定
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void ImageControl_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ContentControl cc)) return;
            if (!(cc.DataContext is PairData pairData)) return;

            pairData.IsChecked = !pairData.IsChecked;
        }


		/// <summary>
		/// ファイルをまとめて移動する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void MoveFiles_OnClick(object sender, RoutedEventArgs e)
        {
			// Get target object
            if (!(sender is MenuItem mi)) return;
			if (!(mi.DataContext is PairData pairData)) return;

			// Check placing target
            if (pairData.IsChecked)
            {
                MessageBox.Show("自分自身へ移動することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
            }

			// Get moved items
            var movedItems = ViewModel.MatchingViewFiles.Where(i => i.IsChecked).ToList();
            if (movedItems.Count == 0)
            {
                MessageBox.Show("移動対象が選択されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

			// Remove moved items
			movedItems.ForEach(i => ViewModel.MatchingViewFiles.Remove(i));

			// Get the target position
			var placedIndex = ViewModel.MatchingViewFiles.IndexOf(pairData);
			if (placedIndex < 0) return;

			// Move files
			movedItems.ForEach(i =>
            {
                i.IsChecked = false;
                ViewModel.MatchingViewFiles.Insert(++placedIndex, i);
            });
        }
    }




	/// <summary>
	/// ファイルパスからイメージへのコンバーター
	/// </summary>
	public class ImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string filename)) return string.Empty;
			if (string.IsNullOrEmpty(filename)) return string.Empty;

			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				var decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

				return decoder.Frames[0];
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	
	/// <summary>
	/// メインリスト カラム幅コンバーター
	/// </summary>
	public class ColumnWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is GridLength gridLength)) throw new ArgumentException("not double");

			return $"{(gridLength.Value-10)/2}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// 添付プロパティ
	/// </summary>
	public class ControlAttachedProperty
	{
		public static readonly DependencyProperty RemarkProperty
			= DependencyProperty.RegisterAttached(
				"Filename",
				typeof(string),
				typeof(ControlAttachedProperty),
				new FrameworkPropertyMetadata(string.Empty));

		public static string GetFilename(DependencyObject target)
		{
			return (string)target.GetValue(RemarkProperty);
		}

		public static void SetFilename(DependencyObject target, string value)
		{
			target.SetValue(RemarkProperty, value);
		}
	}


	/// <summary>
	/// データビューモデル
	/// </summary>
	public class MainViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Property change event.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Raise property changed event.
		/// </summary>
		/// <param name="propertyName"></param>
		private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		/// <summary>
		/// マッチング・ファイル コレクション
		/// </summary>
		private ObservableCollection<PairData> _matchingViewFiles;
		public ObservableCollection<PairData> MatchingViewFiles
		{
			get => _matchingViewFiles;
			set
			{
				_matchingViewFiles = value;
				RaisePropertyChanged();
			}
		}
		

		/// <summary>
		/// メインファイルリスト
		/// </summary>
        public ObservableCollection<string> MainViewFiles
        {
            get;
            set;
        }


		/// <summary>
		/// サブファイルリスト
		/// </summary>
		public ObservableCollection<string> SubViewFiles
		{
			get;
			set;
		}
	}

	

	/// <summary>
	/// イメージファイルセット
	/// </summary>
    public class ImageFileSet
    {
		/// <summary>
		/// サフィックス・ファイル変更処理
		/// </summary>
        public Action RaiseSuffixFileChanged;


		/// <summary>
		/// プライマリ・ファイル
		/// </summary>
        public string PrimaryFile
        {
            get;
            set;
        }


        /// <summary>
        /// サフィックスファイルリスト
        /// </summary>
        private List<string> _suffixFiles;
        public ReadOnlyCollection<string> SuffixFileCollection => new ReadOnlyCollection<string>(SuffixFileCount > 0 ? _suffixFiles : new List<string>());



		/// <summary>
		/// ファイルを加える
		/// </summary>
		/// <param name="file"></param>
        public void Append(string file)
        {
            if (string.IsNullOrEmpty(PrimaryFile))
            {
                PrimaryFile = file;
            }
            else
            {
                AddSuffixFile(file);
            }
        }


		/// <summary>
		/// サフィックス・ファイルを追加する
		/// </summary>
		/// <param name="suffixFilename"></param>
        public void AddSuffixFile(string suffixFilename)
        {
            if (_suffixFiles == null)
            {
                _suffixFiles = new List<string>();
            }
            _suffixFiles.Add(suffixFilename);

            RaiseSuffixFileChanged();
		}


		/// <summary>
		/// サフィックス・ファイルを取り出す
		/// </summary>
		/// <returns></returns>
        public string PopSuffixFile()
        {
            var subFile = _suffixFiles[_suffixFiles.Count-1];

            _suffixFiles.Remove(subFile);
            if (_suffixFiles.Count == 0)
            {
                _suffixFiles = null;
            }

            RaiseSuffixFileChanged();

			return subFile;
        }


		/// <summary>
		/// サフィックス・ファイルをクリアする
		/// </summary>
		/// <returns></returns>
        public string[] ClearSuffixFiles()
        {
            var result = _suffixFiles?.ToArray() ?? Array.Empty<string>();

            _suffixFiles = null;

            RaiseSuffixFileChanged();

            return result;
        }


		/// <summary>
		/// サフィックス・ファイル数
		/// </summary>
        public int SuffixFileCount => _suffixFiles?.Count ?? 0;



		/// <summary>
		/// イメージファイルセットをマージする
		/// </summary>
		/// <param name="anotherSet"></param>
        public void Merge(ImageFileSet anotherSet)
        {
            if (!string.IsNullOrEmpty(PrimaryFile))
            {
                if (!string.IsNullOrEmpty(anotherSet.PrimaryFile))
                {
                    AddSuffixFile(anotherSet.PrimaryFile);
                }
            }
            else
            {
                PrimaryFile = anotherSet.PrimaryFile;
            }

            if (anotherSet.SuffixFileCount == 0) return;

            foreach (var suffixFile in anotherSet._suffixFiles)
            {
                AddSuffixFile(suffixFile);
            }
		}
	}



	/// <summary>
	/// メインリスト ペアデータ
	/// </summary>
	public class PairData : INotifyPropertyChanged
	{
		/// <summary>
		/// Property change event.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Raise property changed event.
		/// </summary>
		/// <param name="propertyName"></param>
		private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
        public PairData()
        {
            MainFileSet = new ImageFileSet
            {
				RaiseSuffixFileChanged = RaiseMainFileSuffixFileChange
            };

            SubFileSet = new ImageFileSet
            {
				RaiseSuffixFileChanged = RaiseSubFileSuffixFileChanged
            };
        }


		/// <summary>
		/// イメージファイルセット
		/// </summary>
        public ImageFileSet MainFileSet
        {
            get;
            set;
        }


        /// <summary>
        /// メインファイル名
        /// </summary>
		public string MainFile
        {
            get => MainFileSet.PrimaryFile;
            set
            {
                MainFileSet.PrimaryFile = value;
				RaisePropertyChanged();
            }
        }


		/// <summary>
		/// メインファイルのサフィックス変更を通知する
		/// </summary>
        public void RaiseMainFileSuffixFileChange()
        {
            RaisePropertyChanged(nameof(MainFileSuffixFileCount));
            RaisePropertyChanged(nameof(MainFileSuffixIconVisibility));
		}


		/// <summary>
		/// サブファイルのサフィックスファイル数
		/// </summary>
		public int MainFileSuffixFileCount => MainFileSet.SuffixFileCount;


        /// <summary>
        /// サフィックス・アイコンを表示するかファイルがあるか
        /// </summary>
        public Visibility MainFileSuffixIconVisibility => MainFileSuffixFileCount > 0 ? Visibility.Visible : Visibility.Hidden;



		/// <summary>
		/// マス・移動モード
		/// </summary>
        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
				RaisePropertyChanged(nameof(CheckedIconVisibility));
            }
        }


		/// <summary>
		/// マス移動対象データの時に表示する
		/// </summary>
        public Visibility CheckedIconVisibility => IsChecked ? Visibility.Visible : Visibility.Hidden;



		/// <summary>
		/// サブファイル・セット
		/// </summary>
		public ImageFileSet SubFileSet
        {
            get;
            set;
        }


		/// <summary>
		/// サブファイル名
		/// </summary>
        public string SubFile
        {
            get => SubFileSet.PrimaryFile;
            set
            {
                SubFileSet.PrimaryFile = value;
				RaisePropertyChanged();
			}
        }


		/// <summary>
		/// サブファイルのサフィックス変更を通知する
		/// </summary>
		public void RaiseSubFileSuffixFileChanged()
        {
            RaisePropertyChanged(nameof(SubFileSuffixFileCount));
            RaisePropertyChanged(nameof(SubFileSuffixIconVisibility));
		}


		/// <summary>
		/// サブファイルのサフィックスファイル数
		/// </summary>
		public int SubFileSuffixFileCount => SubFileSet.SuffixFileCount;


        /// <summary>
        /// サフィックス・アイコンを表示するかファイルがあるか
        /// </summary>
        public Visibility SubFileSuffixIconVisibility => SubFileSuffixFileCount > 0 ? Visibility.Visible : Visibility.Hidden;


		/// <summary>
		/// データをマージする
		/// </summary>
		/// <param name="anotherData"></param>
        public void Merge(PairData anotherData)
        {
			MainFileSet.Merge(anotherData.MainFileSet);
			RaiseMainFileSuffixFileChange();

			SubFileSet.Merge(anotherData.SubFileSet);
			RaiseSubFileSuffixFileChanged();
        }
    }
}
