﻿namespace MyRecipes.Web.ViewModels.Recipes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateRecipeInputModel
    {
        [Required]
        [MinLength(4)]
        public string Name { get; set; }

        [Required]
        [MinLength(100)]
        public string Instructions { get; set; }

        [Range(1, 24 * 60)]
        [Display(Name = "Preparation Time (in minutes)")]
        public int PreparationTime { get; set; }

        [Range(1, 24 * 60)]
        [Display(Name = "Cooking Time (in minutes)")]
        public int CookingTime { get; set; }

        [Range(1, 100)]
        public int PortionsCount { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IEnumerable<KeyValuePair<string, string>> CategoriesItems { get; set; }
    }
}
