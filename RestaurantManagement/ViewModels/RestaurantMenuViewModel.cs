using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Models;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class RestaurantMenuViewModel : BaseViewModel
{
    private readonly MenuService _menuService;
    private string _searchQuery = "";
    private bool _searchByAllergen;
    private bool _excludeSearchTerm;
    private bool _isSearching;
    private bool _hasNoResults;
    private string _noResultsMessage = "";

    public ObservableCollection<CategoryViewModel> Categories { get; } = new();
    public ObservableCollection<CategoryViewModel> SearchResultCategories { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public bool SearchByAllergen
    {
        get => _searchByAllergen;
        set => SetProperty(ref _searchByAllergen, value);
    }

    public bool ExcludeSearchTerm
    {
        get => _excludeSearchTerm;
        set => SetProperty(ref _excludeSearchTerm, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }
    
    public bool HasNoResults
    {
        get => _hasNoResults;
        set => SetProperty(ref _hasNoResults, value);
    }
    
    public string NoResultsMessage
    {
        get => _noResultsMessage;
        set => SetProperty(ref _noResultsMessage, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand LoadMenuCommand { get; }

    public RestaurantMenuViewModel()
    {
        _menuService = new MenuService();

        SearchCommand = new RelayCommand(_ => {
            _ = SearchAsync();
        });
        ClearSearchCommand = new RelayCommand(_ => ClearSearch());
        LoadMenuCommand = new RelayCommand(_ => {
            _ = LoadMenuAsync();
        });
    }

    public async Task LoadMenuAsync()
    {
        try
        {
            var categories = await _menuService.GetCategoriesWithItemsAsync();
            Categories.Clear();
            
            foreach (var category in categories)
            {
                var categoryVm = new CategoryViewModel(category.Name);
                
                // Add dishes from this category
                foreach (var dish in category.Dishes ?? Enumerable.Empty<Dish>())
                {
                    categoryVm.Items.Add(new MenuItemViewModel(dish));
                }
                
                // Add menus from this category
                foreach (var menu in category.Menus ?? Enumerable.Empty<Menu>())
                {
                    categoryVm.Items.Add(new MenuItemViewModel(menu));
                }
                
                if (categoryVm.Items.Count > 0)
                {
                    Categories.Add(categoryVm);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle errors
            System.Diagnostics.Debug.WriteLine($"Error loading menu: {ex.Message}");
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsSearching = true;
        HasNoResults = false;
        SearchResultCategories.Clear();

        try
        {
            // Dictionary to store items grouped by category
            var groupedResults = new Dictionary<string, List<MenuItemViewModel>>();
            
            if (SearchByAllergen)
            {
                // Search by allergen
                var (dishes, menus) = await _menuService.SearchByAllergenAsync(SearchQuery, ExcludeSearchTerm);
                
                // Process dishes
                foreach (var dish in dishes)
                {
                    var menuItem = new MenuItemViewModel(dish);
                    AddToGroupedResults(groupedResults, menuItem);
                }
                
                // Process menus
                foreach (var menu in menus)
                {
                    var menuItem = new MenuItemViewModel(menu);
                    AddToGroupedResults(groupedResults, menuItem);
                }
            }
            else
            {
                // Search by name
                var dishes = await _menuService.SearchDishesByNameAsync(SearchQuery);
                var menus = await _menuService.SearchMenusByNameAsync(SearchQuery);
                
                // Process dishes
                foreach (var dish in dishes)
                {
                    var menuItem = new MenuItemViewModel(dish);
                    AddToGroupedResults(groupedResults, menuItem);
                }
                
                // Process menus
                foreach (var menu in menus)
                {
                    var menuItem = new MenuItemViewModel(menu);
                    AddToGroupedResults(groupedResults, menuItem);
                }
            }
            
            // Convert grouped results to CategoryViewModel objects
            foreach (var category in groupedResults.OrderBy(g => g.Key))
            {
                var categoryVm = new CategoryViewModel(category.Key);
                foreach (var item in category.Value)
                {
                    categoryVm.Items.Add(item);
                }
                SearchResultCategories.Add(categoryVm);
            }
            
            // Check if any results were found
            if (SearchResultCategories.Count == 0)
            {
                HasNoResults = true;
                if (SearchByAllergen)
                {
                    if (ExcludeSearchTerm)
                    {
                        NoResultsMessage = $"Nu s-au găsit preparate/meniuri care nu conțin alergenul \"{SearchQuery}\".";
                    }
                    else
                    {
                        NoResultsMessage = $"Nu s-au găsit preparate/meniuri care conțin alergenul \"{SearchQuery}\".";
                    }
                }
                else
                {
                    NoResultsMessage = $"Nu s-au găsit preparate/meniuri care conțin \"{SearchQuery}\" în denumire.";
                }
            }
        }
        catch (Exception ex)
        {
            // Handle search errors
            System.Diagnostics.Debug.WriteLine($"Error performing search: {ex.Message}");
            HasNoResults = true;
            NoResultsMessage = "A apărut o eroare în timpul căutării. Încercați din nou.";
        }
    }
    
    private void AddToGroupedResults(Dictionary<string, List<MenuItemViewModel>> groupedResults, MenuItemViewModel menuItem)
    {
        if (!groupedResults.ContainsKey(menuItem.CategoryName))
        {
            groupedResults[menuItem.CategoryName] = new List<MenuItemViewModel>();
        }
        groupedResults[menuItem.CategoryName].Add(menuItem);
    }

    private void ClearSearch()
    {
        SearchQuery = "";
        SearchResultCategories.Clear();
        IsSearching = false;
        HasNoResults = false;
    }
}

public partial class CategoryViewModel : BaseViewModel
{
    // Properties
    public int Id { get; protected set; }
    public string Name { get; set; }
    public ObservableCollection<MenuItemViewModel> Items { get; } = new();

    // Constructors
    public CategoryViewModel(string name)
    {
        Id = 0;
        Name = name;
    }
    
    public CategoryViewModel(Category category)
    {
        Id = category.CategoryID;
        Name = category.Name ?? "";
        
        // Add dishes from this category if available
        if (category.Dishes != null)
        {
            foreach (var dish in category.Dishes)
            {
                Items.Add(new MenuItemViewModel(dish));
            }
        }
        
        // Add menus from this category if available
        if (category.Menus != null)
        {
            foreach (var menu in category.Menus)
            {
                Items.Add(new MenuItemViewModel(menu));
            }
        }
    }
    
    public CategoryViewModel()
    {
        Id = 0;
        Name = "";
    }
} 