using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneClickKafka
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ReadConfig();
		}

		private void ReadConfig()
		{
			zkserver_Cmd.Text = ConfigurationManager.AppSettings["zkserver_Cmd"];
			kafkaserver_folder.Text = ConfigurationManager.AppSettings["kafkaserver_folder"];
			startkafkaserver_cmd.Text = ConfigurationManager.AppSettings["startkafkaserver_cmd"];
			topic_Folder.Text = ConfigurationManager.AppSettings["topic_Folder"];
			topicList_cmd.Text = ConfigurationManager.AppSettings["topicList_cmd"];
			topicCreate_cmd.Text = ConfigurationManager.AppSettings["topicCreate_cmd"];
			topicDelete_cmd.Text = ConfigurationManager.AppSettings["topicDelete_cmd"];
		}

		private void SaveConfig()
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			config.AppSettings.Settings["zkserver_Cmd"].Value = zkserver_Cmd.Text;
			config.AppSettings.Settings["kafkaserver_folder"].Value = kafkaserver_folder.Text;
			config.AppSettings.Settings["startkafkaserver_cmd"].Value = startkafkaserver_cmd.Text;
			config.AppSettings.Settings["topic_Folder"].Value = topic_Folder.Text;
			config.AppSettings.Settings["topicList_cmd"].Value = topicList_cmd.Text;
			config.AppSettings.Settings["topicCreate_cmd"].Value = topicCreate_cmd.Text;
			config.AppSettings.Settings["topicDelete_cmd"].Value = topicDelete_cmd.Text;

			config.Save(ConfigurationSaveMode.Modified);
		}

		private void ClearLogs_Click(object sender, RoutedEventArgs e)
		{
			string kafkaServerDir = kafkaserver_folder.Text.Trim('>');
			// If directory does not exist, don't even try   
			if (Directory.Exists(kafkaServerDir))
			{
				string logsDirMsg= "Cleared the logs in folders as below:\r\n";
				var logDirs = Directory.GetDirectories(kafkaServerDir).Where(dir => dir.EndsWith("logs"));
				if (logDirs != null)
				{
					foreach (string logDir in logDirs)
					{
						Array.ForEach(Directory.GetFiles(logDir), File.Delete);
						Array.ForEach(Directory.GetDirectories(logDir), DeleteDir);
						logsDirMsg += logDir + "\r\n";
					}
				}
				//MsgBar.Visibility = Visibility.Visible;
				//MsgBar.Text = logsDirMsg;
				//MsgBar.Foreground = Brushes.DarkGreen;
				SetMsgBar(logsDirMsg);
			}
			else 
			{
				//MsgBar.Visibility = Visibility.Visible;
				//MsgBar.Text = $"ERROR: The input kafka server directory '{kafkaServerDir}' does not exist!";
				//MsgBar.Foreground = Brushes.Crimson;
				SetMsgBar($"ERROR: The input kafka server directory '{kafkaServerDir}' does not exist!");
			}
			//else showing dir error in msg bar.
		}

		private void DeleteDir(string input)
		{
			DirectoryInfo subdir = new DirectoryInfo(input);
			subdir.Delete(true);
		}

		private void StartZooKeeper_Click(object sender, RoutedEventArgs e)
		{
			RunCmd(zkserver_Cmd.Text);
			MsgBar.Visibility = Visibility.Hidden;
		}

		private void StartKafka_Click(object sender, RoutedEventArgs e)
		{
			RunCmd(startkafkaserver_cmd.Text, kafkaserver_folder.Text);
			MsgBar.Visibility = Visibility.Hidden;
		}

		private void ListAllTopics_Click(object sender, RoutedEventArgs e)
		{
			RunCmd_ListAllTopics();
			//MsgBar.Text = "Listed all the topics as below:";
			//MsgBar.Visibility = Visibility.Visible;
			//MsgBar.Foreground = Brushes.DarkGreen;
			SetMsgBar("Listed all the topics as below:");
			currentTopicsGrid.Visibility = Visibility.Visible;
		}

		private void CreateTopic_Click(object sender, RoutedEventArgs e)
		{
			string cmd = topicCreate_cmd.Text.Replace("xxx", topicName.Text);
			MsgBar.Visibility = Visibility.Visible;
			MsgBar.Text = "Processing...";
			//this.Dispatcher.BeginInvoke(new Action(() => {
			//    MsgBar.Text = "Processing...";
			//}));
			string r = RunCmd_R(cmd, topic_Folder.Text);
			//MsgBar.Text = r;
			//MsgBar.Visibility = Visibility.Visible;
			//MsgBar.Foreground = Brushes.DarkGreen;
			//if (r.ToLower().Contains("error"))
			//{
			//	MsgBar.Foreground = Brushes.Crimson;
			//};
			SetMsgBar(r);
			RunCmd_ListAllTopics();
		}

		private void DeleteTopic_Click(object sender, RoutedEventArgs e)
		{
			string cmd = topicDelete_cmd.Text.Replace("xxx", topicName.Text);
			string r = RunCmd_R(cmd, topic_Folder.Text);
			//this.Dispatcher.BeginInvoke(new Action(() =>
			//{
			//    MsgBar.Text = r;
			//}));
			//MsgBar.Text = r;
			//MsgBar.Visibility = Visibility.Visible;
			//MsgBar.Foreground = Brushes.DarkGreen;
			//if (r.ToLower().Contains("error")) 
			//{
			//	MsgBar.Foreground = Brushes.Crimson;
			//};
			SetMsgBar(r);
			RunCmd_ListAllTopics();
		}

		private void RunCmd(string cmd, string dir = "")
		{
			Task tsk = new Task(() =>
			{
				Process p = new Process();
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/k " + cmd;
				p.StartInfo.UseShellExecute = false;


				if (dir != "")
				{
					p.StartInfo.WorkingDirectory = dir.Trim('>');
				}

				try
				{
					p.Start();
				}
				catch (Exception e)
				{
					string s = e.Message;
				}
			});
			tsk.Start();
			tsk.Wait();
		}

		private string RunCmd_R(string cmd, string dir = "")
		{
			string output = string.Empty;
			Task tsk = new Task(() =>
			{
				//this.Dispatcher.Invoke(new Action(() =>
				//{
				//    MsgBar.Visibility = Visibility.Visible;
				//    MsgBar.Text = "Processing...";
				//}));

				Process p = new Process();
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/c " + cmd;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.CreateNoWindow = true;

				if (dir != "")
				{
					p.StartInfo.WorkingDirectory = dir.Trim('>');
				}

				p.Start();
				// Synchronously read the standard output of the spawned process. 
				StreamReader reader = p.StandardOutput;
				output = reader.ReadToEnd();

				// Write the redirected output.
				var r1 = output.Split(new[] { "\r\n" }, StringSplitOptions.None).Where(t => t != "");
				p.WaitForExit();
			});
			tsk.Start();
			tsk.Wait();

			return output;
		}

		private void RunCmd_ListAllTopics()
		{
			string output = RunCmd_R(topicList_cmd.Text, topic_Folder.Text);
			var topics = output.Split(new[] { "\r\n" }, StringSplitOptions.None).Where(t => t != "");
			currentTopics.Items.Clear();
			foreach (string t in topics)
			{
				currentTopics.Items.Add(t);
			}
			currentTopics.SelectionChanged += ListBox_SelectionChanged;
		}

		private void ShowConfig_Click(object sender, RoutedEventArgs e)
		{
			ConfigPanel.Visibility = (ConfigPanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
			AppWindow.Width = (ConfigPanel.Visibility == Visibility.Visible) ? AppWindow.Width + 400 : AppWindow.Width - 400;
			SaveConfig();
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				topicName.Text = (sender as ListBox).SelectedItem.ToString();
			}
			catch (Exception)
			{
				topicName.Text = "";
			}
		}

		private void SaveConfigEvent(object sender, TextChangedEventArgs e)
		{
			SaveConfig();
		}

		private void SetMsgBar(string msg) 
		{
			MsgBar.Text = msg;
			MsgBar.Visibility = Visibility.Visible;
			MsgBar.Foreground = Brushes.DarkGreen;
			if (msg.ToLower().Contains("error"))
			{
				MsgBar.Foreground = Brushes.Crimson;
			};
		}
	}
}
