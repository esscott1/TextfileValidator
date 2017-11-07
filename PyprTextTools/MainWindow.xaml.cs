using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using PyprTextToolLib;

namespace PyprTextTools
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	
	public partial class MainWindow : Window
	{
		private System.Diagnostics.Stopwatch stopwatch;
		List<string> filename;
		CancellationTokenSource m_cancelTokenSource = null;
		private bool m_running = false;
		public MainWindow()
		{
			InitializeComponent();
		}
		private void SetProgress(int value)
		{
			progressBar1.Value = value + 1;
			subProgreeBar1.Value = 0;
			
		}
		private void SetSubProgress(int value)
		{
			subProgreeBar1.Value = value + 1;
		}
		private async void btnProcess_click(object sender, RoutedEventArgs e)
		{
			//System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			//timer.Start();
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
			timer.Interval = 1000;
			timer.Start(); 
			stopwatch = new System.Diagnostics.Stopwatch();
			timer.Tick+=timer_Tick;
			stopwatch.Start(); 
			timer.Start();
			if (!m_running)
			{
				m_running = true;
				lblStatus.Content = "Processing...";
				btnProcess.Content = "Cancel";
				List<string> items = new List<string>();
				foreach (string item in lbFilesToConsider_Copy.Items)
					items.Add(item);
				progressBar1.Maximum = items.Count + 1;
				subProgreeBar1.Maximum = 100;
				SeparateData separateData = new SeparateData(tbTargetLocation_Copy.Text, items.ToArray(), cbUseParallelForEach.IsChecked);
				m_running = true;
				m_cancelTokenSource = new CancellationTokenSource();
				Progress<int> prog = new Progress<int>(SetProgress);
				Progress<int> subProg = new Progress<int>(SetSubProgress);
				try
				{
					lblStatus.Content = "Processing...";
					Task ts = separateData.Process(prog, subProg, m_cancelTokenSource.Token);
					await ts;
					lblStatus.Content = "Done";

				}
				catch (OperationCanceledException)
				{
					lblStatus.Content = "Canceled";
				}
				finally
				{
					btnProcess.Content = "Process"; 
					m_running = false;
					m_cancelTokenSource = null;
					timer.Stop();
					stopwatch.Stop();
				}
			}
			else
			{
				lblStatus.Content = "Waiting to Cancel...";
				m_cancelTokenSource.Cancel();
				timer.Stop();
				stopwatch.Stop();

			}
			
		}

		void timer_Tick(object sender, EventArgs e)
		{
			TimeSpan ts = stopwatch.Elapsed;
			string timer = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
			lblTimer.Content = timer;
			//throw new NotImplementedException();
		}
		private void btnOpenFile_click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (openFileDialog.ShowDialog() == true)
				foreach (string filename in openFileDialog.FileNames) 
					lbFilesToConsider.Items.Add(filename);

		}

		private void btnOpenTargetLocationDialog_click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
			folderBrowserDialog1.Description = "select where to save your files";
			folderBrowserDialog1.ShowDialog();
				tbTargetLocation.Text = folderBrowserDialog1.SelectedPath;

		}

		private void btnOpenFile2_click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (openFileDialog.ShowDialog() == true)
				foreach (string filename in openFileDialog.FileNames)
					lbFilesToConsider_Copy.Items.Add(filename);

		}
		private void btnOpenTargetLocationDialog2_click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
			folderBrowserDialog1.Description = "select where to save your files";
			folderBrowserDialog1.ShowDialog();
			tbTargetLocation_Copy.Text = folderBrowserDialog1.SelectedPath;

		}
	}
}
