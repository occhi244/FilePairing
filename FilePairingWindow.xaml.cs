using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
		public MainViewModel ViewModel
		{
			get;
			set;
		}


		public string MainPath
		{
			get;
			set;
		}

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
		/// コンストラクタ
		/// </summary>
		public FilePairingWindow()
		{
			InitializeComponent();

			ViewModel = new MainViewModel();
		}


		/// <summary>
		/// ウィンドウのロード後に、フォルダ選択ダイアログを表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			// show progress dialog

			ViewModel.MatchingViewFiles = new ObservableCollection<PairData>(GetImageFiles(MainPath).Select(i => new PairData(){ MainFile = i }));
			ViewModel.SubViewFiles = new ObservableCollection<string>(GetImageFiles(SubPath));

			DataContext = ViewModel;
		}


		/// <summary>
		/// 画像ファイルリストを得る
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static IEnumerable<string> GetImageFiles(string path)
		{
			return Directory.GetFiles(path).Where(i =>
				i.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
				.OrderBy(i => i).ToArray();
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
		/// メインリスト マウスボタン処理
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
		/// メインリスト ドロップ処理
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
				else
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
			else if (e.Data.GetDataPresent(DataFormats.Text)) // サブファイルリストからの移動
			{
				var droppedFilename = e.Data.GetData(DataFormats.Text) as string;

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
				//pairDataDroppedAt.SubFile = droppedFilename;
			}

			lv.SelectedItem = pairDataDroppedAt;
		}


		/// <summary>
		/// シフトキーが押されているか
		/// </summary>
        public bool IsShiftKeyPressed =>
            (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
            (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down;


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

            sourceList.Add(pairData.SubFile);
            pairData.SubFile = string.Empty;

			if (IsShiftKeyPressed)
            {
				foreach (var suffixFile in pairData.SubFileSet.ClearSuffixFiles())
                {
					sourceList.Add(suffixFile);
				}
            }
            else if (pairData.SubFileSet.SuffixFileCount > 0)
            {
                pairData.SubFile = pairData.SubFileSet.PopSuffixFile();
            }
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

        private void MainListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
    }



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
			if (!(value is double width)) throw new ArgumentException("not double");

			return $"{width+10}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}




	/// <summary>
	/// メインリスト 幅コンバーター
	/// </summary>
	public class MainListViewWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(value is double width) ? throw new ArgumentException("not double") : $"{((width + 10) * 2) + 28}";
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


		public ObservableCollection<string> SubViewFiles
		{
			get;
			set;
		}
	}





    public class ImageFileSet
    {
        public Action RaiseSuffixFileChanged;


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



        public void AddSuffixFile(string suffixFilename)
        {
            if (_suffixFiles == null)
            {
                _suffixFiles = new List<string>();
            }
            _suffixFiles.Add(suffixFilename);

            RaiseSuffixFileChanged();
		}


        public string PopSuffixFile()
        {
            var subFile = _suffixFiles[0];

            _suffixFiles.Remove(subFile);
            if (_suffixFiles.Count == 0)
            {
                _suffixFiles = null;
            }

            RaiseSuffixFileChanged();

			return subFile;
        }


        public string[] ClearSuffixFiles()
        {
            var result = _suffixFiles?.ToArray() ?? Array.Empty<string>();

            _suffixFiles = null;

            RaiseSuffixFileChanged();

            return result;
        }


        public int SuffixFileCount => _suffixFiles?.Count ?? 0;


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



        public ImageFileSet MainFileSet
        {
            get;
            set;
        }


        public string MainFile
        {
            get => MainFileSet.PrimaryFile;
            set
            {
                MainFileSet.PrimaryFile = value;
				RaisePropertyChanged();
            }
        }


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




		public ImageFileSet SubFileSet
        {
            get;
            set;
        }


        public string SubFile
        {
            get => SubFileSet.PrimaryFile;
            set
            {
                SubFileSet.PrimaryFile = value;
				RaisePropertyChanged();
			}
        }



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



        public void Merge(PairData anotherData)
        {
			MainFileSet.Merge(anotherData.MainFileSet);
			RaiseMainFileSuffixFileChange();

			SubFileSet.Merge(anotherData.SubFileSet);
			RaiseSubFileSuffixFileChanged();
        }




		/*
		private string _mainFile;
		public string MainFile
		{
			get => _mainFile;
			set
			{
				_mainFile = value;
				RaisePropertyChanged();
			}
		}

		


		/// <summary>
		/// サブファイルリスト
		/// </summary>
		private string _subFile;
		public string SubFile
		{
			get => _subFile;
			set
			{
				_subFile = value;
				RaisePropertyChanged();
			}
		}



        public void AddSubFileSuffix(string suffixFilename)
        {
            if (_subFileSuffixFiles == null)
            {
                _subFileSuffixFiles = new List<string>();
            }
			_subFileSuffixFiles.Add(suffixFilename);

			RaisePropertyChanged(nameof(SuffixFileCount));
			RaisePropertyChanged(nameof(SuffixIconVisibility));
			RaisePropertyChanged(nameof(HasSuffix));
        }


        public string ShiftSubFileSuffix()
        {
            var subFile = _subFileSuffixFiles[0];

            _subFileSuffixFiles.Remove(subFile);
            if (_subFileSuffixFiles.Count == 0)
            {
                _subFileSuffixFiles = null;
            }

            RaisePropertyChanged(nameof(SuffixFileCount));
			RaisePropertyChanged(nameof(SuffixFileCollection));
			RaisePropertyChanged(nameof(SuffixIconVisibility));
			RaisePropertyChanged(nameof(HasSuffix));

            return subFile;
        }



        public string[] ClearSubFileSuffix()
        {
            var result = _subFileSuffixFiles?.ToArray() ?? Array.Empty<string>();

            _subFileSuffixFiles = null;

            RaisePropertyChanged(nameof(SuffixFileCount));
			RaisePropertyChanged(nameof(SuffixIconVisibility));
            RaisePropertyChanged(nameof(HasSuffix));

            return result;
        }


		/// <summary>
		/// サブファイルのサフィックスファイルリスト
		/// </summary>
        private List<string> _subFileSuffixFiles;
        public ReadOnlyCollection<string> SuffixFileCollection => new ReadOnlyCollection<string>(_subFileSuffixFiles);


		/// <summary>
		/// サブファイルのサフィックスファイル数
		/// </summary>
        public int SuffixFileCount => _subFileSuffixFiles?.Count ?? 0;


        /// <summary>
        /// サフィックス・アイコンを表示するかファイルがあるか
        /// </summary>
		public Visibility SuffixIconVisibility => HasSuffix ? Visibility.Visible : Visibility.Hidden;


		/// <summary>
		/// サブファイルにサフィックスがあるか
		/// </summary>
		public bool HasSuffix => _subFileSuffixFiles != null && _subFileSuffixFiles.Count > 0;
    */
    }
}
