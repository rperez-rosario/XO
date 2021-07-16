using System;
using System.Collections.Generic;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveComputingFramework.Processors;
using AdaptiveProductRecommendationEngine.AdapterGroups;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.VariationParameters;

namespace Terminal
{
  class Program
  {
    static void Main(string[] args)
    {
      TestProductRecommendations();
    }

    private static void TestProductRecommendations()
    {
      HollandProcessor processor = new HollandProcessor();
      ProductGroup productGroup = new ProductGroup();
      IVariationParameter adapterVariationParameter = null;
      List<object> variationParameter = null;

      adapterVariationParameter = new VariationParameter();

      // TEST CODE (Data to be populated from DB in Production.)
      int numberOfIterations = 10;
      int variationProbabilityPercentage = 8;
      int maxProcessedAdapterGroupCount = 10;
      int maxAdapterGroupAdapterCount = 3; // Max number of product to recommend.
      long numberToBeConsideredAsOnHighStock = 10000;

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

      List<uint> recommendedIngredientsDerivedFromQuestionnaire = new List<uint>();
      List<uint> requiredSpecificProductsDerivedFromQuestionnaire = new List<uint>();
      List<uint> allergenicIngredientsDerivedFromQuestionnaire = new List<uint>();
      Dictionary<uint, uint> ingredientsThatWorkWellWithEachOther =
        new Dictionary<uint, uint>();
      Dictionary<uint, uint> ingredientsThatCounteractEachOther =
        new Dictionary<uint, uint>();

      adapterVariationParameter.Parameter = new List<Object>();
      variationParameter = (List<object>)adapterVariationParameter.Parameter;
      variationParameter.Add(recommendedIngredientsDerivedFromQuestionnaire);
      variationParameter.Add(requiredSpecificProductsDerivedFromQuestionnaire);
      variationParameter.Add(allergenicIngredientsDerivedFromQuestionnaire);
      variationParameter.Add(ingredientsThatWorkWellWithEachOther);
      variationParameter.Add(ingredientsThatCounteractEachOther);
      
      variationParameter.Add(numberToBeConsideredAsOnHighStock);

      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientApple);
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientCoconut);
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientCoriander);

      allergenicIngredientsDerivedFromQuestionnaire.Add(ingredientCarrot);

      ingredientsThatCounteractEachOther.Add(ingredientCarrot, ingredientApple);
      ingredientsThatWorkWellWithEachOther.Add(ingredientOliveOil, ingredientCoriander);

      product1.IngredientId = new List<uint>();
      product1.VariationParameter = new VariationParameter();
      product1.ProductId = 1;
      product1.QuantityAvailableInStock = 10;
      product1.IngredientId.Add(ingredientApple);
      product1.IngredientId.Add(ingredientCarrot);
      product1.IngredientId.Add(ingredientCoriander);
      product1.VariationParameter.Parameter = variationParameter;
      product1.ProductName = "Apple Carrot Coriander Mask";

      product2.IngredientId = new List<uint>();
      product2.VariationParameter = new VariationParameter();
      product2.ProductId = 2;
      product2.QuantityAvailableInStock = 100;
      product2.IngredientId.Add(ingredientApple);
      product2.VariationParameter.Parameter = variationParameter;
      product2.ProductName = "Apple Cleanser";

      product3.IngredientId = new List<uint>();
      product3.VariationParameter = new VariationParameter();
      product3.ProductId = 3;
      product3.QuantityAvailableInStock = 5;
      product3.IngredientId.Add(ingredientOliveOil);
      product3.IngredientId.Add(ingredientCoriander);
      product3.VariationParameter.Parameter = variationParameter;
      product3.ProductName = "Olive Oil Coriander Lip Balm";

      product4.IngredientId = new List<uint>();
      product4.VariationParameter = new VariationParameter();
      product4.ProductId = 4;
      product4.QuantityAvailableInStock = 0;
      product4.IngredientId.Add(ingredientCarrot);
      product4.IngredientId.Add(ingredientApple);
      product4.VariationParameter.Parameter = variationParameter;
      product4.ProductName = "Carrot Apple Scrub";

      product5.IngredientId = new List<uint>();
      product5.VariationParameter = new VariationParameter();
      product5.ProductId = 5;
      product5.QuantityAvailableInStock = 7;
      product5.IngredientId.Add(ingredientApple);
      product5.IngredientId.Add(ingredientCoconut);
      product5.VariationParameter.Parameter = variationParameter;
      product5.ProductName = "Apple Coconut Scrub";

      productGroup.Adapter = new List<IAdapter>();
      productGroup.Adapter.Add(product1);
      productGroup.Adapter.Add(product2);
      productGroup.Adapter.Add(product3);
      productGroup.Adapter.Add(product4);
      productGroup.Adapter.Add(product5);
      variationParameter.Add(productGroup);

      requiredSpecificProductsDerivedFromQuestionnaire.Add(product3.ProductId);

      productGroup.VariationParameter = new VariationParameter();
      productGroup.VariationParameter.Parameter = adapterVariationParameter;

      // Make sure we're not recommending more products than we have.
      maxAdapterGroupAdapterCount = productGroup.Adapter.Count < maxAdapterGroupAdapterCount ?
        productGroup.Adapter.Count : maxAdapterGroupAdapterCount;

      Seed(ref processor, productGroup, maxProcessedAdapterGroupCount, maxAdapterGroupAdapterCount,
        adapterVariationParameter.Parameter);

      processor.Log = logProcessor;
      processor.ProcessAdapterGroups(numberOfIterations, variationProbabilityPercentage,
        maxProcessedAdapterGroupCount);

      // TODO: Populate View with top result.

      // END TEST CODE (Data to be populated from DB in Production.)
    }

    private static void Seed(ref HollandProcessor Hp, ProductGroup Product,
      int MaxProcessedAdapterGroupCount, int maxAdapterGroupAdapterCount,
      object AdapterGroupVariationParameter)
    {
      Random prng = new Random();
      int i = 0;
      int j = 0;
      int k = 0;
      ProductAdapter currentProduct;
      ProductAdapter productToAdd;
      ProductGroup productGroup;
      bool productFoundInSeed = false;

      if (IsThereStockOnMinimumNumberOfSku(maxAdapterGroupAdapterCount, Product))
      {
        for (; i < MaxProcessedAdapterGroupCount; i++)
        {
          productGroup = new ProductGroup();
          productGroup.VariationParameter = new VariationParameter();
          productGroup.VariationParameter.Parameter = AdapterGroupVariationParameter;
          productGroup.Adapter = new List<IAdapter>();
          for (j = 0; j < maxAdapterGroupAdapterCount; j++)
          {
            if (productGroup.Adapter.Count == 0)
            {
              productToAdd =
                (ProductAdapter)Product.Adapter[prng.Next(0, Product.Adapter.Count - 1)];
              if (productToAdd.QuantityAvailableInStock > 0)
              {
                productGroup.Adapter.Add(productToAdd);
              }
              else
              {
                j--;
                //break;
              }
            }
            else
            {
              productToAdd =
                (ProductAdapter)Product.Adapter[prng.Next(0, Product.Adapter.Count - 1)];
              for (k = 0; k < productGroup.Adapter.Count; k++)
              {
                currentProduct = (ProductAdapter)productGroup.Adapter[k];
                productFoundInSeed = false;

                if (productToAdd.ProductId == currentProduct.ProductId)
                {
                  productFoundInSeed = true;
                  break;
                }
              }
              if (!productFoundInSeed && productToAdd.QuantityAvailableInStock > 0)
              {
                productGroup.Adapter.Add((IAdapter)productToAdd);
              }
              else
              {
                j--;
              }
            }
          }
          Hp.AdapterGroup.Add(productGroup);
        }
      }
    }

    private static bool IsThereStockOnMinimumNumberOfSku(int MaxAdapterGroupAdapterCount,
      ProductGroup Product)
    {
      int numberOfSku = 0;
      foreach (ProductAdapter p in Product.Adapter)
      {
        if (p.QuantityAvailableInStock > 0)
          numberOfSku++;
      }
      return MaxAdapterGroupAdapterCount <= numberOfSku;
    }
  }
}
