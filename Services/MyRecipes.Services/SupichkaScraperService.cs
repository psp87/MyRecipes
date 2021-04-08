namespace MyRecipes.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using AngleSharp;
    using MyRecipes.Data.Common.Repositories;
    using MyRecipes.Data.Models;
    using MyRecipes.Services.Models;

    public class SupichkaScraperService : ISupichkaScraperService
    {
        private readonly IConfiguration config;
        private readonly IBrowsingContext context;

        private readonly IDeletableEntityRepository<Category> categoryRepository;
        private readonly IDeletableEntityRepository<Ingredient> ingredientRepository;
        private readonly IRepository<RecipeIngredient> recipeIngredientRepository;
        private readonly IRepository<RecipeCategory> recipeCategoryRepository;
        private readonly IDeletableEntityRepository<Recipe> recipeRepository;
        private readonly IRepository<Image> imageRepository;

        public SupichkaScraperService(
            IDeletableEntityRepository<Category> categoryRepository,
            IDeletableEntityRepository<Ingredient> ingredientRepository,
            IRepository<RecipeIngredient> recipeIngredientRepository,
            IRepository<RecipeCategory> recipeCategoryRepository,
            IDeletableEntityRepository<Recipe> recipeRepository,
            IRepository<Image> imageRepository)
        {
            this.config = Configuration.Default.WithDefaultLoader();
            this.context = BrowsingContext.New(this.config);

            this.categoryRepository = categoryRepository;
            this.ingredientRepository = ingredientRepository;
            this.recipeIngredientRepository = recipeIngredientRepository;
            this.recipeCategoryRepository = recipeCategoryRepository;
            this.recipeRepository = recipeRepository;
            this.imageRepository = imageRepository;
        }

        public async Task DbRecipeSeeder()
        {
            var recipesBag = new ConcurrentBag<RecipeDto>();

            Parallel.For(1, 1000, (i) =>
            {
                try
                {
                    var recipe = this.GetRecipe(i);
                    recipesBag.Add(recipe);
                }
                catch
                {
                }
            });

            foreach (var item in recipesBag)
            {
                var recipe = new Recipe
                {
                    Name = item.Name,
                    Time = item.Time,
                    PortionsCount = item.PortionsCount,
                    Instructions = item.Instructions,
                    OriginalUrl = item.OriginalUrl,
                };

                await this.recipeRepository.AddAsync(recipe);
                await this.recipeRepository.SaveChangesAsync();

                foreach (var ingredient in item.Ingredients)
                {
                    var ingredientId = await this.GetOrCreateIngredientAsync(ingredient.Key);

                    var recipeIngredient = new RecipeIngredient
                    {
                        IngredientId = ingredientId,
                        RecipeId = recipe.Id,
                        Quantity = ingredient.Value,
                    };

                    await this.recipeIngredientRepository.AddAsync(recipeIngredient);
                    await this.recipeIngredientRepository.SaveChangesAsync();
                }

                foreach (var category in item.Categories)
                {
                    var categoryId = await this.GetOrCreateCategoryAsync(category);

                    var recipeCategory = new RecipeCategory
                    {
                        CategoryId = categoryId,
                        RecipeId = recipe.Id,
                    };

                    await this.recipeCategoryRepository.AddAsync(recipeCategory);
                    await this.recipeCategoryRepository.SaveChangesAsync();
                }

                var image = new Image
                {
                    RemoteImageUrl = item.RemoteImageUrl,
                    RecipeId = recipe.Id,
                };

                await this.imageRepository.AddAsync(image);
                await this.imageRepository.SaveChangesAsync();
            }
        }

        private async Task<int> GetOrCreateIngredientAsync(string ingredientName)
        {
            var ingredient = this.ingredientRepository.AllAsNoTracking().FirstOrDefault(x => x.Name == ingredientName);

            if (ingredient == null)
            {
                ingredient = new Ingredient()
                {
                    Name = ingredientName,
                };

                await this.ingredientRepository.AddAsync(ingredient);
                await this.ingredientRepository.SaveChangesAsync();
            }

            return ingredient.Id;
        }

        private async Task<int> GetOrCreateCategoryAsync(string categoryName)
        {
            var category = this.categoryRepository.AllAsNoTracking().FirstOrDefault(x => x.Name == categoryName);

            if (category == null)
            {
                category = new Category()
                {
                    Name = categoryName,
                };

                await this.categoryRepository.AddAsync(category);
                await this.categoryRepository.SaveChangesAsync();
            }

            return category.Id;
        }

        private RecipeDto GetRecipe(int id)
        {
            var document = this.context.OpenAsync($"https://www.supichka.com/%D1%80%D0%B5%D1%86%D0%B5%D0%BF%D1%82%D0%B0/{id}")
                .GetAwaiter()
                .GetResult();

            if (document.StatusCode == HttpStatusCode.NotFound ||
                document.QuerySelector("[property='datePublished']") == null)
            {
                throw new InvalidOperationException();
            }

            var recipe = new RecipeDto();

            // Get recipe name
            recipe.Name = document.QuerySelector(".page__title").TextContent.Trim();

            // Get recipe time
            var time = double.Parse(document.QuerySelector("[property='totalTime']").TextContent.Trim());
            recipe.Time = TimeSpan.FromMinutes(time);

            // Get recipe portions count
            recipe.PortionsCount = document.QuerySelector("[property='recipeYield']").TextContent.Trim();

            // Get recipe categories
            var categories = document.QuerySelectorAll(".tabs__wrapper > li > a")
                .Select(x => x.TextContent.Trim())
                .ToList();

            foreach (var category in categories)
            {
                recipe.Categories.Add(category);
            }

            // Get recipe instructions
            var instructions = new StringBuilder();

            var preview = document.QuerySelector(".description__text > p").TextContent.Trim();
            instructions.AppendLine(preview);

            var steps = document.QuerySelectorAll(".description__text > ol > li")
                .Select(x => x.TextContent)
                .ToList();

            for (int i = 1; i <= steps.Count; i++)
            {
                instructions.AppendLine();
                instructions.AppendLine($"{i}. {steps[i - 1]}");
            }

            recipe.Instructions = instructions.ToString().TrimEnd();

            // Get recipe ingredients
            var elements = document.QuerySelectorAll(".box__block > ul > li:not([class])");

            foreach (var element in elements)
            {
                var ingredient = element.QuerySelector(".ingredient").TextContent;
                var quantity = element.QuerySelector(".qt").TextContent;

                recipe.Ingredients[ingredient] = quantity;
            }

            // Get recipe image url
            var relativeUrl = document.QuerySelector("[property='image']").GetAttribute("src");
            recipe.RemoteImageUrl = $"https://supichka.com{relativeUrl}";

            // Get original url
            recipe.OriginalUrl = $"https://www.supichka.com/%D1%80%D0%B5%D1%86%D0%B5%D0%BF%D1%82%D0%B0/{id}";

            return recipe;
        }
    }
}
