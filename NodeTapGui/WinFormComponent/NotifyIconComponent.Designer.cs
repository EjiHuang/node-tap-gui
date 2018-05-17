namespace NodeTapGui.WinFormComponent
{
    partial class NotifyIconComponent
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyIconComponent));
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_ShowWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_CloseApp = new System.Windows.Forms.ToolStripMenuItem();
            this.NotifyIconContextMenuStrip.SuspendLayout();
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.NotifyIconContextMenuStrip;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "NodeTapGui";
            this.NotifyIcon.Visible = true;
            // 
            // NotifyIconContextMenuStrip
            // 
            this.NotifyIconContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.NotifyIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_ShowWindow,
            this.MenuItem_CloseApp});
            this.NotifyIconContextMenuStrip.Name = "NotifyIconContextMenuStrip";
            this.NotifyIconContextMenuStrip.Size = new System.Drawing.Size(159, 60);
            // 
            // MenuItem_ShowWindow
            // 
            this.MenuItem_ShowWindow.Name = "MenuItem_ShowWindow";
            this.MenuItem_ShowWindow.Size = new System.Drawing.Size(158, 28);
            this.MenuItem_ShowWindow.Text = "Show me";
            // 
            // MenuItem_CloseApp
            // 
            this.MenuItem_CloseApp.Name = "MenuItem_CloseApp";
            this.MenuItem_CloseApp.Size = new System.Drawing.Size(158, 28);
            this.MenuItem_CloseApp.Text = "Close me";
            this.NotifyIconContextMenuStrip.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip NotifyIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_ShowWindow;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_CloseApp;
    }
}
