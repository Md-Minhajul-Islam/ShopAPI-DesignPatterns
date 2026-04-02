# 🎂 Branch: 06-decorator
## Decorator Pattern — Logging Product Repository

---

## 🤔 What Problem Does It Solve?

Without Decorator, logging is mixed into the repository:

```csharp
// ❌ BAD — repository has TWO jobs: data access + logging
public async Task<IEnumerable<Product>> GetAllAsync()
{
    Console.WriteLine("Getting products...");   // logging
    var sw = Stopwatch.StartNew();              // timing

    var result = await _db.Products.ToListAsync(); // actual work

    Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms"); // logging
    return result;
    // repeat ALL of this for every method 😬
}
```

**Problems:**
- Repository violates Single Responsibility Principle
- Logging copy-pasted in every method
- Removing logging = editing every method
- Mixed concerns make code hard to read

---

## ✅ The Solution

> **Wrap** an existing object with a new object that **adds behaviour** — without changing the original!

```
Before Decorator:
ProductService → ProductRepository → MS SQL

After Decorator:
ProductService → LoggingProductRepository → ProductRepository → MS SQL
                        ↑
               adds logging + timing
               same IProductRepository interface ✅
               ProductRepository unchanged ✅
               ProductService unchanged ✅
```

---

## 🔄 Data Flow

```
GET /api/products
        ↓
ProductsController
        ↓
ProductService._repo.GetAllAsync()
  → _repo is LoggingProductRepository (injected by DI!)
        ↓
LoggingProductRepository.GetAllAsync()
  → log: "→ [DB] Fetching all products..."
  → start stopwatch
  → call _inner.GetAllAsync()  ← delegates to real repo!
        ↓
ProductRepository.GetAllAsync()  ← actual EF Core call
  → SELECT * FROM Products (AsNoTracking)
        ↓
LoggingProductRepository receives result
  → stop stopwatch
  → log: "← [DB] Fetched 4 products in 12ms"
  → return result
        ↓
ProductService receives products
(never knew about logging!) ✅
        ↓
Response: 200 OK [products array]
```

---

## 📁 Files Added This Branch

```
Decorator/
└── LoggingProductRepository.cs   ← THE DECORATOR ⭐
                                     wraps ProductRepository
                                     adds logging + timing

Extensions/
└── RepositoryExtensions.cs       ← updated to wire decorator in DI
```

> **Zero new files in Models, Services, Controllers!**
> **Zero changes to ProductRepository, ProductService, ProductsController!**

---

## 🧠 Key Concepts

### Decorator Structure
```csharp
// Implements SAME interface as the class it wraps!
public class LoggingProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;  // the REAL repo inside
    private readonly ILogger<LoggingProductRepository> _logger;

    // inject the real repo into the decorator
    public LoggingProductRepository(
        IProductRepository inner,
        ILogger<LoggingProductRepository> logger)
    {
        _inner  = inner;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("→ [DB] Fetching all products...");

        try
        {
            var result = await _inner.GetAllAsync();  // delegate to real repo ✅
            sw.Stop();
            _logger.LogInformation("← [DB] Fetched {Count} products in {Ms}ms",
                result.Count(), sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError("✗ [DB] GetAllAsync failed: {Error}", ex.Message);
            throw;  // re-throw — don't swallow!
        }
    }
}
```

### Wiring in DI — The Key Step
```csharp
// RepositoryExtensions.cs
// Step 1: Register the REAL repository (concrete class)
services.AddScoped<ProductRepository>();

// Step 2: Register interface → Decorator wrapping real repo
services.AddScoped<IProductRepository>(provider =>
    new LoggingProductRepository(
        provider.GetRequiredService<ProductRepository>(),   // inner = real repo
        provider.GetRequiredService<ILogger<LoggingProductRepository>>()
    ));

// Result:
// Anyone asking for IProductRepository gets LoggingProductRepository
// which has ProductRepository inside it ✅
```

### Why Re-throw?
```csharp
catch (Exception ex)
{
    _logger.LogError("Failed: {Error}", ex.Message);
    throw;  // ← re-throw the ORIGINAL exception
    //  ↑
    // Decorator logs the error AND lets it bubble up
    // Caller still handles it properly — we don't hide it!
}
```

### ILogger vs Console.WriteLine
```csharp
// ❌ Console.WriteLine — no levels, no filtering
Console.WriteLine("Getting products");

// ✅ ILogger — structured, filterable, production-ready
_logger.LogInformation("Fetching {Count} products", count);  // Info level
_logger.LogWarning("Product {Id} not found", id);            // Warning level
_logger.LogError("DB failed: {Error}", ex.Message);          // Error level
// Logs can be filtered, saved to files, sent to cloud tools!
```

---

## 🆚 Decorator vs Proxy

```
Decorator → ADDS behaviour to same interface (e.g. adds logging)
Proxy     → CONTROLS ACCESS to same interface (e.g. auth check)
Both      → wrap an object with same interface
```

---

## 🧱 Stacking Decorators — Like Cake Layers! 🎂

```csharp
// Tomorrow — add caching on top of logging:
IProductRepository
  → CachingProductRepository      ← outermost layer
      → LoggingProductRepository  ← middle layer
          → ProductRepository     ← core (real work)

// Change just ONE line in RepositoryExtensions:
services.AddScoped<IProductRepository>(provider =>
    new CachingProductRepository(
        new LoggingProductRepository(
            provider.GetRequiredService<ProductRepository>(),
            provider.GetRequiredService<ILogger<...>>()
        ),
        provider.GetRequiredService<IMemoryCache>()
    ));
```

---

## 🧪 What You See In Terminal

```bash
GET /api/products:
info: → [DB] Fetching all products...
info: ← [DB] Fetched 4 products in 12ms ✅

GET /api/products/999:
info: → [DB] Fetching product ID: 999...
warn: ← [DB] Product ID: 999 NOT FOUND (3ms) ⚠️

POST /api/products (new product):
info: → [DB] Adding product 'New Laptop'...
info: ← [DB] Product 'New Laptop' added with ID: 5 in 8ms ✅

DELETE /api/products/1:
info: → [DB] Deleting product ID: 1...
info: ← [DB] Product ID: 1 deleted in 5ms ✅
```

---

## ✅ Key Takeaways

- Decorator = **same interface** in and out — caller never knows it's wrapped
- Inner object = **real implementation** — unchanged, unaware of wrapper
- DI wires it **transparently** — ProductService never changed
- `throw` (not `throw ex`) = re-throws **original exception** with full stack trace
- `ILogger` > `Console.WriteLine` — structured, filterable, production-ready
- Decorators **stack** like layers — add caching, retry, auth on top of logging
- **Zero changes** to ProductRepository, ProductService, or any Controller
