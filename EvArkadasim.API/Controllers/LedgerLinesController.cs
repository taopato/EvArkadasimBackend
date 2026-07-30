using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.LedgerLines.Dtos;
using Application.Features.LedgerLines.Queries.GetLedgerLinesByExpense;
using Application.Features.LedgerLines.Queries.GetLedgerLinesByHouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Services.Repositories;

namespace WebAPI.Controllers
{
    /// <summary>Ledger satırlarını okuma amaçlı uçlar.</summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LedgerLinesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHouseRepository _houseRepository;
        private readonly IExpenseRepository _expenseRepository;

        public LedgerLinesController(
            IMediator mediator,
            IHouseRepository houseRepository,
            IExpenseRepository expenseRepository)
        {
            _mediator = mediator;
            _houseRepository = houseRepository;
            _expenseRepository = expenseRepository;
        }

        /// <summary>Belirli harcamaya ait ledger satırları</summary>
        [HttpGet("ByExpense/{expenseId}")]
        public async Task<IActionResult> ByExpense(int expenseId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized();
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense is null)
                return NotFound();
            if (!await _houseRepository.IsActiveMemberAsync(expense.HouseId, currentUserId.Value))
                return Forbid();

            List<LedgerLineDto> data = await _mediator.Send(new GetLedgerLinesByExpenseQuery { ExpenseId = expenseId });
            return Ok(new { isSuccess = true, data });
        }

        /// <summary>Belirli eve (aktif) ait ledger satırları</summary>
        [HttpGet("ByHouse/{houseId}")]
        public async Task<IActionResult> ByHouse(int houseId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId is null)
                return Unauthorized();
            if (!await _houseRepository.IsActiveMemberAsync(houseId, currentUserId.Value))
                return Forbid();

            List<LedgerLineDto> data = await _mediator.Send(new GetLedgerLinesByHouseQuery { HouseId = houseId });
            return Ok(new { isSuccess = true, data });
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            return int.TryParse(value, out var userId) && userId > 0 ? userId : null;
        }
    }
}
