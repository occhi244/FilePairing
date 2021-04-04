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
	public partial class MainWindow : Window
	{
		public MainViewModel ViewModel
		{
			get;
			set;
		}







		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

		//	ViewModel = new MainViewModel();
		}









		private void RenameFiles(string mainFilename, string subFilename)
		{
			var count = 0;
			var digit = ViewModel.MainViewFiles.Count.ToString().Length;

			foreach (var pairData in ViewModel.MainViewFiles)
			{
				if (!string.IsNullOrEmpty(pairData.SubFile))
				{
					var suffix = $"{++count}".PadLeft(digit, '0');

					Rename(pairData.MainFile, mainFilename, suffix);
					Rename(pairData.SubFile, subFilename, suffix);
				}
			}
		}



		private void Rename(string source, string baseName, string count)
		{
			var ext = Path.GetExtension(source);

			File.Move(source, $"{baseName}{count}{ext}");
		}


	}
}
