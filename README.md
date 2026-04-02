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
