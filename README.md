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




# 🏭 Branch: 02-factory-pattern
## Factory Pattern — Payment Processors

---

## 🤔 What Problem Does It Solve?

Without Factory Pattern, the controller decides which class to create:

```csharp
// ❌ BAD — controller knows about every payment class
if (type == "Cash")        new CashPaymentProcessor();
else if (type == "Card")   new CardPaymentProcessor();
else if (type == "Bkash")  new BkashPaymentProcessor();
// add new payment? Edit this controller again 😬
```

**Problems:**
- Controller tightly coupled to every processor class
- Adding new payment type = editing existing code
- Violates Open/Closed Principle

---

## ✅ The Solution

> A Factory is a class whose only job is to **create the right object** based on input.

```
OrderService says → "I need a processor for bKash"
                          ↓
            PaymentProcessorFactory.Create(PaymentMethod.Bkash)
                          ↓
                  returns BkashPaymentProcessor
                          ↓
        OrderService calls .ProcessAsync() — doesn't know which class! ✅
```

---

## 🔄 Data Flow

```
POST /api/orders
{ "productId": 1, "quantity": 2, "paymentMethod": 3 }
        ↓
OrdersController
  → maps JSON → PlaceOrderRequest DTO
        ↓
OrderService.PlaceOrderAsync()
  → validates product exists
  → validates stock available
  → calculates total amount
  → _factory.Create(PaymentMethod.Bkash)  ← Factory Pattern ⭐
        ↓
PaymentProcessorFactory
  → switch(paymentMethod)
  → returns BkashPaymentProcessor
        ↓
BkashPaymentProcessor.ProcessAsync(order)
  → sets order.Status = Confirmed
  → sets order.PaymentNote = "bKash paid"
        ↓
OrderRepository.AddAsync(order)  → MS SQL
        ↓
Response: 201 Created + order JSON
```

---

## 📁 Files Added This Branch

```
Models/
└── Order.cs                        ← Order model + PaymentMethod enum
                                       + OrderStatus enum

Factory/
├── IPaymentProcessor.cs            ← contract all processors follow
├── CashPaymentProcessor.cs         ← handles cash logic
├── CardPaymentProcessor.cs         ← handles card logic
├── BkashPaymentProcessor.cs        ← handles bKash logic
├── NagadPaymentProcessor.cs        ← handles Nagad logic
└── PaymentProcessorFactory.cs      ← THE FACTORY ⭐

Repositories/
├── IOrderRepository.cs             ← order data contract
└── OrderRepository.cs              ← EF Core order operations

Services/
├── IOrderService.cs                ← order service contract
└── OrderService.cs                 ← order business logic

Controllers/
└── OrdersController.cs             ← order endpoints + PlaceOrderRequest DTO
```

---

## 🧠 Key Concepts

### The Factory Class
```csharp
public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    public IPaymentProcessor Create(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.Cash  => new CashPaymentProcessor(),
            PaymentMethod.Card  => new CardPaymentProcessor(),
            PaymentMethod.Bkash => new BkashPaymentProcessor(),
            PaymentMethod.Nagad => new NagadPaymentProcessor(),
            _ => throw new NotSupportedException(...)
        };
    }
}
// Adding RocketPay tomorrow?
// Step 1: Create RocketPaymentProcessor.cs
// Step 2: Add ONE line here ✅ — nothing else changes!
```

### Interface = Swap Without Breaking
```csharp
// OrderService only sees this — never the concrete class!
IPaymentProcessor processor = _factory.Create(paymentMethod);
processor.ProcessAsync(order);  // works for ALL payment types ✅
```

### Task.FromResult vs async
```csharp
// No await inside? → Task.FromResult (no async needed)
public Task<bool> ProcessAsync(Order order)
{
    order.Status = OrderStatus.Confirmed;
    return Task.FromResult(true);  // wraps value in completed Task
}

// Has await inside? → use async keyword
public async Task<bool> ProcessAsync(Order order)
{
    await _httpClient.PostAsync("bkash-api.com", ...);  // real async work
    return true;
}
```

