using System;
using Base;
using Emgu.CV;
using NUnit.Framework;

namespace BaseTest
{
    [TestFixture]
    public class FIlterTest
    {
        [Test]
        public void FilterApplyTest()
        {
            Mat src = new Mat();
            Mat dst = new Mat();

            Context context = new ContextStub();
            context.SetFilter(new FilterStub());
            context.Apply(src, dst);    // 둘이 같은 객체로 된다
            Assert.AreEqual(src,dst);
        }
    }
}
