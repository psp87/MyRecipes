namespace MyRecipes.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using MyRecipes.Data;
    using MyRecipes.Services;
    using MyRecipes.Web.ViewModels;
    using MyRecipes.Web.ViewModels.Home;

    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext db;
        private readonly ISupichkaScraperService scraperService;

        public HomeController(ApplicationDbContext db, ISupichkaScraperService scraperService)
        {
            this.db = db;
            this.scraperService = scraperService;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                CategoriesCount = this.db.Categories.Count(),
                ImagesCount = this.db.Images.Count(),
                IngredientsCount = this.db.Ingredients.Count(),
                RecipesCount = this.db.Recipes.Count(),
            };

            return this.View(viewModel);
        }

        public async Task<IActionResult> SeedRecipes()
        {
            await this.scraperService.DbRecipeSeeder();

            return this.RedirectToAction(nameof(this.Index));
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
