using System;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveProductRecommendationEngine.Common;

namespace AdaptiveProductRecommendationEngine.AdapterAppropriatenessFunctions
{
  public class QuestionnaireIngredientMatrix : IAdapterAppropriatenessFunction
  {
    public void ComputeAppropriateness(ref IAdapter Adapter)
    {
      // 1 point for each ingredient that also appears as recommended by the questionnaire.
      // Decimal.Max - 1000 for product marked as required by the questionnaire.
      // Decimal.Min + 1000 for product marked as an allergen by the questionnaire.
      // Decimal.Min + 1000 for product marked as out of stock.
      // 1 point for product whose stock is high (define high.)
      throw new NotImplementedException();
    }

    public void ComputeAppropriateness(ref IAdapterGroup Adapter)
    {
      // Loop through all product ingredients in group, Decimal.Min + 1000 and exit loop for 
      // any pair that counteract each other.
      throw new NotImplementedException();
    }
  }
}