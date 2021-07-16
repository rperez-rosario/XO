using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveProductRecommendationEngine.Common
{
  public enum EnumAdapterGroupVariationParameter : int
  {
    RequiredIngredientsDerivedFromQuestionnaire = 0,
    RequiredSpecificProductsDerivedFromQuestionnaire = 1,
    AllergenicIngredientsDerivedFromQuestionnaire = 2,
    ProductGroup = 3
  }
}