### DTO — PlaceOrderRequest
```csharp
// Client only sends what THEY know:
public record PlaceOrderRequest(
    int ProductId,
    int Quantity,
    PaymentMethod PaymentMethod
);
// Server calculates: TotalAmount, Status, PaymentNote, CreatedAt
```

---

## 💡 Memory vs Database

```csharp
order.Status = OrderStatus.Confirmed;  // ← MEMORY ONLY!
order.PaymentNote = "bKash paid";      // ← MEMORY ONLY!

await _orderRepo.AddAsync(order);      // ← NOW hits MS SQL ✅
// Everything set in memory is saved in one INSERT
```

---

## 🧪 Test Endpoints

```json
// Cash payment (paymentMethod: 1)
POST /api/orders
{ "productId": 1, "quantity": 2, "paymentMethod": 1 }
→ 201 Created { "id": 1, "status": 2, "paymentNote": "Cash payment collected" }

// Card payment (paymentMethod: 2)
{ "productId": 1, "quantity": 1, "paymentMethod": 2 }
→ 201 Created

// bKash payment (paymentMethod: 3)
{ "productId": 2, "quantity": 3, "paymentMethod": 3 }
→ 201 Created

// Nagad payment (paymentMethod: 4)
{ "productId": 2, "quantity": 1, "paymentMethod": 4 }
→ 201 Created

// GET all orders
GET /api/orders → 200 OK [array of orders with products joined]

// GET order by id
GET /api/orders/1 → 200 OK
GET /api/orders/999 → 404 Not Found
```

---

## 🔑 PaymentMethod Enum Values

```
1 = Cash
2 = Card
3 = Bkash
4 = Nagad
```

---

## ✅ Key Takeaways

- Factory = **one place** that knows all implementations
- Caller only sees the **interface** — never concrete classes
- Adding new type = **one new file + one new line** in factory
- `Task.FromResult()` wraps a value in a completed Task (no async needed)
- DTO = only send what the client knows — server calculates the rest
- Objects modified in memory are saved **all at once** via SaveChangesAsync()



# 🎯 Branch: 03-strategy-pattern
## Strategy Pattern — Discount Algorithms

---

## 🤔 What Problem Does It Solve?

Without Strategy Pattern, one giant method handles all discount logic:

```csharp
// ❌ BAD — one method does everything
public decimal ApplyDiscount(Order order, string type)
{
    if (type == "Seasonal") return order.TotalAmount * 0.90m;
    else if (type == "Loyalty") return order.TotalAmount * 0.85m;
    else if (type == "Coupon") return order.TotalAmount - 100m;
    else return order.TotalAmount;
    // add new discount? Edit this method again 😬
}
```

**Problems:**
- One method doing too many things
- Hard to test each discount independently
- Adding new discount = editing existing code
- Violates Open/Closed Principle

---

## ✅ The Solution

> Define a family of algorithms, put each in its own class, and make them interchangeable.

```
OrderService says → "apply the right discount"
                          ↓
               IDiscountStrategy  ← the contract
            /      |       |      \
     Seasonal   Loyalty  Coupon  NoDiscount
     Strategy   Strategy Strategy  Strategy
                          ↓
              each knows its OWN calculation logic
```

---

## 🔄 Data Flow

```
POST /api/orders
{ "productId": 1, "quantity": 2, "paymentMethod": 1, "discountType": 1 }
        ↓
OrderService.PlaceOrderAsync()
  → calculates base total: 1500 × 2 = 3000tk
        ↓
  Strategy Pattern ⭐:
  switch(discountType)
  → SeasonalDiscountStrategy    (discountType: 1)
        ↓
  DiscountContext.SetStrategy(strategy)
  DiscountContext.ApplyDiscount(3000)
        ↓
  SeasonalDiscountStrategy.Calculate(3000)
  → 3000 - (3000 × 0.10) = 2700tk
        ↓
  order.TotalAmount      = 3000tk  (original)
  order.DiscountedAmount = 2700tk  (after discount)
  order.DiscountNote     = "Seasonal discount (10%) — saved ৳300"
        ↓
  Factory creates payment processor
  Processor charges 2700tk ✅
        ↓
Response: 201 Created
```

