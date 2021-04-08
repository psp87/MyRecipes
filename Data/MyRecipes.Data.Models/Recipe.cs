namespace MyRecipes.Data.Models
{
    using System;
    using System.Collections.Generic;

    using MyRecipes.Data.Common.Models;

    public class Recipe : BaseDeletableModel<int>
    {
        public Recipe()
        {
            this.Ingredients = new HashSet<RecipeIngredient>();
            this.Categories = new HashSet<RecipeCategory>();
            this.Images = new HashSet<Image>();
        }

        public string Name { get; set; }

        public string Instructions { get; set; }

        public TimeSpan Time { get; set; }

        public string PortionsCount { get; set; }

        public string OriginalUrl { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<RecipeIngredient> Ingredients { get; set; }

        public virtual ICollection<RecipeCategory> Categories { get; set; }

        public virtual ICollection<Image> Images { get; set; }
    }
}
