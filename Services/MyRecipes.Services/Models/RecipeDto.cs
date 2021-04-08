namespace MyRecipes.Services.Models
{
    using System;
    using System.Collections.Generic;

    public class RecipeDto
    {
        public RecipeDto()
        {
            this.Categories = new List<string>();
            this.Ingredients = new Dictionary<string, string>();
        }

        public string Name { get; set; }

        public TimeSpan Time { get; set; }

        public string PortionsCount { get; set; }

        public string Instructions { get; set; }

        public string RemoteImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public ICollection<string> Categories { get; set; }

        public IDictionary<string, string> Ingredients { get; set; }
    }
}
