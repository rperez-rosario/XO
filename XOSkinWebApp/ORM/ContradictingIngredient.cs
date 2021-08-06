using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ContradictingIngredient
    {
        public long Id { get; set; }
        public long IngredientA { get; set; }
        public long IngredientB { get; set; }

        public virtual Ingredient IngredientANavigation { get; set; }
        public virtual Ingredient IngredientBNavigation { get; set; }
    }
}
