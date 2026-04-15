namespace TCG.Shop
{
    public enum PurchaseStatus
    {
        Success,
        InsufficientFunds,
        OutOfStock,
        AlreadyOwned,
        InvalidListing,
        InventoryFull
    }

    /// <summary>
    /// Returned by ShopManager.TryPurchase to describe the outcome of a purchase attempt.
    /// </summary>
    public readonly struct PurchaseResult
    {
        public readonly PurchaseStatus Status;
        public readonly string         Message;
        public readonly ShopItemListing Listing;

        public bool IsSuccess => Status == PurchaseStatus.Success;

        public PurchaseResult(PurchaseStatus status, ShopItemListing listing, string message = "")
        {
            Status  = status;
            Listing = listing;
            Message = message;
        }

        public static PurchaseResult Success(ShopItemListing listing)
            => new(PurchaseStatus.Success, listing, "Purchase successful.");

        public static PurchaseResult Fail(PurchaseStatus status, ShopItemListing listing, string message)
            => new(status, listing, message);

        public override string ToString() => $"[{Status}] {Message}";
    }
}
