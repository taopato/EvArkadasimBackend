using System;

namespace Domain.Entities
{
    public class BillDocument
    {
        public int Id { get; set; }
        public int BillId { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePathOrUrl { get; set; } = null!;
        public int UploadedByUserId { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Bill Bill { get; set; } = null!;
    }
}
