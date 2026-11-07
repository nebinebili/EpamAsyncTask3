using System.Collections.ObjectModel;

[assembly: CLSCompliant(false)]

namespace ProductRepositoryAsync.Tests;

internal sealed class Database : IDatabase
{
    private readonly object lockObject = new();
    private readonly Dictionary<string, Collection> database = new();

    public bool IsCollectionExistAsyncFailure { get; init; }

    public bool IsCollectionElementExistFailure { get; init; }

    public bool CreateCollectionAsyncFailure { get; init; }

    public bool GenerateIdAsyncFailure { get; init; }

    public bool GetCollectionElementAsyncFailure { get; init; }

    public bool InsertCollectionElementAsyncFailure { get; init; }

    public bool DeleteCollectionElementAsyncFailure { get; init; }

    public bool UpdateCollectionElementAsyncFailure { get; init; }

    public bool ReturnConnectionIssue { get; init; }

    public Task<OperationResult> IsCollectionExistAsync(string collectionName, out bool collectionExists)
    {
        collectionExists = false;

        switch (this.IsCollectionExistAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (this.HasCollection(collectionName))
            {
                collectionExists = true;
            }
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> IsCollectionElementExistAsync(
        string collectionName,
        int id,
        out bool collectionElementExists)
    {
        collectionElementExists = false;

        switch (this.IsCollectionElementExistFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.HasCollection(collectionName))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            collectionElementExists = this.HasCollectionElement(collectionName, id);
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> CreateCollectionAsync(string collectionName)
    {
        switch (this.CreateCollectionAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (this.HasCollection(collectionName))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            this.database.Add(collectionName, new Collection(collectionName));

            return Task.FromResult(OperationResult.Success);
        }
    }

    public Task<OperationResult> GenerateIdAsync(string collectionName, out int id)
    {
        id = 0;

        switch (this.GenerateIdAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.database.TryGetValue(collectionName, out var collection))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            id = collection.GenerateElementId();
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> GetCollectionElementAsync(
        string collectionName,
        int id,
        out IDictionary<string, string> data)
    {
        data = null!;

        switch (this.GetCollectionElementAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.database.TryGetValue(collectionName, out Collection? collection))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            if (!collection.Elements.TryGetValue(id, out CollectionElement? collectionElement))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            data = new ReadOnlyDictionary<string, string>(collectionElement.Properties);
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> InsertCollectionElementAsync(
        string collectionName,
        int id,
        IDictionary<string, string> data)
    {
        switch (this.InsertCollectionElementAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.database.TryGetValue(collectionName, out Collection? collection))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            if (collection.Elements.ContainsKey(id))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            CollectionElement element = new CollectionElement(id);

            foreach (KeyValuePair<string, string> pair in data)
            {
                element.Properties.Add(pair.Key, pair.Value);
            }

            collection.Elements.Add(id, element);
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> DeleteCollectionElementAsync(string collectionName, int id)
    {
        switch (this.DeleteCollectionElementAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.database.TryGetValue(collectionName, out Collection? collection))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            if (!collection.Elements.ContainsKey(id))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            _ = collection.Elements.Remove(id);
        }

        return Task.FromResult(OperationResult.Success);
    }

    public Task<OperationResult> UpdateCollectionElementAsync(
        string collectionName,
        int id,
        IDictionary<string, string> data)
    {
        switch (this.UpdateCollectionElementAsyncFailure)
        {
            case true when this.ReturnConnectionIssue:
                return Task.FromResult(OperationResult.ConnectionIssue);
            case true:
                return Task.FromResult(OperationResult.Failure);
            default:
                break;
        }

        if (!IsValidCollectionName(collectionName))
        {
            return Task.FromResult(OperationResult.InvalidCollectionName);
        }

        lock (this.lockObject)
        {
            if (!this.database.TryGetValue(collectionName, out Collection? collection))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            if (!collection.Elements.TryGetValue(id, out CollectionElement? collectionElement))
            {
                return Task.FromResult(OperationResult.Failure);
            }

            collectionElement.Properties.Clear();

            foreach (KeyValuePair<string, string> pair in data)
            {
                collectionElement.Properties.Add(pair.Key, pair.Value);
            }
        }

        return Task.FromResult(OperationResult.Success);
    }

    public bool HasCollection(string collectionName)
    {
        return this.database.TryGetValue(collectionName, out _);
    }

    public bool HasCollectionElement(string collectionName, int id)
    {
        return this.database.TryGetValue(collectionName, out Collection? collection) && collection.Elements.TryGetValue(id, out _);
    }

    private static bool IsValidCollectionName(string collectionName)
    {
        return !string.IsNullOrWhiteSpace(collectionName) && !(
            collectionName.StartsWith(' ') ||
            collectionName.EndsWith(' ') ||
            collectionName.Length < 5);
    }

    private class Collection(string collectionName)
    {
        private int nextId = 1;

        public string Name { get; private set; } = collectionName;

        public IDictionary<int, CollectionElement> Elements { get; } = new Dictionary<int, CollectionElement>();

        public int GenerateElementId()
        {
            return this.nextId++;
        }
    }

    private class CollectionElement(int id)
    {
        public int Id { get; private set; } = id;

        public Dictionary<string, string> Properties { get; } = new();
    }
}
