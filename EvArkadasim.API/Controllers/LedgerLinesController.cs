using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.LedgerLines.Dtos;
using Application.Features.LedgerLines.Queries.GetLedgerLinesByExpense;
using Application.Features.LedgerLines.Queries.GetLedgerLinesByHouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>Ledger satırlarını okuma amaçlı uçlar.</summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LedgerLinesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LedgerLinesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Belirli harcamaya ait ledger satırları</summary>
        [HttpGet("ByExpense/{expenseId}")]
        public async Task<IActionResult> ByExpense(int expenseId)
        {
            List<LedgerLineDto> data = await _mediator.Send(new GetLedgerLinesByExpenseQuery { ExpenseId = expenseId });
            return Ok(new { isSuccess = true, data });
        }

        /// <summary>Belirli eve (aktif) ait ledger satırları</summary>
        [HttpGet("ByHouse/{houseId}")]
        public async Task<IActionResult> ByHouse(int houseId)
        {
            List<LedgerLineDto> data = await _mediator.Send(new GetLedgerLinesByHouseQuery { HouseId = houseId });
            return Ok(new { isSuccess = true, data });
        }
    }
}
