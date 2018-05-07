using NodeTapGui;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Command
{
	public class CmdHelper
    {
		public MainWindow Main;
		Process proc;

		public CmdHelper(MainWindow main)
		{
			Main = main;
		}

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
				 proc.Start();
				 proc.BeginOutputReadLine();
				 proc.OutputDataReceived += Proc_OutputDataReceived;
			 });
        }

		public void CloseProc()
		{
			if (proc != null && !proc.HasExited)
			{
				proc.Close();
				Process.GetProcesses().Where(pr => pr.ProcessName == "sstap").FirstOrDefault().Kill();
			}
		}

		private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Main.ConsoleText.Value += e.Data + Environment.NewLine;
		}
	}
}
