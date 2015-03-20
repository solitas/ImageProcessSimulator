using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Base;
using Emgu.CV;

namespace ImageProcessSimulator.Filters
{
    class EdgeFilter :IFilter
    {
        public void Dispose()
        {
            
        }

        public string FilterIdentify
        {
            get { return "EdgeFilter"; }
        }

        public void Apply(Mat org, Mat dst)
        {
            CvInvoke.Canny(org,dst,10,100);
        }
    }
}
