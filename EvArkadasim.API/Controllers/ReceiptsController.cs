using Application.Features.Expenses.Commands.CreateExpense;
using Domain.Entities;
using Domain.Enums;
using EvArkadasim.API.Services.Receipts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace EvArkadasim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReceiptsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IReceiptOcrService _ocrService;
        private readonly IMediator _mediator;

        public ReceiptsController(
            AppDbContext dbContext,
            IWebHostEnvironment environment,
            IReceiptOcrService ocrService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _environment = environment;
            _ocrService = ocrService;
            _mediator = mediator;
        }

        [HttpPost("Scan")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Scan([FromForm] ReceiptScanRequest request, CancellationToken cancellationToken)
        {
            if (request.Image == null || request.Image.Length == 0)
                return BadRequest("Fiş görseli zorunlu.");

            var turkeyToday = DateTime.UtcNow.AddHours(3).Date;
            var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "receipts");
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(request.Image.FileName);
            var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension;
            var fileName = $"{request.HouseId}_{request.UploadedByUserId}_{turkeyToday:yyyyMMdd}_{Guid.NewGuid():N}{safeExtension}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using (var target = System.IO.File.Create(fullPath))
            {
                await request.Image.CopyToAsync(target, cancellationToken);
            }

            ReceiptOcrResult ocrResult;
            await using (var readStream = request.Image.OpenReadStream())
            {
                ocrResult = await _ocrService.ExtractAsync(readStream, request.Image.FileName, cancellationToken);
            }

            var receipt = new Receipt
            {
                HouseId = request.HouseId,
                UploadedByUserId = request.UploadedByUserId,
                ImageUrl = $"/uploads/receipts/{fileName}",
                CreatedAt = DateTime.UtcNow,
            };
            ApplyOcrResult(receipt, ocrResult, turkeyToday);

            _dbContext.Receipts.Add(receipt);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(await BuildReceiptDetailAsync(receipt.Id, cancellationToken));
        }

        [HttpGet("ByHouse/{houseId:int}")]
        public async Task<IActionResult> ByHouse(int houseId, CancellationToken cancellationToken)
        {
            var receipts = await _dbContext.Receipts
                .AsNoTracking()
                .Where(x => x.HouseId == houseId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.HouseId,
                    x.UploadedByUserId,
                    x.ImageUrl,
                    x.StoreName,
                    x.ReceiptDate,
                    x.DetectedTotalAmount,
                    x.Status,
                    x.CreatedAt,
                    ItemCount = x.Items.Count,
                    x.ConvertedExpenseId
                })
                .ToListAsync(cancellationToken);

            return Ok(receipts);
        }

        [HttpGet("{receiptId:int}")]
        public async Task<IActionResult> Get(int receiptId, CancellationToken cancellationToken)
        {
            var receipt = await BuildReceiptDetailAsync(receiptId, cancellationToken);
            return receipt == null ? NotFound() : Ok(receipt);
        }

        [HttpPost("{receiptId:int}/Reparse")]
        public async Task<IActionResult> Reparse(int receiptId, CancellationToken cancellationToken)
        {
            var receipt = await _dbContext.Receipts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == receiptId, cancellationToken);

            if (receipt == null) return NotFound();
            if (string.IsNullOrWhiteSpace(receipt.ImageUrl)) return BadRequest("Fiş görseli bulunamadı.");

            var relativePath = receipt.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(webRoot, relativePath);
            if (!System.IO.File.Exists(fullPath)) return NotFound("Fiş dosyası bulunamadı.");

            await using var stream = System.IO.File.OpenRead(fullPath);
            var ocrResult = await _ocrService.ExtractAsync(stream, Path.GetFileName(fullPath), cancellationToken);

            ApplyOcrResult(receipt, ocrResult);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(await BuildReceiptDetailAsync(receiptId, cancellationToken));
        }

        [HttpPut("{receiptId:int}")]
        public async Task<IActionResult> Update(int receiptId, [FromBody] UpdateReceiptRequest request, CancellationToken cancellationToken)
        {
            var receipt = await _dbContext.Receipts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == receiptId, cancellationToken);

            if (receipt == null) return NotFound();

            receipt.StoreName = request.StoreName;
            receipt.ReceiptDate = request.ReceiptDate;
            receipt.DetectedTotalAmount = request.DetectedTotalAmount;
            receipt.Status = ReceiptStatus.Reviewed;
            receipt.UpdatedAt = DateTime.UtcNow;

            _dbContext.ReceiptItems.RemoveRange(receipt.Items);
            receipt.Items.Clear();

            foreach (var item in request.Items.OrderBy(x => x.SortOrder))
            {
                receipt.Items.Add(new ReceiptItem
                {
                    Name = item.Name?.Trim() ?? string.Empty,
                    Price = item.Price,
                    Quantity = item.Quantity <= 0 ? 1 : item.Quantity,
                    LineTotal = item.LineTotal > 0 ? item.LineTotal : item.Price * (item.Quantity <= 0 ? 1 : item.Quantity),
                    BoxLeft = item.BoxLeft,
                    BoxTop = item.BoxTop,
                    BoxWidth = item.BoxWidth,
                    BoxHeight = item.BoxHeight,
                    IsAssigned = item.IsAssigned,
                    IsShared = item.IsShared,
                    PersonalUserId = item.IsShared ? null : item.PersonalUserId,
                    SortOrder = item.SortOrder,
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(await BuildReceiptDetailAsync(receiptId, cancellationToken));
        }

        [HttpPost("{receiptId:int}/ConvertToExpense")]
        public async Task<IActionResult> ConvertToExpense(int receiptId, [FromBody] ConvertReceiptToExpenseRequest request, CancellationToken cancellationToken)
        {
            var receipt = await _dbContext.Receipts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == receiptId, cancellationToken);

            if (receipt == null) return NotFound();
            if (receipt.Items.Count == 0) return BadRequest("Fişte dönüştürülecek kalem bulunamadı.");

            var sharedTotal = receipt.Items.Where(x => x.IsShared).Sum(x => x.LineTotal);
            var personalItems = receipt.Items
                .Where(x => !x.IsShared && x.PersonalUserId.HasValue)
                .GroupBy(x => x.PersonalUserId!.Value)
                .Select(g => new PersonalExpenseDto
                {
                    UserId = g.Key,
                    Tutar = g.Sum(x => x.LineTotal)
                })
                .ToList();

            var title = string.IsNullOrWhiteSpace(receipt.StoreName) ? "Fiş Harcaması" : $"{receipt.StoreName} Fişi";
            var note = request.Note ?? receipt.StoreName ?? "Fişten oluşturuldu";

            var command = new CreateExpenseCommand
            {
                Tur = title,
                Tutar = receipt.Items.Sum(x => x.LineTotal),
                HouseId = receipt.HouseId,
                OdeyenUserId = request.PayerUserId,
                KaydedenUserId = request.RecordedByUserId,
                OrtakHarcamaTutari = sharedTotal,
                Date = receipt.ReceiptDate ?? DateTime.UtcNow,
                SahsiHarcamalar = personalItems,
                Category = request.Category ?? ExpenseCategory.Market,
                Note = note,
                Description = note,
                Aciklama = note
            };

            var createdExpense = await _mediator.Send(command, cancellationToken);

            receipt.Status = ReceiptStatus.Converted;
            receipt.ConvertedExpenseId = createdExpense.Id;
            receipt.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                receiptId = receipt.Id,
                expense = createdExpense
            });
        }

        private async Task<object?> BuildReceiptDetailAsync(int receiptId, CancellationToken cancellationToken)
        {
            return await _dbContext.Receipts
                .AsNoTracking()
                .Where(x => x.Id == receiptId)
                .Select(x => new
                {
                    x.Id,
                    x.HouseId,
                    x.UploadedByUserId,
                    x.ImageUrl,
                    x.RawOcrText,
                    x.StoreName,
                    x.ReceiptDate,
                    x.DetectedTotalAmount,
                    x.Status,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.ConvertedExpenseId,
                    Items = x.Items
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new
                        {
                            i.Id,
                            i.Name,
                            i.Price,
                            i.Quantity,
                            i.LineTotal,
                            i.BoxLeft,
                            i.BoxTop,
                            i.BoxWidth,
                            i.BoxHeight,
                            i.IsAssigned,
                            i.IsShared,
                            i.PersonalUserId,
                            i.SortOrder
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private void ApplyOcrResult(Receipt receipt, ReceiptOcrResult ocrResult, DateTime? fallbackReceiptDate = null)
        {
            receipt.RawOcrText = ocrResult.RawText;
            receipt.StoreName = string.IsNullOrWhiteSpace(ocrResult.StoreName) ? null : ocrResult.StoreName;
            receipt.ReceiptDate = ocrResult.ReceiptDate ?? fallbackReceiptDate ?? receipt.ReceiptDate ?? DateTime.UtcNow.AddHours(3).Date;
            receipt.DetectedTotalAmount = ocrResult.TotalAmount ?? (ocrResult.Items.Count > 0 ? ocrResult.Items.Sum(x => x.LineTotal) : receipt.DetectedTotalAmount);
            receipt.Status = ocrResult.Items.Count > 0 ? ReceiptStatus.Parsed : ReceiptStatus.Uploaded;
            receipt.UpdatedAt = DateTime.UtcNow;

            if (receipt.Items.Count > 0)
            {
                _dbContext.ReceiptItems.RemoveRange(receipt.Items);
                receipt.Items.Clear();
            }

            foreach (var item in ocrResult.Items.Select((value, index) => new ReceiptItem
            {
                Name = value.Name,
                Price = value.Price,
                Quantity = value.Quantity,
                LineTotal = value.LineTotal,
                BoxLeft = value.BoxLeft,
                BoxTop = value.BoxTop,
                BoxWidth = value.BoxWidth,
                BoxHeight = value.BoxHeight,
                IsAssigned = false,
                IsShared = true,
                SortOrder = index
            }))
            {
                receipt.Items.Add(item);
            }
        }
    }

    public class ReceiptScanRequest
    {
        public int HouseId { get; set; }
        public int UploadedByUserId { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateReceiptRequest
    {
        public string? StoreName { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public decimal? DetectedTotalAmount { get; set; }
        public List<UpdateReceiptItemRequest> Items { get; set; } = new();
    }

    public class UpdateReceiptItemRequest
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal LineTotal { get; set; }
        public int? BoxLeft { get; set; }
        public int? BoxTop { get; set; }
        public int? BoxWidth { get; set; }
        public int? BoxHeight { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsShared { get; set; } = true;
        public int? PersonalUserId { get; set; }
        public int SortOrder { get; set; }
    }

    public class ConvertReceiptToExpenseRequest
    {
        public int PayerUserId { get; set; }
        public int RecordedByUserId { get; set; }
        public string? Note { get; set; }
        public ExpenseCategory? Category { get; set; }
    }
}
