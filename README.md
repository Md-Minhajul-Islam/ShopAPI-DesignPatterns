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
