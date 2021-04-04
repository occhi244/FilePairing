using System;
using System.Collections.Generic;
using System.Linq;
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
	/// FilenameInputWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FilenameInputWindow : Window
	{
		/// <summary>
		/// データビューモデル
		/// </summary>
		public FilenameInputViewModel ViewModel
		{
			get;
			set;
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FilenameInputWindow()
		{
			InitializeComponent();


			ViewModel = new FilenameInputViewModel
			{
				MainFilename = "施工前",
				SubFilename = "施工後"
			};
			DataContext = ViewModel;
		}


		/// <summary>
		/// OK ボタン処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}


	/// <summary>
	/// ファイル名データビューモデル
	/// </summary>
	public class FilenameInputViewModel
	{
		public string MainFilename
		{
			get;
			set;
		}

		public string SubFilename
		{
			get;
			set;
		}
	}
}
