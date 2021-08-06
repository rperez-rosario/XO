using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Ingredient
    {
        public Ingredient()
        {
            ContradictingIngredientIngredientANavigations = new HashSet<ContradictingIngredient>();
            ContradictingIngredientIngredientBNavigations = new HashSet<ContradictingIngredient>();
            ProductIngredients = new HashSet<ProductIngredient>();
            SynergeticIngredientIngredientANavigations = new HashSet<SynergeticIngredient>();
            SynergeticIngredientIngredientBNavigations = new HashSet<SynergeticIngredient>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ContradictingIngredient> ContradictingIngredientIngredientANavigations { get; set; }
        public virtual ICollection<ContradictingIngredient> ContradictingIngredientIngredientBNavigations { get; set; }
        public virtual ICollection<ProductIngredient> ProductIngredients { get; set; }
        public virtual ICollection<SynergeticIngredient> SynergeticIngredientIngredientANavigations { get; set; }
        public virtual ICollection<SynergeticIngredient> SynergeticIngredientIngredientBNavigations { get; set; }
    }
}
