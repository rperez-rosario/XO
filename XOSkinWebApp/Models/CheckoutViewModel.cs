using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ValidationAttributeHelper;

namespace XOSkinWebApp.Models
{
  public class CheckoutViewModel
  {
    public List<ShoppingCartLineItemViewModel> LineItem { get; set; }

    [Required(ErrorMessage = "Name on credit card required.")]
    public String BillingName { get; set; }

    [Required(ErrorMessage = "Billing address line 1 required.")]
    public String BillingAddress1 { get; set; }

    public String BillingAddress2 { get; set; }

    [Required(ErrorMessage = "Billing city required.")]
    public String BillingCity { get; set; }

    [Required(ErrorMessage = "Billing state required.")]
    public String BillingState { get; set; }

    [Required(ErrorMessage = "Billing country required.")]
    public String BillingCountry { get; set; }

    [Required(ErrorMessage = "Billing postal code required.")]
    public String BillingPostalCode { get; set; }

    public bool ShippingAddressSame { get; set; }

    public String ShippingName { get; set; }    
    public String ShippingAddress1 { get; set; }
    public String ShippingAddress2 { get; set; }
    public String ShippingCity { get; set; }
    public String ShippingState { get; set; }
    public String ShippingCountry { get; set; }
    public String ShippingPostalCode { get; set; }

    [Required]
    public String CreditCardNumber { get; set; }
    [Required]
    public String CreditCardCVC { get; set; }
    [Required]
    public DateTime CreditCardExpirationDate { get; set; }

    public CheckoutViewModel()
    {
      LineItem = new List<ShoppingCartLineItemViewModel>();
    }
  }
}
