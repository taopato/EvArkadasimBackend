namespace Domain.Enums
{
    public enum VisibilityMode : byte
    {
        OnBillDate = 0,        // Fatura gününde görünür
        BeforeDueByDays = 1,   // Son ödeme gününden X gün önce
        OnDueDate = 2          // Son ödeme gününde görünür
    }
}
