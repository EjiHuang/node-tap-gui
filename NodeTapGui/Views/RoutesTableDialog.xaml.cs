using NodeTapGui.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NodeTapGui
{
    /// <summary>
    /// RoutesTableDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RoutesTableDialog
    {
        #region properties

        /// <summary>
        ///     路由表集合
        /// </summary>
        public ObservableCollection<RoutesTabelModel> RoutesTableCollection { set; get; }

        #endregion

        #region constructors

        public RoutesTableDialog()
        {
            InitializeComponent();
            // 设置数据上下文
            DataContext = this;
        }

        public RoutesTableDialog(ObservableCollection<RoutesTabelModel> collection) : this()
        {
            // 接收路由表集合
            RoutesTableCollection = collection;
        }


        #endregion

        #region controls event

        /// <summary>
        ///     当前Cell改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            // 将为空的项从集合中移除
            if (null != dg.SelectedItem && string.IsNullOrWhiteSpace((dg.SelectedItem as RoutesTabelModel)?.RouteIp))
            {
                RoutesTableCollection.RemoveAt(dg.SelectedIndex);
            }
        }

        #endregion
    }
}
