using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FilePairing
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FilePairingWindow : Window
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
		/// </summary>
		private bool _isRowMove = false;




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

			ViewModel.MainViewFiles = new ObservableCollection<PairData>(GetImageFiles(MainPath).Select(i => new PairData(){ MainFile = i }));
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
		private void MainListView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListView sourceListView)) return;
			if (e.OriginalSource is Image image)
			{
				if (!(image.DataContext is PairData pd)) return;

				_sourceListView = sourceListView;

				var imageLocalPath = new Uri(image.Source.ToString()).LocalPath;
				if (pd.MainFile == imageLocalPath)
				{
					_isRowMove = true;
				}
				DragDrop.DoDragDrop(sourceListView, pd, DragDropEffects.Move);
				CleanupDragDrop();
			}
		}


		/// <summary>
		/// メインリスト ドロップ処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainListView_OnDrop(object sender, DragEventArgs e)
		{
			if (!(sender is ListView lv)) return;

			if (!(e.OriginalSource is FrameworkElement targetItem)) return;
			if (!(lv.ContainerFromElement(targetItem) is ListViewItem li)) return;
			if (!(li.Content is PairData droppedLine)) return;

			var subFileList = SubListView.ItemsSource as ObservableCollection<string>;

			if (e.Data.GetDataPresent(typeof(PairData)))
			{
				var droppedItem = e.Data.GetData(typeof(PairData)) as PairData;

				if (_isRowMove)
				{
					if (!(lv.ItemsSource is ObservableCollection<PairData> sourceList)) return;

					var origIndex = sourceList.IndexOf(droppedLine);
					sourceList.Remove(droppedItem);
					var newIndex = sourceList.IndexOf(droppedLine) > origIndex ? origIndex-1 : origIndex;
					sourceList.Insert(newIndex, droppedItem);
				}
				else
				{
					if (!string.IsNullOrEmpty(droppedLine.SubFile))
					{
						subFileList.Add(droppedLine.SubFile);
					}
					droppedLine.SubFile = droppedItem.SubFile;
					droppedItem.SubFile = string.Empty;
				}
			}
			else if (e.Data.GetDataPresent(DataFormats.Text))
			{
				var droppedFilename = e.Data.GetData(DataFormats.Text) as string;

				if (!string.IsNullOrEmpty(droppedLine.SubFile))
				{
					subFileList.Add(droppedLine.SubFile);
				}
				subFileList.Remove(droppedFilename);
				droppedLine.SubFile = droppedFilename;
			}
			lv.SelectedItem = droppedLine;
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

			if (e.Data.GetDataPresent(typeof(PairData)))
			{
				var pairData = e.Data.GetData(typeof(PairData)) as PairData;
				sourceList.Add(pairData.SubFile);
				pairData.SubFile = string.Empty;
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

			var win = new FilenameInputWindow
			{
				Owner = this
			};

			if (win.ShowDialog() != true) return;

			// RenameFiles(win.ViewModel.MainFilename, win.ViewModel.SubFilename);
		}




		private void ClearImages()
		{
			MainListView.ItemsSource = null;
			SubListView.ItemsSource = null;
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
			if (!(value is double width)) throw new ArgumentException("not double");

			return $"{(width+10)*2 + 28}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
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
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		private ObservableCollection<PairData> _mainViewFiles;
		public ObservableCollection<PairData> MainViewFiles
		{
			get => _mainViewFiles;
			set
			{
				_mainViewFiles = value;
				RaisePropertyChanged();
			}
		}


		public ObservableCollection<string> SubViewFiles
		{
			get;
			set;
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
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public string MainFile
		{
			get;
			set;
		}

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
	}
}
