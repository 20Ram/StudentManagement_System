namespace CrudOperation.Service
{
    public static class MoneyHelper
    {
        public static long ToPaise(decimal rupees)
        {
            decimal paise = decimal.Round(rupees * 100m, 0, MidpointRounding.AwayFromZero);
            if (paise <= 0 || paise > long.MaxValue)
                throw new PaymentException("Invalid payment amount.");
            return (long)paise;
        }
    }
}