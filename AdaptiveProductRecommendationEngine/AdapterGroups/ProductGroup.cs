using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.AdapterAppropriatenessFunctions;
using AdaptiveProductRecommendationEngine.VariationParameters;
using AdaptiveProductRecommendationEngine.Common;

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
      ProductAdapter productToAdd;
      ProductAdapter currentProduct;
      Random prng = new Random();
      bool productFoundInCombination = false;
      List<object> variationParameter = (List<object>)VariationParameter.Parameter;
      int largestAdapterGroupCount = AdapterGroup.Adapter.Count > this.Adapter.Count ?
        AdapterGroup.Adapter.Count : this.Adapter.Count;
      int i = 0;
      int j = 0;
      int k = 0;
      
      for (; i < largestAdapterGroupCount; i++)
      {
        combination = new ProductGroup(new List<IAdapter>());
        combination.VariationParameter = new VariationParameter();
        combination.VariationParameter.Parameter = this.VariationParameter.Parameter;
        combination.Adapter = new List<IAdapter>();
        
        for (j = 0; j < largestAdapterGroupCount; j++)
        {
          if (combination.Adapter.Count == 0)
          {
            productToAdd = prng.Next(0, 2) == 0 ? 
              (ProductAdapter)this.Adapter[j] : (ProductAdapter)Adapter[j];

            if (productToAdd.QuantityAvailableInStock > 0)
            {
              combination.Adapter.Add(productToAdd);
            }
            else
            {
              j--;
            }
          }
          else
          {
            if (!productFoundInCombination)
            {
              productToAdd =
                prng.Next(0, 2) == 0 ?
                (ProductAdapter)this.Adapter[j] : (ProductAdapter)Adapter[j];
            }
            else
            {
              productToAdd =
                prng.Next(0, 2) == 0 ?
                (ProductAdapter)this.Adapter[prng.Next(0, this.Adapter.Count)] :
                (ProductAdapter)Adapter[prng.Next(0, Adapter.Count)];
            }

            for (k = 0; k < combination.Adapter.Count; k++)
            {
              currentProduct = (ProductAdapter)combination.Adapter[k];
              productFoundInCombination = false;

              if (productToAdd.ProductId == currentProduct.ProductId)
              {
                productFoundInCombination = true;
                break;
              }
            }
            if (!productFoundInCombination && productToAdd.QuantityAvailableInStock > 0)
            {
              combination.Adapter.Add((IAdapter)productToAdd);
            }
            else
            {
              j--;
            }
          }
        }
      }
      return combination;
    }

    public void Variate()
    {
      Random prng = new Random();
      List<object> parameter = (List<object>)VariationParameter.Parameter;
      ProductGroup product = 
        (ProductGroup)parameter[(int)EnumAdapterGroupVariationParameter.ProductGroup];
      ProductGroup variation = new ProductGroup(new List<IAdapter>());
      ProductAdapter productToAdd;
      ProductAdapter currentProduct;
      bool productFoundInVariation = false;
      int largestAdapterGroupCount = this.Adapter.Count;
      int i = 0;
      int j = 0;
      int k = 0;

      for (; i < largestAdapterGroupCount; i++)
      {
        variation = new ProductGroup(new List<IAdapter>());
        variation.VariationParameter = new VariationParameter();
        variation.VariationParameter.Parameter = this.VariationParameter.Parameter;
        variation.Adapter = new List<IAdapter>();

        for (j = 0; j < largestAdapterGroupCount; j++)
        {
          if (variation.Adapter.Count == 0)
          {
            productToAdd = prng.Next(0, 2) == 0 ?
              (ProductAdapter)this.Adapter[j] : (ProductAdapter)Adapter[j];

            if (productToAdd.QuantityAvailableInStock > 0)
            {
              variation.Adapter.Add(productToAdd);
            }
            else
            {
              j--;
            }
          }
          else
          {
            productToAdd = 
              (ProductAdapter)product.Adapter[prng.Next(0, product.Adapter.Count - 1)];

            for (k = 0; k < variation.Adapter.Count; k++)
            {
              currentProduct = (ProductAdapter)variation.Adapter[k];
              productFoundInVariation = false;

              if (productToAdd.ProductId == currentProduct.ProductId)
              {
                productFoundInVariation = true;
                break;
              }
            }
            if (!productFoundInVariation && productToAdd.QuantityAvailableInStock > 0)
            {
              variation.Adapter.Add((IAdapter)productToAdd);
            }
            else
            {
              j--;
            }
          }
        }
        this.Adapter = variation.Adapter;
      }
    }

    public void ComputeGroupAppropriateness()
    {
      IAdapter product = null;
      IAdapterGroup productGroup = null;
      decimal productAppropriateness = 0.0M;
      int i = 0;

      for (; i < Adapter.Count; i++)
      {
        product = Adapter.ElementAt(i);
        foreach (IAdapterAppropriatenessFunction appropriatenessFunction in 
          AdapterAppropriatenessFunction)
        {
          appropriatenessFunction.ComputeAppropriateness(ref product);
          if (productAppropriateness < 0)
            break;
          productAppropriateness += product.Appropriateness;
        }
        if (productAppropriateness < 0)
          break;
      }

      productAppropriateness = productAppropriateness / Adapter.Count;
      productAppropriateness = productAppropriateness / AdapterAppropriatenessFunction.Count;

      productGroup = this;

      foreach (IAdapterAppropriatenessFunction appropriatenessFunction in
        AdapterAppropriatenessFunction)
      {
        appropriatenessFunction.ComputeAppropriateness(ref productGroup);
      }

      AdapterGroupAppropriateness = AdapterGroupAppropriateness / Adapter.Count;
      AdapterGroupAppropriateness =
        AdapterGroupAppropriateness / AdapterAppropriatenessFunction.Count;
      AdapterGroupAppropriateness += productAppropriateness;
    }
  }
}
