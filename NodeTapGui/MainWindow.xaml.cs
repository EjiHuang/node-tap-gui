using Command;
using NodeTapGui.Controls;
using NotifyProperty;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using MessageBox = System.Windows.MessageBox;

namespace NodeTapGui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
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
        const string CMD_HOST = @"--host ";
        const string CMD_PORT = @"--port ";
        const string CMD_PASSWD = @"--passwd ";
        const string CMD_METHOD = @"--method ";
        const string CMD_TCPHOST = @"--tcphost ";
        const string CMD_TCPPORT = @"--tcpport ";
        const string CMD_TCPPASSWD = @"--tcppasswd ";
        const string CMD_TCPMETHOD = @"--tcpmethod ";
        const string CMD_UDPHOST = @"--udphost ";
        const string CMD_UDPPORT = @"--udpport ";
        const string CMD_UDPPASSWD = @"--udppasswd ";
        const string CMD_UDPMETHOD = @"--udpmethod ";
        const string CMD_XTUDP = @"--xtudp ";
        const string CMD_DNS = @"--dns ";
        const string CMD_SKIPDNS = @"--skipdns ";

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

        /// <summary>
        ///     网络延迟
        /// </summary>
        public NotifyPropertyEx<string> HostDelays { get; } = string.Empty;

        #endregion

        #region method & event

        #region control event

        /// <summary>
        ///     事件：窗口加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsConnectInternet())
                MessageBox.Show("网络电波貌似无法到达，请检查当前计算机网络是否连通。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            ConsoleView.ScrollToEnd();
            ConsoleText.Value += "ʕ •ᴥ•ʔ Welcome to use node sstap.";

            Cmd = new CmdHelper(this);

            // 启动获取网络延迟定时器
            GetHostDelays();
        }

        /// <summary>
        ///     事件：窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
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
        }

        /// <summary>
        ///     事件：启动按钮按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Start_ClickAsync(object sender, RoutedEventArgs e)
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
            else if (IsSeparateMode && IsUdpProtocol)
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
        }

        /// <summary>
        ///     事件：文本框编辑事件，使文本框一直保持文本最后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConsoleView_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConsoleView.SelectionStart = ConsoleView.Text.Length;
            ConsoleView.ScrollToEnd();
        }

        /// <summary>
        ///     事件：扫描屏幕上的二维码并且绘制矩形标志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_QRCodeImport_Click(object sender, RoutedEventArgs e)
        {
            var data = DecodeQRCodeAndDrawRect();
            if (string.IsNullOrWhiteSpace(data))
                MessageBox.Show("二维码识别错误，请检查当前屏幕下是否存在清晰的二维码？", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                // 屏蔽掉ss://和等号后面的中文字符
                if (data.Substring(0, 5) != "ss://")
                {
                    MessageBox.Show("好像不是ss的二维码吧？请尝试手动输入", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                byte[] buffer = Convert.FromBase64String(data.Substring(5, data.IndexOf('=') + 1 - 5));
                string decodedData = Encoding.UTF8.GetString(buffer);
                var arrayData = decodedData.Split(':');
                // 设置数据
                Method.Value = arrayData[0];
                Password.Value = arrayData[1].Split('@')[0];
                Host.Value = arrayData[1].Split('@')[1];
                Port.Value = arrayData[2];
#if DEBUG
                Console.WriteLine(decodedData);
#endif
            }
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
        ///		只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TB_XtudpTimes_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        #endregion

        #region method

        /// <summary>
        ///     事件集合
        /// </summary>
        private void HandleEvents()
        {
            /// ##############################################################################
            ///     事件：窗口加载完成事件
            /// ##############################################################################
            Loaded += MainWindow_Loaded;

            /// ##############################################################################
            ///     事件：窗口关闭事件
            /// ##############################################################################
            Closed += MainWindow_Closed;

            /// ##############################################################################
            ///     事件：文本框编辑事件，使文本框一直保持文本最后
            /// ##############################################################################
            ConsoleView.TextChanged += ConsoleView_TextChanged;

            /// ##############################################################################
            ///     事件：扫描屏幕上的二维码并且绘制矩形标志
            /// ##############################################################################
            Btn_QRCodeImport.Click += Btn_QRCodeImport_Click;

            /// ##############################################################################
            ///     事件：启动按钮按钮单击事件
            /// ##############################################################################
            Btn_Start.Click += Btn_Start_ClickAsync;
        }

        /// <summary>
        ///     识别二维码并且绘制矩形
        /// </summary>
        /// <returns>二维码字符流</returns>
        private string DecodeQRCodeAndDrawRect()
        {
            foreach (var screen in Screen.AllScreens)
            {
                using (Bitmap bmpScreen = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmpScreen))
                    {
                        g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, bmpScreen.Size, CopyPixelOperation.SourceCopy);
                    }

                    var source = new BitmapLuminanceSource(bmpScreen);
                    var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                    var result = new QRCodeReader().decode(bitmap);

                    if (null != result)
                    {
                        var retX = result.ResultPoints.Select(o => o.X);
                        var retY = result.ResultPoints.Select(o => o.Y);
                        var minX = retX.Min();
                        var minY = retY.Min();
                        var maxX = retX.Max();
                        var maxY = retY.Max();

                        var margin = (maxX - minX) * 0.20f;
                        minX += -margin;
                        maxX += margin;
                        minY += -margin;
                        maxY += margin;

                        // 使用GDI+进行矩形绘制，不够炫酷，弃用
                        //using (var pen = new Pen(ColorTranslator.FromHtml("#F7190B"), 4F))
                        //{
                        //    using (var g = Graphics.FromHdc(GetWindowDC(IntPtr.Zero)))
                        //        g.DrawRectangle(pen, (float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
                        //}

                        // 用于获取系统dpi缩放比例
                        PresentationSource ps = PresentationSource.FromVisual(this);
                        if (ps != null)
                        {
                            // 显示二维码矩形
                            var qrcRect = new QRCodeRect
                                (
                                    // 除于缩放比例以达到局部窗口禁用缩放比例的效果
                                    targetWidth: (maxX - minX) / (float)ps.CompositionTarget.TransformToDevice.M11,
                                    targetHeight: (maxY - minY) / (float)ps.CompositionTarget.TransformToDevice.M22,
                                    targetLeft: minX / (float)ps.CompositionTarget.TransformToDevice.M11,
                                    targetTop: minY / (float)ps.CompositionTarget.TransformToDevice.M22,
                                    fullHeight: screen.Bounds.Height,
                                    fullWidth: screen.Bounds.Width
                                );
                            qrcRect.Top = qrcRect.Left = 0;
                            qrcRect.Width = screen.Bounds.Width;
                            qrcRect.Height = screen.Bounds.Height;
                            qrcRect.Show();
                        }

                        return result.Text;
                    }
                }
            }
            // 到这里说明GG了
            return string.Empty;
        }

        /// <summary>
        ///     获取到Host的网络延迟
        /// </summary>
        public void GetHostDelays()
        {
            var ping = new Ping();
            var data = @"FXFXFXFXFXFXFXFXFXFXFXFXFFXFXFXFXFXFXFXFXFXFXFXFXF";
            ping.PingCompleted += async (sender, args) =>
            {
                HostDelays.Value = (args.Reply == null) ? "time out" : args.Reply.RoundtripTime.ToString() + " ms";
                await Task.Delay(1000);
                ping.SendAsync(Host.Value, data);
            };

            ping.SendAsync(Host.Value, data);
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

        #endregion

        #region native api

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        #endregion

    }
}
