namespace MyRecipes.Services
{
    using System.Threading.Tasks;

    public interface ISupichkaScraperService
    {
        Task DbRecipeSeeder();
    }
}
