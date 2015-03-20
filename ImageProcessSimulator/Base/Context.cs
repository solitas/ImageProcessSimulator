using System;
using Emgu.CV;

namespace Base
{
    public abstract class Context
    {
        protected IFilter BaseFilter;

        protected Context()
        {
            
        }

        public void SetFilter(IFilter filter)
        {
            if (BaseFilter != null)
            {
                BaseFilter.Dispose();
            }

            BaseFilter = filter;
        }

        public void Apply(Mat org, Mat dst)
        {
            if (BaseFilter == null)
                throw new NullReferenceException("Filter is null. Please Set Filter in Context");

            BaseFilter.Apply(org,dst);
        }
    }
}
