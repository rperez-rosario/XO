using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;

namespace AdaptiveProductRecommendationEngine.AdapterGroups
{
  public class ProductGroup : IAdapterGroup
  {
    public List<IAdapter> Adapter { get; set; }
    public List<IAdapterAppropriatenessFunction> AdapterAppropriatenessFunction { get; set; }
    public IVariationParameter VariationParameter { get; set; }
    public decimal AdapterGroupAppropriateness { get; set; }

    public ProductGroup()
    {
      Adapter = new List<IAdapter>();
      AdapterAppropriatenessFunction = new List<IAdapterAppropriatenessFunction>();
      VariationParameter = (IVariationParameter)new object();
      AdapterGroupAppropriateness = 0.0M;

      AdapterAppropriatenessFunction.Add(new QuestionnaireIngredientMatrix());
    }

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
