using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;

namespace AdaptiveProductRecommendationEngine.Adapters
{
  class ProductAdapter : IAdapter
  {
    public decimal Appropriateness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IVariationParameter VariationParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Variate(IAdapterGroup AdapterGroup)
    {
      throw new NotImplementedException();
    }
  }
}
