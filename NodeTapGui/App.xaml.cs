using NodeTapGui.WinFormComponent;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Navigation;

namespace NodeTapGui
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region properties

        /// <summary>
        ///     状态栏图标对象
        /// </summary>
        private NotifyIconComponent _notifyIcon;

        #endregion

        #region override method

        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查是否为管理员权限运行
            CheckAdministrator();

            // 构造主窗口并显示
            MainWindow = new MainView();
            MainWindow.Show();

            // 配置NotifyIcon
            _notifyIcon = new NotifyIconComponent();
            MainWindow.StateChanged +=
                (s, args) =>
                {
                    var win = s as MainView;
                    if (win.WindowState == WindowState.Minimized)
                    {
                        win.ShowInTaskbar = false;
                        _notifyIcon.ShowBalloonTip("NodeTapGui has been minimized!");
                        // 停止Ping
                        Common.CommonEx.TimerGetHostDelays.Stop();
                    }
                };
            _notifyIcon.OnShowWindowHandler +=
                (s, args) =>
                {
                    // 还原显示窗口
                    if (MainWindow.WindowState == WindowState.Minimized)
                    {
                        MainWindow.WindowState = WindowState.Normal;
                        MainWindow.ShowInTaskbar = true;
                        // 开启Ping
                        Common.CommonEx.TimerGetHostDelays.Start();
                    }
                };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 释放资源
            _notifyIcon.Dispose();
        }

        #endregion

        #region method

        /// <summary>  
        ///     检查是否是管理员身份  
        /// </summary>  
        private void CheckAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,  
                // so instead we launch the app as administrator in a new process.  
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator  
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process  
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Shut down the current process  
                Environment.Exit(0);
            }

        }

        #endregion
    }
}
