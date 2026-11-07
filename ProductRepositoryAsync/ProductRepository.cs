using System.Globalization;

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

    public async Task<Product> GetProductAsync(int productId)
    {
        OperationResult collectionExistResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (collectionExistResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionExistResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        OperationResult collectionElementExistResult = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

        if (collectionElementExistResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionElementExistResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionElementExists)
        {
            throw new ProductNotFoundException();
        }

        OperationResult getCollectionElementResult = await this.database.GetCollectionElementAsync(this.productCollectionName, productId, out IDictionary<string, string> data);

        if (getCollectionElementResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (getCollectionElementResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        return new Product
        {
            Id = productId,
            Category = data["category"],
            Name = data["name"],
            UnitPrice = decimal.Parse(data["price"], CultureInfo.InvariantCulture),
            UnitsInStock = int.Parse(data["in-stock"], CultureInfo.InvariantCulture),
            Discontinued = bool.Parse(data["discontinued"]),
        };
    }

    public Task<int> AddProductAsync(Product product)
    {
        ValidateProduct(product);

        return this.AddProductAsyncCore(product);
    }

    public async Task RemoveProductAsync(int productId)
    {
        OperationResult collectionExistResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (collectionExistResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionExistResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        OperationResult collectionElementExistResult = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

        if (collectionElementExistResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionElementExistResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionElementExists)
        {
            throw new ProductNotFoundException();
        }

        OperationResult deleteCollectionElementResult = await this.database.DeleteCollectionElementAsync(this.productCollectionName, productId);

        if (deleteCollectionElementResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (deleteCollectionElementResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }
    }

    public Task UpdateProductAsync(Product product)
    {
        ValidateProduct(product);

        return this.UpdateProductAsyncCore(product);
    }

    private static void ValidateProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        ArgumentException.ThrowIfNullOrWhiteSpace(product.Name, nameof(product));

        ArgumentException.ThrowIfNullOrWhiteSpace(product.Category, nameof(product));

        if (product.UnitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(product));
        }

        if (product.UnitsInStock < 0)
        {
            throw new ArgumentException("Units in stock cannot be negative.", nameof(product));
        }
    }

    private async Task UpdateProductAsyncCore(Product product)
    {
        OperationResult collectionResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (collectionResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        OperationResult productResult = await this.database.IsCollectionElementExistAsync(this.productCollectionName, product.Id, out bool productExists);

        if (productResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (productResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!productExists)
        {
            throw new ProductNotFoundException();
        }

        IDictionary<string, string> data = new Dictionary<string, string>
        {
            ["name"] = product.Name!,
            ["category"] = product.Category!,
            ["price"] = product.UnitPrice.ToString(CultureInfo.InvariantCulture),
            ["in-stock"] = product.UnitsInStock.ToString(CultureInfo.InvariantCulture),
            ["discontinued"] = product.Discontinued.ToString(CultureInfo.InvariantCulture),
        };

        OperationResult updateResult = await this.database.UpdateCollectionElementAsync(this.productCollectionName, product.Id, data);

        if (updateResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (updateResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }
    }

    private async Task<int> AddProductAsyncCore(Product product)
    {
        OperationResult collectionResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (collectionResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (collectionResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            OperationResult createResult = await this.database.CreateCollectionAsync(this.productCollectionName);

            if (createResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException();
            }

            if (createResult != OperationResult.Success)
            {
                throw new RepositoryException();
            }
        }

        OperationResult generateResult = await this.database.GenerateIdAsync(this.productCollectionName, out int id);

        if (generateResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (generateResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        IDictionary<string, string> data = new Dictionary<string, string>
        {
            ["name"] = product.Name!,
            ["category"] = product.Category!,
            ["price"] = product.UnitPrice.ToString(CultureInfo.InvariantCulture),
            ["in-stock"] = product.UnitsInStock.ToString(CultureInfo.InvariantCulture),
            ["discontinued"] = product.Discontinued.ToString(CultureInfo.InvariantCulture),
        };

        OperationResult insertResult = await this.database.InsertCollectionElementAsync(this.productCollectionName, id, data);

        if (insertResult == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        if (insertResult != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        return id;
    }
}
