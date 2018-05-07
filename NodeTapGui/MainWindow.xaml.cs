using Command;
using Imagin.Common;
using NotifyProperty;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace NodeTapGui
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : BasicWindow
    {
        #region constructor

        public MainWindow()
        {
            InitializeComponent();
            // 绑定数据上下文
            DataContext = this;
            // 加载事件方法
            HandleEvents();
        }

        #endregion

        #region Constants

        /*
         * 规则：.\sstap.exe --host [ss host] --port [ss port] --passwd [ss password] --xtudp [x times] --method [ss method]
         * 命令：host:       默认 Shadowsocks地址(可选)
         *      port:       默认 Shadowsocks端口(可选)
         *      passwd:     默认 Shadowsocks密码(可选)
         *      method:     默认 Shadowsocks加密方式(可选)
         *      tcphost:    TCP Shadowsocks地址(可选)
         *      tcpport:    TCP Shadowsocks端口(可选)
         *      tcppasswd:  TCP Shadowsocks密码(可选)
         *      tcpmethod:  TCP Shadowsocks加密方式(可选)
         *      udphost:    UDP Shadowsocks地址(可选)
         *      udpport:    UDP Shadowsocks端口(可选)
         *      udppasswd:  UDP Shadowsocks密码(可选)
         *      udpmethod:  UDP Shadowsocks加密方式(可选)
         *      xtudp:      UDP 多倍发包倍率(适用于游戏)
         *      dns:        指定DNS(默认8.8.8.8)
         *      skipdns:    DNS不经过Shadowsocks转发(默认false)
         */
        const string CMD_NODE_TAP_EXE = @".\sstap.exe ";
        const string CMD_HOST         = @"--host ";
        const string CMD_PORT         = @"--port ";
        const string CMD_PASSWD       = @"--passwd ";
        const string CMD_METHOD       = @"--method ";
        const string CMD_TCPHOST      = @"--tcphost ";
        const string CMD_TCPPORT      = @"--tcpport ";
        const string CMD_TCPPASSWD    = @"--tcppasswd ";
        const string CMD_TCPMETHOD    = @"--tcpmethod ";
        const string CMD_UDPHOST      = @"--udphost ";
        const string CMD_UDPPORT      = @"--udpport ";
        const string CMD_UDPPASSWD    = @"--udppasswd ";
        const string CMD_UDPMETHOD    = @"--udpmethod ";
        const string CMD_XTUDP        = @"--xtudp ";
        const string CMD_DNS          = @"--dns ";
        const string CMD_SKIPDNS      = @"--skipdns ";

        #endregion

        #region properties

        /// <summary>
        ///     尾标 
        /// </summary>
        public string CmdTag = Environment.NewLine + @"$  ";

        /// <summary>
        ///     命令字符串
        /// </summary>
        public string CmdString = string.Empty;

        /// <summary>
        ///     控制台帮助类
        /// </summary>
        public CmdHelper Cmd;

		/// <summary>
		///		加密方式
		/// </summary>
		public NotifyPropertyEx<List<string>> MethodList { get; } = new List<string>
		{
			"rc4-md5", "aes-256-cfb", "aes-128-gcm", "aes-192-gcm", "aes-256-gcm"
		};

		/// <summary>
		///     控制台输出文本
		/// </summary>
		public NotifyPropertyEx<string> ConsoleText { get; } = string.Empty;

        /// <summary>
        ///     是否开启UDP多倍发包倍率
        /// </summary>
        public NotifyPropertyEx<bool> IsXtudp { get; } = true;

		/// <summary>
		///     Host
		/// </summary>
		public NotifyPropertyEx<string> Host { get; } = ConfigurationManager.AppSettings["Host"];

		/// <summary>
		///     Port
		/// </summary>
		public NotifyPropertyEx<string> Port { get; } = ConfigurationManager.AppSettings["Port"];

		/// <summary>
		///     Password
		/// </summary>
		public NotifyPropertyEx<string> Password { get; } = ConfigurationManager.AppSettings["Password"];

		/// <summary>
		///     Method
		/// </summary>
		public NotifyPropertyEx<string> Method { get; } = ConfigurationManager.AppSettings["Method"];

		/// <summary>
		///		Xtudp Times
		/// </summary>
		public NotifyPropertyEx<string> XtudpTimes { get; } = "20";

		/// <summary>
		///     Udp Protocol?
		/// </summary>
		public NotifyPropertyEx<bool> IsUdpProtocol { get; } = false;

		/// <summary>
		///		Enable Separate Mode?
		/// </summary>
		public NotifyPropertyEx<bool> IsSeparateMode { get; } = false;

		#endregion

		#region method

		/// <summary>
		///     事件集合
		/// </summary>
		private void HandleEvents()
        {
			/*
				窗口加载完成事件
			 */
			Loaded += (s, e) =>
            {
                if (!IsConnectInternet())
                    MessageBox.Show("网络电波貌似无法到达，请检查当前计算机网络是否连通。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ConsoleView.ScrollToEnd();
                ConsoleText.Value += "ʕ •ᴥ•ʔ Welcome to use node sstap.";

				Cmd = new CmdHelper(this);
			};

			/*
				窗口关闭事件
			 */
			Closed += (s, e) =>
			{
				// 保存信息到程序配置中
				Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				cfa.AppSettings.Settings["Host"].Value = Host.Value;
				cfa.AppSettings.Settings["Port"].Value = Port.Value;
				cfa.AppSettings.Settings["Password"].Value = Password.Value;
				cfa.AppSettings.Settings["Method"].Value = Method.Value;
				cfa.Save();
				// 关闭SSTAP进程
				Cmd.CloseProc();
			};

			/*
				文本框编辑事件，使文本框一直保持文本最后
			 */
			ConsoleView.TextChanged += (s, e) => 
			{
				ConsoleView.SelectionStart = ConsoleView.Text.Length;
				ConsoleView.ScrollToEnd();
			};

			/*
				启动按钮按钮单击事件
			 */
			Btn_Start.Click += async (s, e) =>
            {
				// 构造命令
                CmdString = CMD_NODE_TAP_EXE;
				// 添加Host
				CmdString += string.IsNullOrWhiteSpace(Host.Value) ? "" : string.Format("{0}{1} ", CMD_HOST, Host.Value);
				// 添加端口
				CmdString += string.IsNullOrWhiteSpace(Port.Value) ? "" : string.Format("{0}{1} ", CMD_PORT, Port.Value);
				// 添加密码
				CmdString += string.IsNullOrWhiteSpace(Password.Value) ? "" : string.Format("{0}{1} ", CMD_PASSWD, Password.Value);
				// 判断udp多倍发包倍率(适用于游戏)
				if (IsXtudp)
				{
					// Xtudp Times
					CmdString += string.IsNullOrWhiteSpace(XtudpTimes.Value) ? "" : string.Format("{0}{1} ", CMD_XTUDP, XtudpTimes.Value);
				}
				// 加密
				CmdString += string.IsNullOrWhiteSpace(Method.Value) ? "" : string.Format("{0}{1} ", CMD_METHOD, Method.Value);

				// 判断通信协议
				if (IsSeparateMode && !IsUdpProtocol)
				{
					// tcp host
					CmdString += string.IsNullOrWhiteSpace(Host.Value) ? "" : string.Format("{0}{1} ", CMD_TCPHOST, Host.Value);
					// tcp port
					CmdString += string.IsNullOrWhiteSpace(Port.Value) ? "" : string.Format("{0}{1} ", CMD_TCPPORT, Port.Value);
					// tcp passwd
					CmdString += string.IsNullOrWhiteSpace(Password.Value) ? "" : string.Format("{0}{1} ", CMD_TCPPASSWD, Password.Value);
					// tcp method
					CmdString += string.IsNullOrWhiteSpace(Method.Value) ? "" : string.Format("{0}{1} ", CMD_TCPMETHOD, Method.Value);
				}
				else if(IsSeparateMode && IsUdpProtocol)
				{
					// udp host
					CmdString += string.IsNullOrWhiteSpace(Host.Value) ? "" : string.Format("{0}{1} ", CMD_UDPHOST, Host.Value);
					// udp port
					CmdString += string.IsNullOrWhiteSpace(Port.Value) ? "" : string.Format("{0}{1} ", CMD_UDPPORT, Port.Value);
					// udp passwd
					CmdString += string.IsNullOrWhiteSpace(Password.Value) ? "" : string.Format("{0}{1} ", CMD_UDPPASSWD, Password.Value);
					// udp method
					CmdString += string.IsNullOrWhiteSpace(Method.Value) ? "" : string.Format("{0}{1} ", CMD_UDPMETHOD, Method.Value);
				}


				ConsoleText.Value += CmdTag + CmdString + Environment.NewLine;
                await Cmd.ExecuteCommandAsync(CmdString);

				// ʕ •ᴥ•ʔ保持焦点
				ConsoleView.Focus();
			};
        }

		/// <summary>
		///		密码框光标处理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			PasswordBox pwdBox = (PasswordBox)sender;
			var select = pwdBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
			select.Invoke(pwdBox, new object[] { pwdBox.Password.Length + 1, pwdBox.Password.Length + 1 });
		}

        /// <summary>
        ///     检查当前网络是否连通
        /// </summary>
        /// <returns></returns>
        private static bool IsConnectInternet()
        {
            int Description = 0;
            return InternetGetConnectedState(Description, 0);
        }
		[DllImport("wininet.dll")]
		private extern static bool InternetGetConnectedState(int Description, int ReservedValue);


		#endregion
		/// <summary>
		///		只能输入数字
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TB_XtudpTimes_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}
	}
}
