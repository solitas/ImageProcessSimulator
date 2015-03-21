using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Base;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ImageProcessSimulator.Filters
{
    class BinaryFilter : IFilter
    {
        private int _threshold;
        private int _maxValue;

        public BinaryFilter(int thres = 100, int maxValue = 255)
        {
            _threshold = thres;
            _maxValue = maxValue;
        }

        public string FilterIdentify
        {
            get { return "BinaryThreshold"; }
        }

        public void Apply(Mat org, Mat dst)
        {
            Mat gray = org.ToImage<Gray, byte>().Mat;
            CvInvoke.Threshold(gray, dst, _threshold, _maxValue, ThresholdType.Binary);
        }

        public void Apply(Image<Bgr,byte> org, out Image<Bgr,byte> dst)
        {
            using (Image<Gray, byte> grayImage = org.Convert<Gray, byte>())
            {
                dst = grayImage.ThresholdBinary(new Gray(_threshold), new Gray(_maxValue)).Convert<Bgr,Byte>();
            }
        }
        public void Dispose()
        {

        }
    }
}
