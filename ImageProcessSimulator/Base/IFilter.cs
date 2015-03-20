using System;
using Emgu.CV;

namespace Base
{
    public interface IFilter : IDisposable
    {
        string FilterIdentify { get; }
        void Apply(Mat org, Mat dst);
    }
}
