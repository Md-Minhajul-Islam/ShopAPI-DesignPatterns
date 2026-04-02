# ☝️ Branch: 04-singleton
## Singleton Pattern — App Configuration Service

---

## 🤔 What Problem Does It Solve?

Without Singleton, config objects get created repeatedly:

```csharp
// ❌ BAD — new config object every request!
public class OrderService
{
    public async Task PlaceOrderAsync(...)
    {
        var config = new AppConfiguration();  // reads file EVERY request!
        var max = config.MaxOrderQuantity;
    }
}

public class ProductService
{
    public async Task AddProductAsync(...)
    {
        var config = new AppConfiguration();  // another new instance! 😬
    }
}
```

**Problems:**
- Config file read from disk on every request — wasteful!
- Multiple instances = potentially inconsistent values
- Magic numbers scattered throughout codebase

---

## ✅ The Solution

> Ensure a class has only **ONE instance** for the entire app lifetime, with a global point of access.

```
First request:
AppConfigService.Instance → creates ONE object → stored in memory

Every request after:
AppConfigService.Instance → returns SAME object → no new creation ✅

ProductService  ──────────┐
                          ▼
OrderService  ──────────▶ ONE AppConfigService instance
                          ▲
ConfigController ─────────┘
```

---

## 🔄 Data Flow

```
App starts
    ↓
Program.cs: builder.Services.AddSingleton<IAppConfigService, AppConfigService>()
    ↓
.NET creates ONE AppConfigService instance
Console: "✅ AppConfigService created — App: ShopAPI v1.0.0"  ← prints ONCE!
    ↓
─────────────────────────────────────
Request 1: POST /api/products
    ↓
ProductService receives IAppConfigService (SAME instance)
  → _config.IsProductPriceValid(999999)
  → false → throw ValidationException "Price must be between ৳1 and ৳100,000"
─────────────────────────────────────
Request 2: POST /api/orders { quantity: 99 }
    ↓
OrderService receives IAppConfigService (SAME instance)
  → _config.IsOrderQuantityValid(99)
  → false → throw ValidationException "Quantity must be between 1 and 50"
─────────────────────────────────────
Request 3: GET /api/config
    ↓
ConfigController receives IAppConfigService (SAME instance)
  → _config.GetConfiguration()
  → returns AppConfiguration object ✅
```

---

## 📁 Files Added This Branch

```
Singleton/
├── AppConfiguration.cs     ← settings model (not a DB entity!)
├── IAppConfigService.cs    ← contract with helper methods
└── AppConfigService.cs     ← Singleton implementation

Controllers/
└── ConfigController.cs     ← GET /api/config endpoint

Extensions/
└── SingletonExtensions.cs  ← AddAppConfiguration() registration
```

---

## 🧠 Key Concepts

### Way 1 — Classic Singleton (manual)
```csharp
public class AppConfigService : IAppConfigService
{
    // THE instance — stored as static field
    private static AppConfigService? _instance;
    private static readonly object _lock = new object();  // thread safety!

    // private constructor — nobody can write "new AppConfigService()"
    private AppConfigService() { }

    public static AppConfigService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)  // only ONE thread enters at a time
                {
                    _instance ??= new AppConfigService();
                }
            }
            return _instance;  // always the SAME object ✅
        }
    }
}

// Usage anywhere in app:
var config = AppConfigService.Instance;  // always same object!
```

### Way 2 — DI Singleton (preferred in .NET)
```csharp
// Program.cs — .NET manages the single instance
builder.Services.AddSingleton<IAppConfigService, AppConfigService>();
//               ↑
// AddSingleton = ONE instance for entire app lifetime
// vs AddScoped  = ONE per HTTP request
// vs AddTransient = NEW every injection
```

### Helper Methods
```csharp
public interface IAppConfigService
{
    AppConfiguration Config { get; }
    AppConfiguration GetConfiguration();          // return full config
    bool IsOrderQuantityValid(int quantity);       // 1 to 50
    bool IsProductPriceValid(decimal price);       // ৳1 to ৳100,000
    bool IsLowStock(int stock);                    // stock <= 5
    decimal GetCouponDiscountAmount();             // 150tk
}
```

### Proof It's Singleton
```
App starts:
"✅ AppConfigService created — App: ShopAPI v1.0.0"  ← prints ONCE

100 requests later:
Never prints again → same instance reused every time ✅
```

---

## 🆚 Where Each Setting Is Used

```
MaxOrderQuantity (50)      → OrderService validates quantity
MaxProductPrice (100,000)  → ProductService validates price
MinProductPrice (1)        → ProductService validates price
CouponDiscountAmount (150) → CouponDiscountStrategy gets amount
LowStockThreshold (5)      → OrderService warns when stock drops
```

---

## 🧪 Test Endpoints

```json
// GET config — see all settings
GET /api/config
→ {
    "appName": "ShopAPI",
    "appVersion": "1.0.0",
    "maxOrderQuantity": 50,
    "maxProductPrice": 100000,
    "minProductPrice": 1,
    "couponDiscountAmount": 150,
    "lowStockThreshold": 5
  }

// Test max quantity validation
POST /api/orders
{ "productId": 1, "quantity": 99, "paymentMethod": 1 }
→ 400 "Quantity must be between 1 and 50" ✅

// Test price validation
POST /api/products
{ "name": "Super Laptop", "price": 999999, "stock": 5 }
→ 400 "Price must be between ৳1.00 and ৳100,000.00" ✅

// Test low stock warning (check terminal)
POST /api/orders { "quantity": enough to drop stock below 5 }
→ Terminal: "⚠️ Low stock warning: ProductName has 3 left!"
```

---

## ✅ Key Takeaways

- Singleton = **one instance** shared across entire app lifetime
- `lock(_lock)` = **thread safe** — prevents race condition on first creation
- Private constructor = **nobody can write** `new AppConfigService()`
- `AddSingleton()` = **.NET manages** the single instance (preferred)
- Config lives in `Singleton/` not `Models/` — it's **not a DB entity**!
- Centralizes magic numbers — change `MaxOrderQuantity` in ONE place ✅
