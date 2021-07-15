using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.AdapterAppropriatenessFunctions;

namespace AdaptiveProductRecommendationEngine.AdapterGroups
{
  public struct ProductGroup : IAdapterGroup
  {
    public List<IAdapter> Adapter { get; set; }
    public List<IAdapterAppropriatenessFunction> AdapterAppropriatenessFunction { get; set; }
    public int EsporadicVariationProbability { get; set; }
    public IVariationParameter VariationParameter { get; set; }
    public decimal AdapterGroupAppropriateness { get; set; }

    public ProductGroup(List<IAdapter> Adapter)
    {
      AdapterGroupAppropriateness = 0.0M;
      this.Adapter = Adapter;
      AdapterAppropriatenessFunction = new List<IAdapterAppropriatenessFunction>();
      EsporadicVariationProbability = 0;
      VariationParameter = null;

      AdapterAppropriatenessFunction.Add(new QuestionnaireIngredientMatrix());
    }

    public IAdapterGroup Combine(IAdapterGroup AdapterGroup)
    {
      ProductGroup combination = new ProductGroup(new List<IAdapter>());
      ProductAdapter product;
      Random prng = new Random();
      int largestAdapterGroupCount = AdapterGroup.Adapter.Count > this.Adapter.Count ?
        AdapterGroup.Adapter.Count : this.Adapter.Count;
      int i = 0;

      for (; i < largestAdapterGroupCount; i++)
        if (this.Adapter[i] != null && AdapterGroup.Adapter[i] != null)
          combination.Adapter.Add(prng.Next(0, 2) == 0 ?
            this.Adapter[i] : AdapterGroup.Adapter[i]);
        else if (this.Adapter[i] != null || AdapterGroup.Adapter[i] != null)
          combination.Adapter.Add(this.Adapter[i] == null ?
            AdapterGroup.Adapter[i] : this.Adapter[i]);
        else
        {
          product = new ProductAdapter();
          product.Variate(this);
          combination.Adapter.Add(product);
        }
      return combination;
    }

    public void ComputeGroupAppropriateness()
    {
      IAdapter product = null;
      IAdapterGroup productGroup = null;
      decimal productAppropiateness = 0.0M;
      int i = 0;

      for (; i < Adapter.Count; i++)
        foreach (IAdapterAppropriatenessFunction appropriatenessFunction in 
          AdapterAppropriatenessFunction)
        {
          appropriatenessFunction.ComputeAppropriateness(ref product);
          productAppropiateness -= product.Appropriateness;
        }
      productAppropiateness = productAppropiateness / Adapter.Count;
      productAppropiateness = productAppropiateness / AdapterAppropriatenessFunction.Count;

      productGroup = this;

      foreach (IAdapterAppropriatenessFunction appropriatenessFunction in
        AdapterAppropriatenessFunction)
        appropriatenessFunction.ComputeAppropriateness(ref productGroup);

      AdapterGroupAppropriateness = AdapterGroupAppropriateness / Adapter.Count;
      AdapterGroupAppropriateness =
        AdapterGroupAppropriateness / AdapterAppropriatenessFunction.Count;
      AdapterGroupAppropriateness -= productAppropiateness;
    }

    public void Variate()
    {
      int i = 0;
      Random prng = new Random();
      
      for (; i < Adapter.Count; i++)
      {
        if (prng.Next(0, 2) == 1)
        {
          Adapter[i].Variate(this);
        }
      }
    }
  }
}
