using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveProductRecommendationEngine.Common
{
  public enum EnumAdapterGroupVariationParameter : int
  {
    RecommendedIngredientsDerivedFromQuestionnaire = 0,
    RequiredSpecificProductsDerivedFromQuestionnaire = 1,
    AllergenicIngredientsDerivedFromQuestionnaire = 2,
    ingredientsThatWorkWellWithEachOther = 3,
    IngredientsThatCounteractEachOther = 4,
    NumberToBeConsideredAsOnHighStock = 5,
    ProductGroup = 6
  }
}
