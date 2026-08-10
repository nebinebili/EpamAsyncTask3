namespace ProductRepositoryAsync;

/// <summary>
/// Represents a product storage service and provides a set of methods for managing the list of products.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly string productCollectionName;
    private readonly IDatabase database;

    public ProductRepository(string productCollectionName, IDatabase database)
    {
        this.productCollectionName = productCollectionName;
        this.database = database;
    }

    public Task<Product> GetProductAsync(int productId)
    {
        throw new NotImplementedException();
    }

    public Task<int> AddProductAsync(Product product)
    {
        throw new NotImplementedException();
    }

    public Task RemoveProductAsync(int productId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateProductAsync(Product product)
    {
        throw new NotImplementedException();
    }
}
