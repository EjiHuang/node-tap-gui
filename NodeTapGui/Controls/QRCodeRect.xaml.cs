using NotifyProperty;
using System.Threading.Tasks;
using System.Windows;

namespace NodeTapGui.Controls
{
    /// <summary>
    ///     QRCodeRect.xaml 的交互逻辑
    /// </summary>
    public partial class QRCodeRect : Window
    {
        public NotifyPropertyEx<float> TargetWidth { get; } = 0f;
        public NotifyPropertyEx<float> TargetHeight { get; } = 0f;
        public NotifyPropertyEx<float> TargetLeft { get; } = 0f;
        public NotifyPropertyEx<float> TargetTop { get; } = 0f;

        public QRCodeRect(float targetWidth, float targetHeight, float targetLeft, float targetTop, float fullWidth, float fullHeight)
        {
            InitializeComponent();

            DataContext = this;
            

            TargetWidth.Value = targetWidth;
            TargetHeight.Value = targetHeight;
            TargetLeft.Value = targetLeft;
            TargetTop.Value = targetTop;

            Loaded += async (s, e) =>
            {
                await Task.Delay(3000);
                Close();
            };
        }
    }
}