---

## 📁 Files Added This Branch

```
Models/
└── Order.cs              ← + DiscountType enum
                             + DiscountedAmount field
                             + DiscountNote field

Strategy/
├── IDiscountStrategy.cs          ← contract all strategies follow
├── NoDiscountStrategy.cs         ← returns original price
├── SeasonalDiscountStrategy.cs   ← 10% off
├── LoyaltyDiscountStrategy.cs    ← 15% off
├── CouponDiscountStrategy.cs     ← fixed amount off
└── DiscountContext.cs            ← holds + runs chosen strategy

Services/
└── OrderService.cs       ← updated: picks strategy, runs through context
Controllers/
└── OrdersController.cs   ← updated: DTO includes discountType + couponCode
```

---

## 🧠 Key Concepts

### Strategy Interface
```csharp
public interface IDiscountStrategy
{
    string DiscountType { get; }
    decimal Calculate(decimal amount);   // the algorithm
    string GetDescription(decimal original, decimal discounted);
}
```

### Context Class — The Bridge
```csharp
public class DiscountContext
{
    private IDiscountStrategy _strategy = new NoDiscountStrategy();

    // swap strategy at runtime!
    public void SetStrategy(IDiscountStrategy strategy)
        => _strategy = strategy;

    // runs whatever strategy is set — doesn't care which one!
    public decimal ApplyDiscount(decimal amount)
        => _strategy.Calculate(amount);
}
```

### Strategy Selection in OrderService
```csharp
IDiscountStrategy strategy = discountType switch
{
    DiscountType.Seasonal => new SeasonalDiscountStrategy(),
    DiscountType.Loyalty  => new LoyaltyDiscountStrategy(),
    DiscountType.Coupon   => new CouponDiscountStrategy(couponCode, 150m),
    _                     => new NoDiscountStrategy()
};

context.SetStrategy(strategy);
var discountedAmount = context.ApplyDiscount(totalAmount);
// OrderService never calls Calculate() directly ✅
// It always goes through the Context!
```

### Coupon — Constructor Injection
```csharp
// Coupon needs extra data — injected via constructor
public class CouponDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _couponAmount;
    private readonly string _couponCode;

    public CouponDiscountStrategy(string couponCode, decimal couponAmount)
    {
        _couponCode   = couponCode;
        _couponAmount = couponAmount;
    }

    public decimal Calculate(decimal amount)
    {
        var discounted = amount - _couponAmount;
        return discounted < 0 ? 0 : discounted;  // can't go negative!
    }
}
```

---

## 🆚 Strategy vs Factory

```
Factory Pattern   → decides WHICH object to CREATE
                    "give me the right payment processor"

Strategy Pattern  → decides WHICH algorithm to RUN
                    "give me the right discount calculation"
```

---

## 🧪 Test Endpoints

```json
// No discount (default)
POST /api/orders
{ "productId": 1, "quantity": 2, "paymentMethod": 1 }
→ discountedAmount == totalAmount ✅

// Seasonal — 10% off (discountType: 1)
{ "productId": 1, "quantity": 2, "paymentMethod": 1, "discountType": 1 }
→ totalAmount: 3000, discountedAmount: 2700
→ discountNote: "Seasonal discount (10%) applied — saved ৳300"

// Loyalty — 15% off (discountType: 2)
{ "productId": 1, "quantity": 2, "paymentMethod": 1, "discountType": 2 }
→ totalAmount: 3000, discountedAmount: 2550
→ discountNote: "Loyalty discount (15%) applied — saved ৳450"

// Coupon — 150tk off (discountType: 3)
{ "productId": 1, "quantity": 1, "paymentMethod": 1, "discountType": 3, "couponCode": "SAVE150" }
→ totalAmount: 1500, discountedAmount: 1350
→ discountNote: "Coupon 'SAVE150' applied — saved ৳150"
```

