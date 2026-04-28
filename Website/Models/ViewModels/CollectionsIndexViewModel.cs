using Website.Models;

namespace Website.Models.ViewModels;

public class CollectionsIndexViewModel
{
    public ICollection<Collection> OwnedCollections { get; set; } = [];
}