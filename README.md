# 🗄️ Branch: 01-repository-di
## Repository Pattern + Dependency Injection + Service Layer

---

## 🤔 What Problem Does It Solve?

Without this pattern, controllers talk directly to the database:

```csharp
// ❌ BAD — controller knows SQL, connection strings, everything
public IActionResult GetProducts()
{
    var conn = new SqlConnection("Server=...");
    var cmd = new SqlCommand("SELECT * FROM Products", conn);
    // messy, untestable, impossible to maintain
}
```

**Problems:**
- Controller has too many responsibilities
- Can't unit test without a real database
- Switching databases = rewriting every controller
- Business logic mixed with data access

---

## ✅ The Solution

Split responsibilities into 3 clean layers:

```
Controller   → handles HTTP only
Service      → owns business rules + validation
Repository   → owns database operations only
```

---

## 🔄 Data Flow

```
HTTP Request (POST /api/products)
        ↓
ProductsController
  → receives JSON, calls service
        ↓
ProductService
  → validates: name required, price > 0, no duplicates
  → calls repository
        ↓
ProductRepository
  → EF Core → MS SQL
  → INSERT INTO Products...
        ↓
Response: 201 Created + product JSON
```

---

## 📁 Files Added This Branch

```
Models/
└── Product.cs                  ← data model → maps to Products table

Data/
└── AppDbContext.cs             ← EF Core bridge to MS SQL

Repositories/
├── IProductRepository.cs       ← interface (the contract)
└── ProductRepository.cs        ← EF Core implementation

Services/
├── IProductService.cs          ← interface (the contract)
└── ProductService.cs           ← business logic + validation

Exceptions/
├── NotFoundException.cs        ← 404 — resource not found
├── ValidationException.cs      ← 400 — business rule violated
└── DuplicateException.cs       ← 409 — duplicate data

Controllers/
└── ProductsController.cs       ← HTTP layer only

Program.cs                      ← DI container wiring
appsettings.json                ← connection string
```

---

## 🧠 Key Concepts

### Repository Pattern
```
IProductRepository  ← the CONTRACT (what operations exist)
ProductRepository   ← the WORKER   (how EF Core does each operation)

// Caller depends on interface — never the concrete class!
private readonly IProductRepository _repo;  // ✅
private readonly ProductRepository _repo;   // ❌ tightly coupled
```

### Dependency Injection
```csharp
// Program.cs registers the mapping:
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// .NET automatically creates and injects:
public ProductService(IProductRepository repo)  // handed in! ✅
{
    _repo = repo;
}
// Never write: new ProductRepository() ❌
```

### Service Layer
```
Repository   → "Get me this data from the DB"       (no logic)
Service      → "Here are the rules, then save it"   (business logic)
Controller   → "Here's the HTTP request, handle it" (HTTP only)
```

### Custom Exceptions → HTTP Status Codes
```
NotFoundException   → 404 Not Found
ValidationException → 400 Bad Request
DuplicateException  → 409 Conflict
```

---

## 💡 EF Core Highlights

### AsNoTracking() — faster reads
```csharp
// EF Core tracks objects by default (watches for changes)
// For READ-ONLY queries this wastes memory
return await _db.Products.AsNoTracking().ToListAsync();  // ✅ faster!
```

### SaveChangesAsync() — commits to SQL
```csharp
_db.Products.AddAsync(product);  // staged in memory
await _db.SaveChangesAsync();    // NOW it hits MS SQL ← actual INSERT
```

### UpdateAsync — find then update (safe)
```csharp
// ✅ Only updates fields that changed
var existing = await _db.Products.FindAsync(id);
existing.Name  = updated.Name;
existing.Price = updated.Price;
// CreatedAt intentionally NOT updated
await _db.SaveChangesAsync();
```

---

## 🧪 Test Endpoints

```json
// GET all products
GET /api/products → 200 OK

// GET by id
GET /api/products/1 → 200 OK
GET /api/products/999 → 404 "Product with ID 999 was not found"

// POST — add product
POST /api/products
{ "name": "Wireless Headphones", "price": 1500.00, "stock": 25 }
→ 201 Created

// POST — validation errors
{ "name": "", "price": 1500, "stock": 10 }
→ 400 "Product name is required"

{ "name": "Wireless Headphones", "price": 1500, "stock": 10 }
→ 409 "A product named 'Wireless Headphones' already exists"

// PUT — update
PUT /api/products/1
{ "id": 1, "name": "Updated Name", "price": 1800, "stock": 20 }
→ 204 No Content

// DELETE
DELETE /api/products/1 → 204 No Content
DELETE /api/products/999 → 404 Not Found
```

---

## 🔑 DI Lifetime — Why AddScoped?

```
AddScoped    → ONE instance per HTTP request  ✅ use for repos + services
AddSingleton → ONE instance for entire app    → use for config, cache
AddTransient → NEW instance every injection   → rarely needed
```

---

## 📋 MS SQL Table Created

```sql
CREATE TABLE Products (
    Id          INT IDENTITY PRIMARY KEY,  -- auto-increments
    Name        NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),             -- nullable (optional)
    Price       DECIMAL(18,2) NOT NULL,
    Stock       INT NOT NULL,
    CreatedAt   DATETIME2 NOT NULL
)
```
> Auto-generated by EF Core migrations — never written manually!

---

## ✅ Key Takeaways

- Always code against **interfaces**, never concrete classes
- **Repository** = data access only, zero business logic
- **Service** = business rules only, zero SQL knowledge
- **Controller** = HTTP only, delegates everything else
- **DI Container** wires everything — you never write `new Repository()`
- **Custom exceptions** give clear, specific error messages
