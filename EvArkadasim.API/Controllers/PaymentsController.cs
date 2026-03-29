// EvArkadasim.API/Controllers/PaymentsController.cs
using System;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Application.Features.Payments.Commands.CreatePayment;
using Application.Features.Payments.Queries.GetPaymentList;
using Application.Features.Payments.Queries.GetPendingPayments;
using Application.Features.Payments.Commands.ApprovePayment;
using Application.Features.Payments.Commands.RejectPayment;
using Domain.Enums;
using Application.Features.Payments.Commands.AddPaymentWithAllocations;

namespace EvArkadasim.API.Controllers
{
    // İstersen ayrı dosyaya taşıyabilirsin; burada kalması da sorun değil
    public class CreatePaymentForm
    {
        public int HouseId { get; set; }
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public decimal Tutar { get; set; }

        // "Cash" | "BankTransfer" (varsayılan BankTransfer)
        public string? PaymentMethod { get; set; } = "BankTransfer";

        // BankTransfer’de zorunlu, Cash’te opsiyonel
        public IFormFile? Dekont { get; set; }

        public DateTime? OdemeTarihi { get; set; }
        public string? Aciklama { get; set; }

        // 🔽 yeni: katkının hangi cycle için olduğu (opsiyonel bıraktım ama handler’da doğrulamanı öneririm)
        public int? ChargeId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _env;

        public PaymentsController(IMediator mediator, IWebHostEnvironment env)
        {
            _mediator = mediator;
            _env = env;
        }

        // POST: /api/Payments/CreatePayment  (multipart/form-data)
        // Cash -> dekont zorunlu değil
        // BankTransfer -> dekont ZORUNLU
        [HttpPost("CreatePayment")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreatePayment([FromForm] CreatePaymentForm form)
        {
            var pmRaw = (form.PaymentMethod ?? "BankTransfer").Trim();
            var isCash = pmRaw.Equals("Cash", StringComparison.OrdinalIgnoreCase);
            var isTransfer = pmRaw.Equals("BankTransfer", StringComparison.OrdinalIgnoreCase);

            if (!isCash && !isTransfer)
                return BadRequest("paymentMethod geçersiz. 'Cash' veya 'BankTransfer' olmalı.");

            if (isTransfer && form.Dekont == null)
                return BadRequest("BankTransfer için dekont zorunludur.");

            string dekontUrl = "";
            if (form.Dekont != null)
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsRoot = Path.Combine(webRoot, "uploads", "payments");
                Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(form.Dekont.FileName)}";
                var physicalPath = Path.Combine(uploadsRoot, fileName);
                await using (var fs = System.IO.File.Create(physicalPath))
                    await form.Dekont.CopyToAsync(fs);

                dekontUrl = $"/uploads/payments/{fileName}";
            }

            var cmd = new CreatePaymentCommand
            {
                HouseId = form.HouseId,
                BorcluUserId = form.BorcluUserId,
                AlacakliUserId = form.AlacakliUserId,
                Tutar = form.Tutar,
                PaymentMethod = isCash ? PaymentMethod.Cash : PaymentMethod.BankTransfer,
                OdemeTarihi = form.OdemeTarihi ?? DateTime.UtcNow,
                Aciklama = form.Aciklama,
                DekontUrl = dekontUrl,

                // 🔽 yeni: chargeId’yi komuta geçir
                ChargeId = form.ChargeId
            };

            var result = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(CreatePayment), new { id = result.Id }, result);
        }

        // GET: /api/Payments/GetPayments/{houseId}
        [HttpGet("GetPayments/{houseId}")]
        public async Task<IActionResult> GetPayments(int houseId)
        {
            var list = await _mediator.Send(new GetPaymentListQuery { HouseId = houseId });
            return Ok(list);
        }

        // GET: /api/Payments/GetPendingPayments/{userId}
        [HttpGet("GetPendingPayments/{userId:int}")]
        public async Task<IActionResult> GetPendingPayments([FromRoute] int userId)
        {
            var result = await _mediator.Send(new GetPendingPaymentsQuery(userId));
            return Ok(result);
        }

        // POST: /api/Payments/ApprovePayment/{paymentId}
        [HttpPost("ApprovePayment/{paymentId:int}")]
        public async Task<IActionResult> ApprovePayment([FromRoute] int paymentId)
        {
            var result = await _mediator.Send(new ApprovePaymentCommand(paymentId));
            return Ok(new
            {
                success = true,
                message = "Ödeme başarıyla onaylandı",
                data = result
            });
        }

        // POST: /api/Payments/RejectPayment/{paymentId}
        [HttpPost("RejectPayment/{paymentId:int}")]
        public async Task<IActionResult> RejectPayment([FromRoute] int paymentId)
        {
            var result = await _mediator.Send(new RejectPaymentCommand(paymentId));
            return Ok(new
            {
                success = true,
                message = "Ödeme reddedildi",
                data = result
            });
        }

        [HttpPost("AddPaymentWithAllocations")]
        public async Task<IActionResult> AddPaymentWithAllocations([FromBody] AddPaymentWithAllocationsDto dto)
        {
            var res = await _mediator.Send(new AddPaymentWithAllocationsCommand { Model = dto });
            return Ok(res);
        }
    }
}
