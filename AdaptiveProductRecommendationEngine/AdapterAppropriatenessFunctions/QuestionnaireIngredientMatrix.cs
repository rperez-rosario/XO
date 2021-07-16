using System;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveProductRecommendationEngine.Common;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.AdapterGroups;
using System.Collections.Generic;

namespace AdaptiveProductRecommendationEngine.AdapterAppropriatenessFunctions
{
  public class QuestionnaireIngredientMatrix : IAdapterAppropriatenessFunction
  {
    public void ComputeAppropriateness(ref IAdapter Adapter)
    {
      List<object> variationParameter = (List<object>)Adapter.VariationParameter;
      ProductGroup productGroup = 
        (ProductGroup)variationParameter[(int)EnumAdapterGroupVariationParameter.ProductGroup];
      List<uint> recommendedIngredientsDerivedFromQuestionnaire =
        (List<uint>)variationParameter[(int)EnumAdapterGroupVariationParameter.RecommendedIngredientsDerivedFromQuestionnaire];
      ProductAdapter product = (ProductAdapter)Adapter;
      List<uint> requiredSpecificProductsDerivedFromQuestionnaire =
        (List<uint>)variationParameter[(int)EnumAdapterGroupVariationParameter.RequiredSpecificProductsDerivedFromQuestionnaire];
      List<uint> allergenicIngredientsDerivedFromQuestionnaire =
        (List<uint>)variationParameter[(int)EnumAdapterGroupVariationParameter.AllergenicIngredientsDerivedFromQuestionnaire];
      long numberToBeConsideredAsOnHighStock =
        (long)variationParameter[(int)EnumAdapterGroupVariationParameter.NumberToBeConsideredAsOnHighStock];

      bool allergenFound = false;

      foreach (uint allergenicIngredient in allergenicIngredientsDerivedFromQuestionnaire)
        foreach (uint ingredient in product.IngredientId)
          if (allergenicIngredient == ingredient)
          {
            Adapter.Appropriateness = decimal.MinValue + 1000;
            allergenFound = true;
          }
            
      if (!allergenFound)
      {
        if (product.QuantityAvailableInStock <= 0)
        {
          foreach (uint requiredProductId in requiredSpecificProductsDerivedFromQuestionnaire)
          {
            if (product.ProductId == requiredProductId)
            {
              Adapter.Appropriateness = decimal.MaxValue - 1000.0M;
              break;
            }
            else
            {
              foreach (uint ingredient in product.IngredientId)
                foreach (uint recommendedIngredient in recommendedIngredientsDerivedFromQuestionnaire)
                  if (ingredient == recommendedIngredient)
                    Adapter.Appropriateness += 1.0M;
              
              if (product.QuantityAvailableInStock >= numberToBeConsideredAsOnHighStock)
                Adapter.Appropriateness += 1.0M;
              
              

            }
          }
        }
        else
        {
          Adapter.Appropriateness = decimal.MinValue + 1000.0M;
        }
      }
    }

    public void ComputeAppropriateness(ref IAdapterGroup Adapter)
    {
      List<object> variationParameter = (List<object>)Adapter.VariationParameter;
      Dictionary<uint, uint> ingredientsThatCounteractEachOther =
       (Dictionary<uint, uint>)variationParameter[(int)EnumAdapterGroupVariationParameter.IngredientsThatCounteractEachOther];
      Dictionary<uint, uint> ingredientsThatWorkWellWithEachOther =
        (Dictionary<uint, uint>)variationParameter[(int)EnumAdapterGroupVariationParameter.ingredientsThatWorkWellWithEachOther];
      
      
      // Loop through all product ingredients in group, Decimal.Min + 1000 and exit loop for 
      // any pair that counteract each other.
      throw new NotImplementedException();
    }
  }
}