using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MSAPI = Microsoft.WindowsAPICodePack;

namespace FilePairing
{
	/// <summary>
	/// FolderSelectWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FolderSelectWindow : Window
	{
		/// <summary>
		/// フォルダ ビューモデル
		/// </summary>
		public FolderViewModel ViewModel
		{
			get;
			set;
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FolderSelectWindow()
		{
			InitializeComponent();

			ViewModel = new FolderViewModel();
			DataContext = ViewModel;
		}



		/// <summary>
		/// OKボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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



		/// <summary>
		/// フォルダのドロップ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainFolderGroup_OnDrop(object sender, DragEventArgs e)
		{
			if (!GetFolderPath(e, out var folderName)) return;

			ViewModel.MainFolderName = folderName;
		}


		/// <summary>
		/// フォルダのドロップ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SubFolderGroup_OnDrop(object sender, DragEventArgs e)
		{
			if (!GetFolderPath(e, out var folderName)) return;

			ViewModel.SubFolderName = folderName;
		}


		/// <summary>
		/// ドラッグ・オーバー処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FolderGroup_OnPreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = GetFolderPath(e, out _) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}


		/// <summary>
		/// フォルダを得る
		/// </summary>
		/// <param name="e"></param>
		/// <param name="folderName"></param>
		/// <returns></returns>
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


		/// <summary>
		/// フォルダの選択
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FolderSelectButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button senderButton)) return;

			var typeName = senderButton.Name == "MainSelectButton" ? "メイン" : "サブ";

			var dialog = new MSAPI::Dialogs.CommonOpenFileDialog
			{
				IsFolderPicker = true,
				Title = $"{typeName}・フォルダを選択してください",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
			};
			if (dialog.ShowDialog() != MSAPI::Dialogs.CommonFileDialogResult.Ok) return;

			switch (senderButton.Name)
			{
				case "MainSelectButton":
					ViewModel.MainFolderName = dialog.FileName;
					break;
				case "SubSelectButton":
					ViewModel.SubFolderName = dialog.FileName;
					break;
			}
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
