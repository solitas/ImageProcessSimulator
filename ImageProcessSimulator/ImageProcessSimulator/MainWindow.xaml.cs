using System;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ImageProcessSimulator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        const string SampleFile = "Sample/lena.jpg";
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            using (var src = new Mat(SampleFile, LoadImageType.Color))
            {
                Frame.Source = src.ToImage<Bgr, Byte>();
            }
        }
    }
}
