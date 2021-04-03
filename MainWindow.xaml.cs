﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace FilePairing
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainViewModel ViewModel
		{
			get;
			set;
		}






		public MainWindow()
		{
			InitializeComponent();

			ViewModel = new MainViewModel();
		}


		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			var win = new FolderSelectWindow()
			{
				Owner = this
			};
			if (win.ShowDialog() != true)
			{
				Close();
				return;
			}

			ViewModel.MainViewFiles = new ObservableCollection<PairData>(GetImageFiles(win.ViewModel.MainFolderName).Select(i => new PairData(){ MainFile = i }));
			ViewModel.SubViewFiles = new ObservableCollection<string>(GetImageFiles(win.ViewModel.SubFolderName));
			DataContext = ViewModel;
		}


		private static IEnumerable<string> GetImageFiles(string path)
		{
			return Directory.GetFiles(path).OrderBy(i => i).Where(i =>
				i.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase) ||
				i.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)).ToArray();
		}



		private void MainWindow_OnContentRendered(object sender, EventArgs e)
		{
			// IsEnabled = true;
		}


		private ListView _sourceListView;





		private void MainListView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListView sourceListView)) return;

			// if image, check which image was selected -> Main or Sub


			if (e.OriginalSource is Image image)
			{
				if (!(image.DataContext is PairData pd)) return;

				var imageLocalPath = new Uri(image.Source.ToString()).LocalPath;

				if (pd.MainFile == imageLocalPath)
				{
					// drag row
				}
				else
				{
					// drag subimage
				}



				DragDrop.DoDragDrop(sourceListView, image, DragDropEffects.Move);
			}
			else if (e.OriginalSource is ListViewItem li)
			{

				//
			}
			else
			{
				return;
			}

			_sourceListView = sourceListView;
		}



		private void SubListView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListView sourceListView)) return;
			if (!(e.OriginalSource is Image image)) return;

			_sourceListView = sourceListView;

			DragDrop.DoDragDrop(sourceListView, image, DragDropEffects.Move);
		}





		private void MainListView_OnDrop(object sender, DragEventArgs e)
		{
			if (!(sender is ListView lv)) return;
			if (!(lv.ItemsSource is IList sourceList)) return;

			if (!(e.Data.GetData(typeof(Image)) is Image im)) return;
			

			if (!(e.OriginalSource is FrameworkElement targetItem)) return;
			if (!(lv.ContainerFromElement(targetItem) is ListViewItem li)) return;

			if (!(li.Content is PairData currentLine)) return;
			
			if (!(im.DataContext is string filename)) return;

			if (_sourceListView.ItemsSource is ObservableCollection<PairData> originPartDataCollection)
			{
				var currentData = originPartDataCollection.FirstOrDefault(i => i.SubFile == filename);
				if (currentData != null)
				{
					currentData.SubFile = string.Empty;
				}
			}
			else if (_sourceListView.ItemsSource is ObservableCollection<string> originFilenameCollection)
			{
				if (!string.IsNullOrEmpty(currentLine.SubFile))
				{
					originFilenameCollection.Add(currentLine.SubFile);
				}
				originFilenameCollection.Remove(filename);
			}

			currentLine.SubFile = filename;
			lv.SelectedItem = currentLine;

			_sourceListView = null;
		}















		private void MainListView_OnPreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, true) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}

		


		private void SubItem_OnDrop(object sender, DragEventArgs e)
		{
			if (!(sender is ListView lv)) return;
			if (!(lv.ItemsSource is IList sourceList)) return;

			if (!(e.OriginalSource is FrameworkElement targetItem)) return;
			if (!(lv.ContainerFromElement(targetItem) is ListViewItem li)) return;

			if (!(li.Content is PairData currentLine)) return;
			if (!(e.Data.GetData(typeof(Image)) is Image im)) return;
			if (!(im.DataContext is string filename)) return;

			//currentLine.SubFiles.Add(filename);

			//var source = e.Source;
		}













		private void SubListView_OnDrop(object sender, DragEventArgs e)
		{
			// dropped

		}




	}


	internal class ListViewDragDropBehavior : Behavior<ListView>
	{

		const string SelectedItemCollection = "System.Windows.Controls.SelectedItemCollection";

		


		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
			AssociatedObject.DragOver += ListView_DragOver;
			AssociatedObject.Drop += ListView_Drop;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.PreviewMouseLeftButtonDown -= ListView_PreviewMouseLeftButtonDown;
			AssociatedObject.DragOver -= ListView_DragOver;
			AssociatedObject.Drop -= ListView_Drop;
		}


		private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
		}



/*		private void List_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isMouseDown && e.LeftButton == MouseButtonState.Pressed)
			{
				var mousePos = e.GetPosition(null);

			}

			// Get the current mouse position
			
			var diff = _startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
			    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
			    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				// Get the dragged ListViewItem
				var listView = sender as ListView;
				var listViewItem =
					FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

				// Find the data behind the ListViewItem
				var contact = (string)listView.ItemContainerGenerator.
					ItemFromContainer(listViewItem);

				// Initialize the drag & drop operation
				DataObject dragData = new DataObject("myFormat", contact);
				DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
			}
		}
*/

		private void ListView_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(SelectedItemCollection))
			{
				e.Effects = DragDropEffects.Move;
			}
		}

		private void ListView_Drop(object sender, DragEventArgs e)
		{
			if (sender == e.OriginalSource) return;

			if (!(sender is ListView lv)) return;
			if (!(e.Data.GetDataPresent(typeof(Image)))) return;
			if (!(e.Data.GetData(typeof(Image)) is Image droppedImage)) return;





			//lv.ItemsSource
			

		}

		/// <summary>
		/// 項目を移動します。
		/// </summary>
		private void MoveItem(ListView lb, ListViewItem item)
		{
			if (lb == null) return;
			if (lb == ((ListView)item.Parent)) return;

			((ListView)item.Parent).Items.Remove(item);
			if (lb.SelectedItem == null)
			{
				lb.Items.Add(item);
			}
			else
			{
				lb.Items.Insert(lb.SelectedIndex, item);
			}
		}
	}




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