---

## 🔑 DiscountType Enum Values

```
0 = None     (default — no discount)
1 = Seasonal (10% off)
2 = Loyalty  (15% off)
3 = Coupon   (fixed amount off)
```

---

## ✅ Key Takeaways

- Strategy = **each algorithm in its own class**, swappable at runtime
- Context = **holds the strategy**, runs it without knowing which one
- Adding new discount = **one new file** — zero changes to existing strategies
- Coupon uses **constructor injection** to receive discount amount
- Strategy + Factory can combine: Factory creates Strategy ✅
- `discountedAmount` stores the **after-discount price** — original preserved in `totalAmount`



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



# 👁️ Branch: 05-observer
## Observer Pattern — Order Event Notifications

---

## 🤔 What Problem Does It Solve?

Without Observer Pattern, OrderService manually notifies every system:

```csharp
// ❌ BAD — OrderService knows about EVERYTHING
public async Task<Order> PlaceOrderAsync(...)
{
    await _orderRepo.AddAsync(order);

    // manually notify everyone — tightly coupled!
    await _emailService.SendEmail(order);
    await _smsService.SendSMS(order);
    await _inventoryService.Update(order);
    await _analyticsService.Log(order);
    // add Slack notification tomorrow? Edit OrderService again 😬
    // email fails? Everything after it also fails 😬
}
```

**Problems:**
- OrderService coupled to every notification system
- One failure breaks all notifications
- Adding new notification = editing OrderService
- Hard to test in isolation

---

## ✅ The Solution

> When one object (Subject) changes state, all its dependents (Observers) are **notified automatically**.

```
Order placed (event fires)
        ↓
OrderEventPublisher.NotifyAsync(event)  ← ONE call
        ↓         ↓         ↓         ↓
    Email      SMS      Inventory  Analytics
    Observer  Observer   Observer   Observer
    (each runs independently — one failing won't stop others!) ✅
```

---

## 🔄 Data Flow

```
POST /api/orders
        ↓
OrderService.PlaceOrderAsync()
  → validates, calculates discount, processes payment
  → saves order to DB
        ↓
  Observer Pattern ⭐:
  var event = new OrderPlacedEvent(order)
  await _publisher.NotifyAsync(event)
        ↓
OrderEventPublisher loops through _observers list:
        ↓              ↓              ↓              ↓
EmailObserver    SmsObserver   InventoryObserver  AnalyticsObserver
  ✉️ logs email   📱 logs SMS    📦 logs update     📊 logs analytics
  (if fails →    (still runs)   (still runs)       (still runs)
   logged only,
   doesn't crash!)
        ↓
Response: 201 Created ✅
```

---

## 📁 Files Added This Branch

```
Observer/
├── IOrderEvent.cs                  ← event data + OrderEventType enum
│                                      + OrderPlacedEvent concrete class
├── IOrderEventObserver.cs          ← contract all observers follow
├── EmailNotificationObserver.cs    ← sends email notification
├── SmsNotificationObserver.cs      ← sends SMS notification
├── InventoryObserver.cs            ← updates inventory system
├── AnalyticsObserver.cs            ← logs analytics data
├── IOrderEventPublisher.cs         ← subject contract
└── OrderEventPublisher.cs          ← manages list + notifies all

Extensions/
└── ObserverExtensions.cs           ← AddObservers() + SubscribeObservers()
```

---

## 🧠 Key Concepts

### Observer Interface
```csharp
public interface IOrderEventObserver
{
    string ObserverName { get; }
    Task OnOrderPlaced(IOrderEvent orderEvent);  // called when event fires
}
```

