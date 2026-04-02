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
