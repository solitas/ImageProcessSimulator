using System.Windows;
using ImageProcessSimulator.ViewModel;

namespace ImageProcessSimulator.View
{
    /// <summary>
    /// BasicWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BasicWindow : Window
    {
        public BasicWindow()
        {
            InitializeComponent();
            DataContext = new BasicViewModel(SourceBox, DestBox);
        }
    }
}
