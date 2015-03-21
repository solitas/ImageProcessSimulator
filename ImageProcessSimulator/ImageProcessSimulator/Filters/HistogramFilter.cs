using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Base;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace ImageProcessSimulator.Filters
{
    public class HistogramFilter : IFilter
    {
        public string FilterIdentify
        {
            get { return "Histogram"; }
        }

        public void Apply(Mat org, Mat dst)
        {

            int width = org.Width;
            int height = org.Height;

            var gray = org.ToImage<Gray, byte>();

            var dstImage = dst.ToImage<Gray, byte>(true);
            dstImage.SetZero();

            int[] hist = new int[256];
            hist.Initialize();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int val = (int)gray.Data[y, x, 0];
                    hist[val]++;
                }
            }
            int histMax = 0;

            for (int i = 0; i < 256; i++)
            {
                if (hist[i] > histMax)
                {
                    histMax = hist[i];
                }
            }

            for (int x = 0; x < 256; x++)
            {
                double row = (double)hist[x] * 255 / (double)histMax;
                Console.WriteLine("row : {0}", row);
                for (int y = 0; y < (int)row; y++)
                {
                    dstImage.Data[y, x, 0] = 255;
                }
            }
            dst = dstImage.Mat;
        }

        public void Apply(Image<Bgr, byte> org, out Image<Bgr, byte> dst)
        {
            int width = org.Width;
            int height = org.Height;

            var gray = org.Convert<Gray, byte>();
            Image<Gray, byte> dstImage = new Image<Gray, byte>(gray.Size);
            dstImage.SetZero();


            float[] ranges = { 0.0f, 255.0f };

            using (Mat histogram = new Mat())
            using (VectorOfMat vm = new VectorOfMat())
            {
                vm.Push(gray.Mat);
                CvInvoke.CalcHist(vm, new int[] { 0 }, null, histogram, new int[] { 255 }, ranges, false);

                double[] binVal = new double[histogram.Size.Height];  
                GCHandle handle = GCHandle.Alloc(binVal, GCHandleType.Pinned);
                
                using (Matrix<double> m = new Matrix<double>(binVal.Length, 1, handle.AddrOfPinnedObject(), sizeof (double)))
                {
                    histogram.ConvertTo(m, DepthType.Cv64F);
                }

                int histMax = 0;

                for (int i = 0; i < 255; i++)
                {
                    if (binVal[i] > histMax)
                    {
                        histMax = (int)binVal[i];
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    int x2 = (int)((double)x * 255 / (double)width);
                    int row = (int)((double)binVal[x2] * height / (double)histMax);
                    for (int y = height - 1; y >= height - row; y--)
                    {
                        dstImage[y, x] = new Gray(255);
                    }
                }

                dst = dstImage.Convert<Bgr, byte>();
            }
        }

        public void Dispose()
        {

        }
    }
}
