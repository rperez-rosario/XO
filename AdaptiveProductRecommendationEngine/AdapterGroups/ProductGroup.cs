using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;

namespace AdaptiveProductRecommendationEngine.AdapterGroups
{
  class ProductGroup : IAdapterGroup
  {
    public List<IAdapter> Adapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public List<IAdapterAppropriatenessFunction> AdapterAppropriatenessFunction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IVariationParameter VariationParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public decimal AdapterGroupAppropriateness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IAdapterGroup Combine(IAdapterGroup AdapterGroup)
    {
      throw new NotImplementedException();
    }

    public void ComputeGroupAppropriateness()
    {
      throw new NotImplementedException();
    }

    public void Variate()
    {
      throw new NotImplementedException();
    }
  }
}
