using System;
using System.Collections.Generic;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FilePairing
{
	/// <summary>
	/// FolderSelectWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FolderSelectWindow : Window
	{
		public FolderViewModel ViewModel
		{
			get;
			set;
		}



		public FolderSelectWindow()
		{
			InitializeComponent();

			ViewModel = new FolderViewModel();
			DataContext = ViewModel;
		}



		private void OKButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(ViewModel.MainFolderName))
			{
				MessageBox.Show("メインフォルダがセットされていません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (string.IsNullOrEmpty(ViewModel.SubFolderName))
			{
				MessageBox.Show("サブフォルダがセットされていません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (ViewModel.MainFolderName == ViewModel.SubFolderName)
			{
				MessageBox.Show("メインフォルダとサブフォルダが同じです", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			DialogResult = true;
			Close();
		}





		private void MainFolderGroup_OnDrop(object sender, DragEventArgs e)
		{
			if (!GetFolderPath(e, out var folderName)) return;

			ViewModel.MainFolderName = folderName;
		}

		private void SubFolderGroup_OnDrop(object sender, DragEventArgs e)
		{
			if (!GetFolderPath(e, out var folderName)) return;

			ViewModel.SubFolderName = folderName;
		}



		private void FolderGroup_OnPreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = GetFolderPath(e, out _) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}


		private static bool GetFolderPath(DragEventArgs e, out string folderName)
		{
			if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
			{
				folderName = files[0];
				return Directory.Exists(folderName);
			}

			folderName = string.Empty;
			return false;
		}
	}



	public class FolderViewModel : INotifyPropertyChanged
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




		private string _mainFolderName;
		public string MainFolderName
		{
			get => _mainFolderName;
			set
			{
				_mainFolderName = value;
				RaisePropertyChanged();
			}
		}


		private string _subFolderName;
		public string SubFolderName
		{
			get => _subFolderName;
			set
			{
				_subFolderName = value;
				RaisePropertyChanged();
			}
		}
	}
}
