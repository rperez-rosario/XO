using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;

namespace AdaptiveProductRecommendationEngine.Adapters
{
  public struct ProductAdapter : IAdapter
  {
    public decimal Appropriateness { get ; set; }
    public IVariationParameter VariationParameter { get; set; }

    public uint ProductId { get; set; }
    public long QuantityAvailableInStock { get; set; }
    public List<uint> IngredientId { get; set; }

    public ProductAdapter(uint ProductId, long QuantityAvailableInStock, 
      List<uint> IngredientId, IVariationParameter VariationParameter)
    {
      this.ProductId = ProductId;
      this.QuantityAvailableInStock = QuantityAvailableInStock;
      this.IngredientId = IngredientId;
      this.VariationParameter = VariationParameter;
      this.Appropriateness = 0.0M;
    }
    
    public void Variate(IAdapterGroup AdapterGroup)
    {
      // Nothing to variate at this level. Do nothing.
    }
  }
}
