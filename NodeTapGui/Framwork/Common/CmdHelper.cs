using NodeTapGui;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
	public class CmdHelper
	{
		public MainView Main;
		Process proc;

		public CmdHelper(MainView main)
		{
			Main = main;
		}

		/// <summary>
		///		异步执行命令
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public async Task ExecuteCommandAsync(string command)
		{
			await Task.Run(() =>
			 {
				 ProcessStartInfo CmdProcessInfo = new ProcessStartInfo()
				 {
					 FileName = "cmd",
					 Arguments = "/c " + command,
					 UseShellExecute = false,
					 RedirectStandardOutput = true,
					 CreateNoWindow = true,
					 WindowStyle = ProcessWindowStyle.Hidden
				 };
				 proc = new Process() { StartInfo = CmdProcessInfo };
				 proc.StartInfo.RedirectStandardError = true;
				 proc.Start();
				 proc.BeginOutputReadLine();
				 proc.BeginErrorReadLine();
				 proc.OutputDataReceived += Proc_OutputDataReceived;
				 proc.ErrorDataReceived += Proc_ErrorDataReceived;
			 });
		}
		
		/// <summary>
		///		关闭当前关联程序
		/// </summary>
		public void CloseProc()
		{
			if (proc != null && !proc.HasExited)
			{
				proc.Close();
				Process.GetProcesses().Where(pr => pr.ProcessName == "sstap").FirstOrDefault().Kill();
			}
		}

		/// <summary>
		///		程序执行命令信息输出，程序未执行完毕也可以实现输出
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Main.ConsoleText.Value += e.Data + Environment.NewLine;
		}

		/// <summary>
		///		程序错误信息输出
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (null != e.Data)
			{
				Main.ConsoleText.Value += e.Data + Environment.NewLine;
				Main.ConsoleText.Value += "提醒：如果提示命令不存在，请检查当前程序是否在sstap.exe根目录下。";
			}
		}
	}
}
