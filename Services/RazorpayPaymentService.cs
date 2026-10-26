using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service
{
    public class RazorpayPaymentService : IRazorpayPaymentService
    {
        private const string RazorpayApi = "https://api.razorpay.com/v1";
        private static readonly TimeSpan PendingReuseWindow = TimeSpan.FromMinutes(15);
        private readonly HttpClient _httpclient;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IFeePaymentAmountProvider _feeProvider;
        private readonly ILogger<RazorpayPaymentService> _logger;

        public RazorpayPaymentService(
            HttpClient httpClient, AppDbContext context, IConfiguration configuration,
            IFeePaymentAmountProvider feeProvider, ILogger<RazorpayPaymentService> logger)
        {
            _httpclient = httpClient;
            _context = context;
            _configuration = configuration;
            _feeProvider = feeProvider;
            _logger = logger;
        }
        public async Task<PaymentOrderResponseDto> CreateOrderAsync(
            int feeItemId, string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            string keyId = GetSetting("RAZORPAY_KEY_ID");
            string secret = GetSetting("RAZORPAY_KEY_SECRET");

            var fee = await _feeProvider.GetPayableFeeAsync(feeItemId, userId, isAdmin, cancellationToken)
                ?? throw new PaymentException("Fee is unavailable, already paid, or access is denied.");

            long amountInPaise = MoneyHelper.ToPaise(fee.Amount);
            string ownerUserId = fee.OwnerUserId.ToString(CultureInfo.InvariantCulture);
 
            var pending = await _context.PaymentTransactions
                .Where(x => x.FeeItemId == feeItemId && x.Status == PaymentStatus.Pending)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            var cutoff = DateTime.UtcNow - PendingReuseWindow;
            var reusable = pending.FirstOrDefault(x => x.Amount == fee.Amount && x.CreatedAtUtc >= cutoff);
            if (reusable != null)
                return ToOrderResponse(reusable, keyId);

            if (pending.Count > 0)
            {
                foreach (var stale in pending)
                {
 
                    bool hadFailure = !string.IsNullOrEmpty(stale.FailureReason);
                    stale.Status = hadFailure ? PaymentStatus.Failed : PaymentStatus.Cancelled;
                    if (!hadFailure) stale.FailureReason = "Superseded by a newer order.";
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            string orderId = await CreateRazorpayOrderAsync(
                keyId, secret, amountInPaise, feeItemId, ownerUserId, cancellationToken);

            var tx = new PaymentTransaction
            {
                UserId = ownerUserId,
                FeeItemId = feeItemId,
                Amount = fee.Amount,
                RazorpayOrderId = orderId,
                Status = PaymentStatus.Pending
            };
            _context.PaymentTransactions.Add(tx);
            await _context.SaveChangesAsync(cancellationToken);

            return ToOrderResponse(tx, keyId);
        }

        private async Task<string> CreateRazorpayOrderAsync(
            string keyId, string secret, long amountInPaise, int feeItemId, string ownerUserId,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{RazorpayApi}/orders");
            request.Headers.Authorization = CreateAuthHeader(keyId, secret);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    amount = amountInPaise,
                    currency = "INR",
                    receipt = $"fee_{Guid.NewGuid():N}",   // 36 chars, Razorpay limit is 40
                    notes = new
                    {
                        feeItemId = feeItemId.ToString(CultureInfo.InvariantCulture),
                        userId = ownerUserId
                    }
                }),
                Encoding.UTF8, "application/json");

            try
            {
                using var response = await _httpclient.SendAsync(request, cancellationToken);
                string body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Razorpay order creation failed. Status {Status}. Body {Body}",
                        (int)response.StatusCode, body);
                    throw new PaymentException("Could not start payment. Please try again later.", 502);
                }

                using var json = JsonDocument.Parse(body);
                string? orderId = GetString(json.RootElement, "id");
                if (string.IsNullOrWhiteSpace(orderId))
                    throw new PaymentException("Could not start payment. Please try again later.", 502);

                return orderId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not reach Razorpay while creating an order.");
                throw new PaymentException("Could not start payment. Please try again later.", 502);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Razorpay order request timed out.");
                throw new PaymentException("Payment provider timed out. Please try again.", 504);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Razorpay returned an unreadable order response.");
                throw new PaymentException("Could not start payment. Please try again later.", 502);
            }
        }
 
        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            VerifyPaymentDto dto, string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            string secret = GetSetting("RAZORPAY_KEY_SECRET");
            string keyId = GetSetting("RAZORPAY_KEY_ID");

            var tx = await _context.PaymentTransactions
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == dto.PaymentTransactionId, cancellationToken);
 
            const string generic = "Payment verification failed.";
            if (tx == null || (!isAdmin && tx.UserId != userId) || tx.RazorpayOrderId != dto.RazorpayOrderId)
                return new PaymentVerificationResult(false, generic);

            if (tx.Status == PaymentStatus.Paid)
            {
                return tx.RazorpayPaymentId == dto.RazorpayPaymentId
                    ? new PaymentVerificationResult(true, "Payment already verified.")
                    : new PaymentVerificationResult(false, generic);
            }

 
            string payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
            if (!IsValidHmacHex(secret, Encoding.UTF8.GetBytes(payload), dto.RazorpaySignature))
                return new PaymentVerificationResult(false, generic);

 
            JsonElement p = await FetchPaymentAsync(keyId, secret, dto.RazorpayPaymentId, cancellationToken);

            string? status = GetString(p, "status");

            if (status == "authorized")
            { 
                return new PaymentVerificationResult(false,
                    "Payment is authorized but not captured yet. It will be confirmed automatically once captured.");
            }

            bool captured = p.TryGetProperty("captured", out var c) && c.ValueKind == JsonValueKind.True;

            if (GetString(p, "order_id") != tx.RazorpayOrderId
                || status != "captured"
                || !captured
                || GetString(p, "currency") != tx.Currency
                || GetLong(p, "amount") != MoneyHelper.ToPaise(tx.Amount))
            {
                return new PaymentVerificationResult(false, generic);
            }

            await FinalizePaymentAsync(tx.Id, dto.RazorpayPaymentId, cancellationToken);
            return new PaymentVerificationResult(true, "Payment verified successfully.");
        }

        private async Task<JsonElement> FetchPaymentAsync(
            string keyId, string secret, string razorpayPaymentId, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{RazorpayApi}/payments/{Uri.EscapeDataString(razorpayPaymentId)}");
            request.Headers.Authorization = CreateAuthHeader(keyId, secret);

            try
            {
                using var response = await _httpclient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Razorpay payment lookup failed: {Status}", (int)response.StatusCode);
                    throw new PaymentException(
                        "Could not confirm the payment right now. If money was deducted it will be applied automatically.", 502);
                }

                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                return json.RootElement.Clone();    
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not reach Razorpay while verifying a payment.");
                throw new PaymentException(
                    "Could not confirm the payment right now. If money was deducted it will be applied automatically.", 502);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Razorpay payment lookup timed out.");
                throw new PaymentException("Payment provider timed out. Please try again.", 504);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Razorpay returned an unreadable payment response.");
                throw new PaymentException("Could not confirm the payment right now.", 502);
            }
        }

 
        public async Task<bool> HandleWebhookAsync(
            byte[] rawBody, string? signatureHeader, CancellationToken cancellationToken = default)
        {
            string webhookSecret = GetSetting("RAZORPAY_WEBHOOK_SECRET");

            if (string.IsNullOrWhiteSpace(signatureHeader) ||
                !IsValidHmacHex(webhookSecret, rawBody, signatureHeader.Trim()))
            {
                return false;
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(rawBody);
            }
            catch (JsonException ex)
            { 
                _logger.LogError(ex, "Razorpay webhook body was signed but is not valid JSON.");
                return true;
            }

            using (document)
            {
                var root = document.RootElement;
                string? evt = GetString(root, "event");

                
                if (evt != "payment.captured" && evt != "order.paid" && evt != "payment.failed")
                    return true;

                if (!TryGetPaymentEntity(root, out var entity))
                    return true;

                string? orderId = GetString(entity, "order_id");
                string? paymentId = GetString(entity, "id");
                if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(paymentId))
                    return true;

                var tx = await _context.PaymentTransactions
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.RazorpayOrderId == orderId, cancellationToken);

                
                if (tx == null) return true;

                if (evt == "payment.failed")
                {
                    string reason = GetString(entity, "error_description") ?? "Payment failed";
                    await RecordFailureAsync(tx.Id, reason, cancellationToken);
                    return true;
                }

             
                bool amountOk = GetLong(entity, "amount") == MoneyHelper.ToPaise(tx.Amount);
                bool currencyOk = GetString(entity, "currency") == tx.Currency;
                if (!amountOk || !currencyOk)
                {
                 
                    _logger.LogError(
                        "Webhook amount/currency mismatch for order {Order}, payment {Payment}. Manual review needed.",
                        orderId, paymentId);
                    return true;
                }

                await FinalizePaymentAsync(tx.Id, paymentId, cancellationToken);
                return true;
            }
        }
 
        private async Task RecordFailureAsync(int transactionId, string reason, CancellationToken cancellationToken)
        {
            string trimmed = reason.Length > 250 ? reason.Substring(0, 250) : reason;

            await _context.PaymentTransactions
                .Where(x => x.Id == transactionId && x.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FailureReason, trimmed), cancellationToken);
        }
 
        private async Task FinalizePaymentAsync(
            int transactionId, string razorpayPaymentId, CancellationToken cancellationToken)
        {
            
            _context.ChangeTracker.Clear();

            await using var dbTx = await _context.Database.BeginTransactionAsync(cancellationToken);

            var txRows = await _context.PaymentTransactions
                .FromSqlInterpolated($"SELECT * FROM PaymentTransactions WHERE Id = {transactionId} FOR UPDATE")
                .ToListAsync(cancellationToken);
            var tx = txRows.SingleOrDefault();

            if (tx == null || tx.Status == PaymentStatus.Paid)
            {
                await dbTx.RollbackAsync(cancellationToken);
                return;    
            }

            var feeRows = await _context.StudentFees
                .FromSqlInterpolated($"SELECT * FROM StudentFees WHERE Id = {tx.FeeItemId} FOR UPDATE")
                .ToListAsync(cancellationToken);
            var fee = feeRows.SingleOrDefault();

            tx.Status = PaymentStatus.Paid;
            tx.RazorpayPaymentId = razorpayPaymentId;
            tx.PaidAtUtc = DateTime.UtcNow;
            tx.FailureReason = null;

            if (fee == null)
            {
                _logger.LogError("Fee {FeeId} not found while finalizing payment {TxId}.", tx.FeeItemId, tx.Id);
                tx.FailureReason = "Fee record missing; manual review required.";
            }
            else
            {
                decimal remaining = Math.Max(0m, fee.TotalAmount - fee.PaidAmount);
                decimal applied = Math.Min(tx.Amount, remaining);
                fee.PaidAmount += applied;

                if (applied < tx.Amount)
                {
 
                    decimal excess = tx.Amount - applied;
                    string note = $"Overpaid by {excess.ToString("0.00", CultureInfo.InvariantCulture)}; refund required.";
                    tx.FailureReason = note.Length > 250 ? note.Substring(0, 250) : note;
                    _logger.LogError(
                        "Overpayment on fee {FeeId}: payment {Payment} exceeded the amount due by {Excess}. Refund required.",
                        fee.Id, razorpayPaymentId, excess);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await dbTx.CommitAsync(cancellationToken);
        }

 
        public async Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(
            string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentTransactions.AsNoTracking();
            if (!isAdmin) query = query.Where(x => x.UserId == userId);

            return await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(200)
                .Select(x => new PaymentHistoryDto
                {
                    Id = x.Id,
                    FeeItemId = x.FeeItemId,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Status = x.Status,
                    RazorpayOrderId = x.RazorpayOrderId,
                    RazorpayPaymentId = x.RazorpayPaymentId,
                    FailureReason = x.FailureReason,
                    CreatedAtUtc = x.CreatedAtUtc,
                    PaidAtUtc = x.PaidAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PaymentHistoryDto?> GetByIdAsync(
            int id, string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentTransactions.AsNoTracking().Where(x => x.Id == id);
            if (!isAdmin) query = query.Where(x => x.UserId == userId);

            return await query.Select(x => new PaymentHistoryDto
            {
                Id = x.Id,
                FeeItemId = x.FeeItemId,
                Amount = x.Amount,
                Currency = x.Currency,
                Status = x.Status,
                RazorpayOrderId = x.RazorpayOrderId,
                RazorpayPaymentId = x.RazorpayPaymentId,
                FailureReason = x.FailureReason,
                CreatedAtUtc = x.CreatedAtUtc,
                PaidAtUtc = x.PaidAtUtc
            }).SingleOrDefaultAsync(cancellationToken);
        }
 
        private static PaymentOrderResponseDto ToOrderResponse(PaymentTransaction tx, string keyId) => new()
        {
            PaymentTransactionId = tx.Id,
            RazorpayOrderId = tx.RazorpayOrderId,
            AmountInPaise = MoneyHelper.ToPaise(tx.Amount),
            Currency = tx.Currency,
            KeyId = keyId
        };

        private static bool TryGetPaymentEntity(JsonElement root, out JsonElement entity)
        {
            entity = default;
            return root.TryGetProperty("payload", out var payload)
                && payload.ValueKind == JsonValueKind.Object
                && payload.TryGetProperty("payment", out var payment)
                && payment.ValueKind == JsonValueKind.Object
                && payment.TryGetProperty("entity", out entity)
                && entity.ValueKind == JsonValueKind.Object;
        }

        private static string? GetString(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var v)
            && v.ValueKind == JsonValueKind.String
                ? v.GetString()
                : null;

        private static long? GetLong(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var v)
            && v.ValueKind == JsonValueKind.Number
            && v.TryGetInt64(out long l)
                ? l
                : null;

        private static bool IsValidHmacHex(string secret, byte[] data, string suppliedHex)
        {
            byte[] expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), data);
            try
            {
                byte[] supplied = Convert.FromHexString(suppliedHex);
                return supplied.Length == expected.Length
                    && CryptographicOperations.FixedTimeEquals(supplied, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private string GetSetting(string key) =>
            _configuration[key]
            ?? Environment.GetEnvironmentVariable(key)
            ?? throw new InvalidOperationException($"Missing configuration setting: {key}");

        private static AuthenticationHeaderValue CreateAuthHeader(string keyId, string secret) =>
            new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{secret}")));
    }
}
