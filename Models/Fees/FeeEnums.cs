namespace CrudOperation.Models.Fees;
public enum FeeStructureStatus
{
    Draft,
    Published,
    Archived
}

public enum FeeAssignmentStatus
{
    Active,
    Completed,
    Cancelled
}

public enum FeeInstallmentStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public enum FeePaymentMethod
{
    Razorpay,
    Cash,
    BankTransfer,
    Upi,
    Cheque,
    Other
}

public enum FeePaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Cancelled,
    PartiallyRefunded,
    Refunded
}

public enum FeeAdjustmentType
{
    Discount,
    Waiver,
    AdditionalCharge,
    Correction
}

public enum FeeAdjustmentStatus
{
    PendingApproval,
    Approved,
    Rejected,
    Cancelled
}

public enum FeeLateFeeStatus
{
    Pending,
    Applied,
    Waived,
    Cancelled
}

public enum FeeRefundStatus
{
    PendingApproval,
    Approved,
    Processing,
    Succeeded,
    Failed,
    Rejected
}

public enum FeeApprovalDecision
{
    Pending,
    Approved,
    Rejected
}

public enum FeeNotificationType
{
    UpcomingDue,
    DueToday,
    Overdue,
    PaymentConfirmation,
    RefundUpdate
}

public enum FeeNotificationStatus
{
    Pending,
    Processing,
    Sent,
    Failed,
    Cancelled
}

public enum GatewayEventStatus
{
    Received,
    Processing,
    Processed,
    Failed
}