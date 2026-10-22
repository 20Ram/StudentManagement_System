namespace CrudOperation.Service
{
    public class PaymentException : Exception
    {
        public int StatusCode { get; }

        public PaymentException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
