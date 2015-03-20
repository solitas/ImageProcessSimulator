using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Base;
using Emgu.CV;

namespace BaseTest
{
    class FilterStub : IFilter
    {
        public void Dispose()
        {
            
        }

        public string FilterIdentify
        {
            get { return "_StubFilter"; }
        }

        /// <summary>
        /// 원본의 레퍼런스를 연결한다
        /// </summary>
        /// <param name="org"></param>
        /// <param name="dst"></param>
        public void Apply(Mat org, Mat dst)
        {
            // 원본의 레퍼런스를 결과에 연결
            dst = org;
        }
    }
}
