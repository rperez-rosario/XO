using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ProductIngredient
    {
        public long Id { get; set; }
        public long Product { get; set; }
        public long Ingredient { get; set; }

        public virtual Ingredient IngredientNavigation { get; set; }
        public virtual Product ProductNavigation { get; set; }
    }
}
