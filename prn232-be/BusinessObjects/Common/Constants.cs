namespace BusinessObjects.Common
{
    public static class InvoiceTypes
    {
        public const string Car = "Car";
        public const string Part = "Part";
        public const string Service = "Service";
    }

    public static class PaymentStatuses
    {
        public const string Unpaid = "Unpaid";
        public const string Deposited = "Deposited";
        public const string PartiallyPaid = "PartiallyPaid";
        public const string Paid = "Paid";
        public const string Refunded = "Refunded";
    }

    public static class InvoiceStatuses
    {
        public const string Pending = "Pending";
        public const string PendingVerification = "PendingVerification";
        public const string Confirmed = "Confirmed";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class PartOrderStatuses
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Shipping = "Shipping";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class AppointmentStatuses
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class DeliveryMethods
    {
        public const string Pickup = "Pickup";
        public const string Shipping = "Shipping";
    }

    public static class PaymentMethods
    {
        public const string CashAtShowroom = "CashAtShowroom";
        public const string BankTransfer = "BankTransfer";
        public const string COD = "COD";
    }

    public static class InventoryTransactionTypes
    {
        public const string Import = "Import";
        public const string Export = "Export";
        public const string Return = "Return";
        public const string Adjustment = "Adjustment";
    }

    public static class InventoryReferenceTypes
    {
        public const string SupplierReceipt = "SupplierReceipt";
        public const string PartOrder = "PartOrder";
        public const string MaintenanceAppointment = "MaintenanceAppointment";
        public const string StockAdjustment = "StockAdjustment";
    }
}
