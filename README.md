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
