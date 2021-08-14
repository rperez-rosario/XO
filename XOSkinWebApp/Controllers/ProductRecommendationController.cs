using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AdaptiveComputingFramework.Interfaces;
using AdaptiveComputingFramework.Processors;
using AdaptiveProductRecommendationEngine.AdapterGroups;
using AdaptiveProductRecommendationEngine.Adapters;
using AdaptiveProductRecommendationEngine.VariationParameters;
using XOSkinWebApp.Models;
using XOSkinWebApp.ORM;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class ProductRecommendationController : Controller
  {
    private readonly XOSkinContext _context;

    public ProductRecommendationController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("ProductRecommendation.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("ProductRecommendation.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      List<ProductViewModel> product = TestProductRecommendations();

      return View(product);
    }

    private List<ProductViewModel> TestProductRecommendations()
    {
      HollandProcessor processor = new HollandProcessor();
      ProductGroup productGroup = new ProductGroup();
      IVariationParameter adapterVariationParameter = null;
      List<object> variationParameter = null;
      bool allergenFound = false;
      ProductAdapter testProduct;
      List<ProductViewModel> productView = new List<ProductViewModel>();
      int i = 0;
      int j = 0;
      int k = 0;

      adapterVariationParameter = new VariationParameter();

      // TEST CODE (Data to be populated from DB in Production.)
      int numberOfIterations = 77;
      int variationProbabilityPercentage = 18;
      int maxProcessedAdapterGroupCount = 10;
      int maxAdapterGroupAdapterCount = 5; // Max number of product to recommend.

      long numberToBeConsideredAsOnHighStock = 10000;

      List<IAdapterGroup> seed = new List<IAdapterGroup>();

      bool logProcessor = false;

      ProductAdapter product1 = new ProductAdapter();
      ProductAdapter product2 = new ProductAdapter();
      ProductAdapter product3 = new ProductAdapter();
      ProductAdapter product4 = new ProductAdapter();
      ProductAdapter product5 = new ProductAdapter();
      ProductAdapter product6 = new ProductAdapter();
      ProductAdapter product7 = new ProductAdapter();
      ProductAdapter product8 = new ProductAdapter();
      ProductAdapter product9 = new ProductAdapter();
      ProductAdapter product10 = new ProductAdapter();

      uint ingredientApple = 1;
      uint ingredientCoconut = 2;
      uint ingredientCarrot = 3;
      uint ingredientOliveOil = 4;
      uint ingredientCoriander = 6;
      uint ingredientPeppermint = 7;
      uint ingredientEucalyptus = 9;
      uint ingredientBalsamicVinegar = 10;

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

      //Recommended ingredients derived from questionnaire.
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientApple);
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientCoconut);
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientCoriander);
      recommendedIngredientsDerivedFromQuestionnaire.Add(ingredientEucalyptus);

      // Allergens.
      allergenicIngredientsDerivedFromQuestionnaire.Add(ingredientCarrot);

      // Counteracting and collaborating ingredients.
      ingredientsThatCounteractEachOther.Add(ingredientCarrot, ingredientApple);
      ingredientsThatWorkWellWithEachOther.Add(ingredientOliveOil, ingredientCoriander);

      // Full product list.
      product1.IngredientId = new List<uint>();
      product1.VariationParameter = new VariationParameter();
      product1.ProductId = 1;
      product1.QuantityAvailableInStock = 10;
      product1.IngredientId.Add(ingredientApple);
      product1.IngredientId.Add(ingredientCarrot);
      product1.IngredientId.Add(ingredientCoriander);
      product1.VariationParameter.Parameter = variationParameter;
      product1.ProductName = "Apple Carrot Coriander Mask";
      product1.ProductPrice = 19.00M;

      product2.IngredientId = new List<uint>();
      product2.VariationParameter = new VariationParameter();
      product2.ProductId = 2;
      product2.QuantityAvailableInStock = 100;
      product2.IngredientId.Add(ingredientApple);
      product2.VariationParameter.Parameter = variationParameter;
      product2.ProductName = "Apple Cleanser";
      product2.ProductPrice = 15.00M;

      product3.IngredientId = new List<uint>();
      product3.VariationParameter = new VariationParameter();
      product3.ProductId = 3;
      product3.QuantityAvailableInStock = 5;
      product3.IngredientId.Add(ingredientOliveOil);
      product3.IngredientId.Add(ingredientCoriander);
      product3.VariationParameter.Parameter = variationParameter;
      product3.ProductName = "Olive Oil Coriander Lip Balm";
      product3.ProductPrice = 12.00M;

      product4.IngredientId = new List<uint>();
      product4.VariationParameter = new VariationParameter();
      product4.ProductId = 4;
      product4.QuantityAvailableInStock = 0;
      product4.IngredientId.Add(ingredientCarrot);
      product4.IngredientId.Add(ingredientApple);
      product4.VariationParameter.Parameter = variationParameter;
      product4.ProductName = "Carrot Apple Scrub";
      product4.ProductPrice = 22.00M;

      product5.IngredientId = new List<uint>();
      product5.VariationParameter = new VariationParameter();
      product5.ProductId = 5;
      product5.QuantityAvailableInStock = 7;
      product5.IngredientId.Add(ingredientApple);
      product5.IngredientId.Add(ingredientCoconut);
      product5.VariationParameter.Parameter = variationParameter;
      product5.ProductName = "Apple Coconut Scrub";
      product5.ProductPrice = 39.00M;

      product6.IngredientId = new List<uint>();
      product6.VariationParameter = new VariationParameter();
      product6.ProductId = 6;
      product6.QuantityAvailableInStock = 12;
      product6.IngredientId.Add(ingredientOliveOil);
      product6.IngredientId.Add(ingredientApple);
      product6.VariationParameter.Parameter = variationParameter;
      product6.ProductName = "Olive Oil Apple Resurfactant";
      product6.ProductPrice = 21.00M;

      product7.IngredientId = new List<uint>();
      product7.VariationParameter = new VariationParameter();
      product7.ProductId = 7;
      product7.QuantityAvailableInStock = 30;
      product7.IngredientId.Add(ingredientCarrot);
      product7.IngredientId.Add(ingredientCoriander);
      product7.VariationParameter.Parameter = variationParameter;
      product7.ProductName = "Carrot Coriander Paste";
      product7.ProductPrice = 13.00M;

      product8.IngredientId = new List<uint>();
      product8.VariationParameter = new VariationParameter();
      product8.ProductId = 8;
      product8.QuantityAvailableInStock = 7;
      product8.IngredientId.Add(ingredientEucalyptus);
      product8.VariationParameter.Parameter = variationParameter;
      product8.ProductName = "Eucalyptus Night Rub";
      product8.ProductPrice = 38.00M;

      product9.IngredientId = new List<uint>();
      product9.VariationParameter = new VariationParameter();
      product9.ProductId = 9;
      product9.QuantityAvailableInStock = 22;
      product9.IngredientId.Add(ingredientCarrot);
      product9.IngredientId.Add(ingredientPeppermint);
      product9.VariationParameter.Parameter = variationParameter;
      product9.ProductName = "Carrot Peppermint Mask";
      product9.ProductPrice = 19.00M;

      product10.IngredientId = new List<uint>();
      product10.VariationParameter = new VariationParameter();
      product10.ProductId = 10;
      product10.QuantityAvailableInStock = 30;
      product10.IngredientId.Add(ingredientApple);
      product10.IngredientId.Add(ingredientBalsamicVinegar);
      product10.VariationParameter.Parameter = variationParameter;
      product10.ProductName = "Balsamic Apple Rub";
      product10.ProductPrice = 42.00M;

      productGroup.Adapter = new List<IAdapter>();
      productGroup.Adapter.Add(product1);
      productGroup.Adapter.Add(product2);
      productGroup.Adapter.Add(product3);
      productGroup.Adapter.Add(product4);
      productGroup.Adapter.Add(product5);
      productGroup.Adapter.Add(product6);
      productGroup.Adapter.Add(product7);
      productGroup.Adapter.Add(product8);
      productGroup.Adapter.Add(product9);
      productGroup.Adapter.Add(product10);

      // Prune products with allergens according to questionnaire.
      for (; i < productGroup.Adapter.Count; i++)
      {
        testProduct = (ProductAdapter)productGroup.Adapter[i];
        allergenFound = false;
        for (j = 0; j < allergenicIngredientsDerivedFromQuestionnaire.Count; j++)
        {
          for (k = 0; k < testProduct.IngredientId.Count; k++)
          {
            if (allergenicIngredientsDerivedFromQuestionnaire[j] ==
              testProduct.IngredientId[k])
            {
              productGroup.Adapter.Remove((IAdapter)testProduct);
              allergenFound = true;
              i--;
              break;
            }
          }
          if (allergenFound)
          {
            break;
          }
        }
      }

      variationParameter.Add(productGroup);

      requiredSpecificProductsDerivedFromQuestionnaire.Add(product3.ProductId);
      requiredSpecificProductsDerivedFromQuestionnaire.Add(product10.ProductId);

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

      // Prune negative appropriateness.
      foreach (IAdapterGroup group in processor.AdapterGroup)
        group.Adapter.RemoveAll(x => x.Appropriateness < 0);

      foreach (ProductAdapter adapter in processor.TopAdapterGroup.Adapter)
      {
        productView.Add(new ProductViewModel(adapter.ProductName, adapter.ProductPrice));
      }
      // END TEST CODE (Data to be populated from DB in Production.)
      return productView;
    }

    private void Seed(ref HollandProcessor Hp, ProductGroup Product,
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
                (ProductAdapter)Product.Adapter[prng.Next(0, Product.Adapter.Count)];
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
                (ProductAdapter)Product.Adapter[prng.Next(0, Product.Adapter.Count)];
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

    private bool IsThereStockOnMinimumNumberOfSku(int MaxAdapterGroupAdapterCount,
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
