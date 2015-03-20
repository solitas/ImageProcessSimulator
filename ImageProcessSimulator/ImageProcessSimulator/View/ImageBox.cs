using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageProcessSimulator.Util;

namespace ImageProcessSimulator.View
{
    public class ImageBox : Control
    {
        private Image PART_Image;

        /// <summary>
        /// Display Image Source
        /// </summary>
        public IImage Source
        {
            set
            {
                IImage img = value;

                if (img != null)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {

                            BitmapSource source = BitmapSourceConverter.ToBitmapSource(img);
                            PART_Image.Source = source;
                            img.Dispose();

                        });
                    }
                    catch (TaskCanceledException e)
                    {

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        static ImageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageBox),
                new FrameworkPropertyMetadata(typeof(ImageBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Image = GetTemplateChild("PART_Image") as Image;

            if (PART_Image == null)
            {
                throw new ArgumentNullException("Can't find PART_Image");
            }
        }

        public void ShowImage(string file)
        {
            using (var src = new Mat(file, LoadImageType.Color))
            {
                Source = src.ToImage<Bgr, Byte>();
            }
        }
    }
}
