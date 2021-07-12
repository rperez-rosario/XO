using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveComputingFramework.Processors;
using AdaptiveProductRecommendationEngine.AdapterGroups;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.VariationParameters;

namespace XOSkinWebApp.Controllers
{
  public class ProductRecommendationController : Controller
  {
    public IActionResult Index()
    {
      HollandProcessor processor = new HollandProcessor();
      ProductGroup product = new ProductGroup();

      // TEST CODE (Data to be populated from DB in Production.)
      int numberOfIterations = 10;
      int variationProbabilityPercentage = 8;
      int maxProcessedAdapterGroupCount = 10;

      List<IAdapterGroup> seed = new List<IAdapterGroup>();

      bool logProcessor = false;

      ProductAdapter product1 = new ProductAdapter();
      ProductAdapter product2 = new ProductAdapter();
      ProductAdapter product3 = new ProductAdapter();
      ProductAdapter product4 = new ProductAdapter();
      ProductAdapter product5 = new ProductAdapter();
      
      uint ingredientApple = 1;
      uint ingredientCoconut = 2;
      uint ingredientCarrot = 3;
      uint ingredientOliveOil = 4;
      uint ingredientCoriander = 6;

      List<uint> requiredIngredientsDerivedFromQuestionnaire = new List<uint>();
      List<uint> requiredSpecificProductsDerivedFromQuestionnaire = new List<uint>();
      List<uint> allergenicIngredientsDerivedFromQuestionnaire = new List<uint>();
      List<Dictionary<uint, uint>> ingredientsThatCounteractEachOther =
        new List<Dictionary<uint, uint>>();

      requiredIngredientsDerivedFromQuestionnaire.Add(ingredientApple);
      requiredIngredientsDerivedFromQuestionnaire.Add(ingredientCoconut);
      requiredIngredientsDerivedFromQuestionnaire.Add(ingredientCoriander);

      requiredSpecificProductsDerivedFromQuestionnaire.Add(1);
      requiredSpecificProductsDerivedFromQuestionnaire.Add(3);
      
      allergenicIngredientsDerivedFromQuestionnaire.Add(ingredientCarrot);

      product1.ProductId = 1;
      product1.QuantityAvailableInStock = 10;
      product1.IngredientId.Add(ingredientApple);
      product1.IngredientId.Add(ingredientCarrot);
      product1.IngredientId.Add(ingredientCoriander);

      product2.ProductId = 2;
      product2.QuantityAvailableInStock = 100;
      product2.IngredientId.Add(ingredientApple);
      
      product3.ProductId = 3;
      product3.QuantityAvailableInStock = 5;
      product3.IngredientId.Add(ingredientOliveOil);
      product3.IngredientId.Add(ingredientCoriander);

      product4.ProductId = 4;
      product4.QuantityAvailableInStock = 20;
      product4.IngredientId.Add(ingredientCarrot);
      product4.IngredientId.Add(ingredientApple);

      product5.ProductId = 5;
      product5.QuantityAvailableInStock = 7;
      product5.IngredientId.Add(ingredientApple);
      product5.IngredientId.Add(ingredientCoconut);

      product.Adapter.Add(product1);
      product.Adapter.Add(product2);
      product.Adapter.Add(product3);
      product.Adapter.Add(product4);
      product.Adapter.Add(product5);

      // TODO: Define Variation Parameter for the application.
      // TODO: Implement Combine, ComputeGroupAppropriateness and Variate on
      //       ProductGroup.cs.
      // TODO: Implement appropriateness function for the application.

      Seed(ref seed, product);

      processor.Log = logProcessor;
      processor.Seed = seed;
      processor.ProcessAdapterGroups(numberOfIterations, variationProbabilityPercentage,
        maxProcessedAdapterGroupCount);

      // TODO: Populate View with top result.

      // END TEST CODE (Data to be populated from DB in Production.)

      return View();
    }

    private void Seed(ref List<IAdapterGroup> Seed, ProductGroup Product)
    {
      // TODO: Implement.
    }
  }
}