### Publisher — The Subject
```csharp
public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly List<IOrderEventObserver> _observers = new();

    public void Subscribe(IOrderEventObserver observer)
        => _observers.Add(observer);      // add to notification list

    public void Unsubscribe(IOrderEventObserver observer)
        => _observers.Remove(observer);   // remove from list

    public async Task NotifyAsync(IOrderEvent orderEvent)
    {
        foreach (var observer in _observers)
        {
            try
            {
                await observer.OnOrderPlaced(orderEvent);  // notify each!
            }
            catch (Exception ex)
            {
                // ✅ ONE failing observer doesn't break others!
                Console.WriteLine($"⚠️ [{observer.ObserverName}] failed: {ex.Message}");
            }
        }
    }
}
```

### Event Object
```csharp
// Carries ALL data observers need
public class OrderPlacedEvent : IOrderEvent
{
    public Order Order { get; }
    public OrderEventType EventType => OrderEventType.OrderPlaced;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public OrderPlacedEvent(Order order) { Order = order; }
}
```

### Subscribing Observers (Program.cs)
```csharp
// After app.Build() — subscribe all observers
app.SubscribeObservers();
// internally:
// publisher.Subscribe(emailObserver);
// publisher.Subscribe(smsObserver);
// publisher.Subscribe(inventoryObserver);
// publisher.Subscribe(analyticsObserver);
```

### OrderService — ONE Line Does Everything
```csharp
// Save order
await _orderRepo.AddAsync(order);

// Notify ALL observers ← Observer Pattern ⭐
await _publisher.NotifyAsync(new OrderPlacedEvent(order));
// OrderService doesn't know WHO is listening ✅
// Doesn't know what they do ✅
// Doesn't care if one fails ✅
```

---

## 🆚 Observer vs Other Patterns

```
Factory   → creates objects
Strategy  → selects algorithm
Observer  → notifies multiple listeners of an event ← this pattern
```

---

## 🧪 What You See In Terminal

```bash
POST /api/orders → triggers:

🔔 Notifying 4 observer(s)...
✉️  [EMAIL] Order #1 confirmed!
    To: customer@email.com
    Subject: Your order of 2 item(s) — Total: ৳2,700.00
📱 [SMS] Sending to customer...
    'Your order #1 has been placed! Total: ৳2,700.00. Thank you!'
📦 [INVENTORY] Stock update logged
    Product #1 — 2 unit(s) dispatched
📊 [ANALYTICS] Event logged
    Event: order_placed
    ProductId: 1
    Revenue: ৳2,700.00
    PaymentMethod: Cash
```

---

## 🔑 Extension Methods — Clean Program.cs

```csharp
// ObserverExtensions.cs
public static IServiceCollection AddObservers(this IServiceCollection services)
{
    services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
    services.AddSingleton<EmailNotificationObserver>();
    // ...
    return services;
}

public static WebApplication SubscribeObservers(this WebApplication app)
{
    var publisher = app.Services.GetRequiredService<IOrderEventPublisher>();
    publisher.Subscribe(app.Services.GetRequiredService<EmailNotificationObserver>());
    // ...
    return app;
}

// Program.cs stays clean:
builder.Services.AddObservers();
app.SubscribeObservers();
```

---

## ✅ Key Takeaways

- Observer = **one event fires → many listeners react** automatically
- `try/catch` per observer = **one failure never stops others**
- Publisher **doesn't know** what observers do — completely decoupled
- Adding new notification = **new file + one Subscribe() call** — zero other changes
- Publisher registered as **Singleton** — all services share same observer list
- `IOrderEvent` carries all data — observers get everything they need



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



# 🔌 Branch: 07-adapter
## Adapter Pattern — Legacy Shipping System

---

## 🤔 What Problem Does It Solve?

ShopAPI needs to use a legacy shipping system with an incompatible interface:

