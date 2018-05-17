using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeTapGui.WinFormComponent
{
    public partial class NotifyIconComponent: Component
    {
        #region properties

        /// <summary>
        ///     用于通知View显示窗口
        /// </summary>
        public EventHandler OnShowWindowHandler;

        #endregion

        #region constructor

        public NotifyIconComponent()
        {
            InitializeComponent();
            // 基本配置
            NotifyIcon.Visible = true;
            MenuItem_ShowWindow.Click += 
                (s, e) => OnShowWindowHandler?.Invoke(null, null);
            MenuItem_CloseApp.Click += 
                (s, e) => Application.Current.Shutdown();
            NotifyIcon.DoubleClick +=
                (s, e) => OnShowWindowHandler?.Invoke(null, null);
        }

        public NotifyIconComponent(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region method

        /// <summary>
        ///     显示气泡提示
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        public void ShowBalloonTip(string msg, string title = "Tip")
            => NotifyIcon.ShowBalloonTip(3000, msg, title, System.Windows.Forms.ToolTipIcon.Info);

        #endregion
    }
}
