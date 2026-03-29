using System.Collections.Generic;

namespace Application.Features.Expenses.Dtos
{
    public class UpdateExpenseDto
    {
        public string Tur { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }

        // Şahsi kalemler
        public List<PersonalExpenseDto> SahsiHarcamalar { get; set; } = new();

        // 🔹 Not/Açıklama – FE farklı anahtarlar gönderebildiği için hepsini destekle
        public string? Aciklama { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
    }
}