```csharp
// ❌ Legacy system — ugly interface, can't change it!
public class LegacyShippingSystem
{
    public string ship_product(int prod_id, int qty, string addr)
    → returns "SHIP_1_2_123456789"   // weird string format

    public string get_shipping_cost(int prod_id, int qty)
    → returns "COST:150"             // string not decimal!

    public string track_shipment(string ship_code)
    → returns "STATUS:IN_TRANSIT"    // yet another format
}
```

Using this directly in OrderService would mean:
- Legacy code scattered throughout clean codebase
- Switching shipping providers = rewriting everything
- Hard to test — tightly coupled to legacy system
- String parsing logic everywhere

---

## ✅ The Solution

> Convert the interface of a class into another interface that clients expect. Adapter lets **incompatible interfaces work together**.

```
ShopAPI (wants modern interface)
        ↓
IShippingService         ← modern contract
        ↓
LegacyShippingAdapter    ← THE ADAPTER
  → translates inputs
  → calls legacy system
  → translates outputs
        ↓
LegacyShippingSystem     ← old code (untouched!)
```

**Think of a power adapter:**
```
Your laptop ──── Power Adapter ──── Old wall socket
(modern plug)    (bridges gap)      (old socket)
You don't rewire your laptop
You don't rewire the wall
Adapter bridges the gap ✅
```

---

## 🔄 Data Flow

```
POST /api/orders { "address": "Mirpur, Dhaka" }
        ↓
OrderService.PlaceOrderAsync()
  → saves order to DB
        ↓
  Adapter Pattern ⭐:
  _shipping.GetShippingCostAsync(productId, quantity)
        ↓
LegacyShippingAdapter.GetShippingCostAsync(1, 2)
  → translates: productId=1, quantity=2
  → calls: _legacy.get_shipping_cost(prod_id: 1, qty: 2)
        ↓
LegacyShippingSystem.get_shipping_cost(1, 2)
  → returns: "COST:150"
        ↓
LegacyShippingAdapter parses "COST:150" → decimal 150m
  → returns: 150m  ← clean decimal ✅
        ↓
_shipping.ShipOrderAsync(productId, quantity, address)
        ↓
LegacyShippingAdapter.ShipOrderAsync(1, 2, "Mirpur, Dhaka")
  → calls: _legacy.ship_product(prod_id:1, qty:2, addr:"Mirpur")
  → returns: "SHIP_1_2_123456789"
  → parses: "SHIP_1_2_123456789" → "TRK-1_2_123456789"
  → returns: ShippingResult { Success=true, TrackingCode="TRK-..." } ✅
        ↓
order.TrackingCode = "TRK-..."
order.ShippingCost = 150m
await _orderRepo.UpdateAsync(order)   ← update (not add again!)
        ↓
Response: 201 Created with shipping info ✅
```

---

## 📁 Files Added This Branch

```
Adapter/
├── ShippingResult.cs           ← modern clean result model
├── IShippingService.cs         ← modern interface (what ShopAPI wants)
├── LegacyShippingSystem.cs     ← old system (DO NOT MODIFY!)
└── LegacyShippingAdapter.cs    ← THE ADAPTER ⭐ bridges old ↔ new

Models/
└── Order.cs                    ← + TrackingCode, ShippingCost,
                                     ShippingStatus, EstimatedDelivery

Extensions/
└── AdapterExtensions.cs        ← AddAdapters() registration

Controllers/
└── OrdersController.cs         ← + GET /{trackingCode}/track endpoint
```

---

## 🧠 Key Concepts

### Modern Interface
```csharp
public interface IShippingService
{
    Task<ShippingResult> ShipOrderAsync(int productId, int quantity, string address);
    Task<decimal> GetShippingCostAsync(int productId, int quantity);
    Task<string> TrackShipmentAsync(string trackingCode);
}
// Clean, async, strongly typed — exactly what ShopAPI wants!
```

