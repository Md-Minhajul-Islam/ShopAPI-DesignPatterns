using ShopAPI.Models;

namespace ShopAPI.Factory
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor Create(PaymentMethod paymentMethod);
    }

    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        // Factory methos - one place that knows all processors
        public IPaymentProcessor Create(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => new CashPaymentProcessor(),
                PaymentMethod.Card => new CardPaymentProcessor(),
                PaymentMethod.Bkash => new BkashPaymentProcessor(),
                PaymentMethod.Nagad => new NagadPaymentProcessor(),

                // Safety net - new enum value added but factory not updated
                _ => throw new NotSupportedException($"Payment method '{paymentMethod} is not supported.'")
            };
        }
    }
} 
