namespace EvArkadasim.API.Services.Receipts
{
    public interface IReceiptOcrService
    {
        Task<ReceiptOcrResult> ExtractAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    }
}
