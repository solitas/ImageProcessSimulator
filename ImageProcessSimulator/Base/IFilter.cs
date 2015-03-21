using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Base
{
    public interface IFilter : IDisposable
    {
        string FilterIdentify { get; }
        void Apply(Mat org, Mat dst);
        void Apply(Image<Bgr, byte> org, out Image<Bgr, byte> dst);
    }
}