### The Adapter — Translates Both Ways
```csharp
public class LegacyShippingAdapter : IShippingService
{
    private readonly LegacyShippingSystem _legacy = new();

    public Task<decimal> GetShippingCostAsync(int productId, int quantity)
    {
        // 1. Translate inputs + call legacy
        var response = _legacy.get_shipping_cost(
            prod_id: productId,   // modern → legacy param names
            qty: quantity);

        // 2. Translate output: "COST:150" → 150m
        var parts = response.Split(':');
        var cost = decimal.Parse(parts[1]);

        // 3. Return clean decimal ✅
        return Task.FromResult(cost);
    }
}
```

### Private Translators — Heart of the Adapter
```csharp
// Adapter has private methods to parse legacy responses:

private string ParseTrackingCode(string legacy)
    // "SHIP_1_2_123" → "TRK-1_2_123"

private decimal ParseCost(string legacy)
    // "COST:150" → 150m

private string ParseStatus(string legacy)
    // "STATUS:IN_TRANSIT" → "In Transit"
    // "STATUS:DELIVERED"  → "Delivered"
```

### Critical Bug Fixed — UpdateAsync not AddAsync
```csharp
// ❌ BUG: calling AddAsync twice
await _orderRepo.AddAsync(order);          // order.Id = 5 assigned by SQL
// ... shipping ...
await _orderRepo.AddAsync(order);          // ERROR: can't insert Id=5 again!

// ✅ FIX: AddAsync first, UpdateAsync for shipping fields
await _orderRepo.AddAsync(order);          // order.Id = 5 assigned ✅
// ... shipping ...
await _orderRepo.UpdateAsync(order);       // UPDATE WHERE Id=5 ✅
```

---

## 🆚 Adapter vs Decorator

```
Decorator  → SAME interface in + out → ADDS behaviour to existing object
             e.g. LoggingRepository wraps ProductRepository
             both are IProductRepository

Adapter    → DIFFERENT interface in → converts to DIFFERENT interface out
             e.g. LegacyShippingAdapter wraps LegacyShippingSystem
             legacy has ship_product(), adapter exposes ShipOrderAsync()
```

---

## 🧪 Test Endpoints

```json
// Place order with address (triggers shipping)
POST /api/orders
{
    "productId": 1,
    "quantity": 2,
    "paymentMethod": 1,
    "address": "Mirpur, Dhaka"
}
→ 201 Created
{
    "id": 1,
    "trackingCode": "TRK-1_2_...",
    "shippingCost": 150,
    "shippingStatus": "In Transit",
    "estimatedDelivery": "2024-01-18T..."
}

// Track shipment
GET /api/orders/TRK-1_2_.../track
→ 200 OK { "trackingCode": "TRK-...", "status": "In Transit" }
```

Terminal shows legacy being called:
```
[LEGACY] Getting cost for 2x product#1
[LEGACY] Shipping 2x product#1 to Mirpur, Dhaka
// OrderService never saw LegacyShippingSystem directly! ✅
```

---

## 🔑 Switching Shipping Providers Tomorrow

```csharp
// New shipping provider available?
// Step 1: Create ModernShippingAdapter : IShippingService
// Step 2: Change ONE line in AdapterExtensions.cs:

// ❌ Old:
services.AddScoped<IShippingService, LegacyShippingAdapter>();

// ✅ New:
services.AddScoped<IShippingService, ModernShippingAdapter>();

// Zero changes to OrderService, Controller, or any other file ✅
```

---

## ✅ Key Takeaways

- Adapter = **bridges incompatible interfaces** — never modify the legacy code!
- `IShippingService` = what ShopAPI wants — **clean, modern, async**
- Adapter **translates inputs AND outputs** — both directions
- Private parser methods = heart of the adapter — **isolate ugly parsing**
- `UpdateAsync` not `AddAsync` for existing records — **never insert with explicit Id**
- Switching providers = **one line change** in extensions — zero other changes
- `Task.FromResult()` wraps sync legacy calls in async-compatible Tasks