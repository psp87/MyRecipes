namespace MyRecipes.Services.Data
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public interface ICategoriesService
    {
        IEnumerable<KeyValuePair<string, string>> GetAllCategories();
    }
}
