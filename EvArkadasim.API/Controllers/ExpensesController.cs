using Application.Features.Expenses.Commands.CreateExpense;
using Application.Features.Expenses.Commands.CreateIrregularExpense;
using Application.Features.Expenses.Commands.DeleteExpense;
using Application.Features.Expenses.Commands.UpdateExpense;
using Application.Features.Expenses.Dtos;
using Application.Features.Expenses.Queries.GetExpense;
using Application.Features.Expenses.Queries.GetExpensesByHouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExpensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command)
        {
            CreatedExpenseResponseDto result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("AddExpense")]
        public async Task<IActionResult> AddExpense([FromBody] CreateExpenseCommand command)
        {
            CreatedExpenseResponseDto result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetExpenses/{houseId}")]
        public async Task<IActionResult> GetExpenses(int houseId)
        {
            var data = await _mediator.Send(new GetExpensesByHouseQuery { HouseId = houseId });
            return Ok(new { isSuccess = true, data });
        }

        [HttpGet("GetExpense/{expenseId}")]
        public async Task<IActionResult> GetExpense(int expenseId)
        {
            var data = await _mediator.Send(new GetExpenseQuery { ExpenseId = expenseId });
            return Ok(new { isSuccess = true, data });
        }

        [HttpDelete("DeleteExpense/{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            await _mediator.Send(new DeleteExpenseCommand { ExpenseId = expenseId });
            return Ok(new { isSuccess = true, message = "Harcama başarıyla silindi." });
        }

        [HttpPut("UpdateExpense/{expenseId}")]
        public async Task<IActionResult> UpdateExpense(int expenseId, [FromBody] UpdateExpenseDto dto)
        {
            await _mediator.Send(new UpdateExpenseCommand
            {
                ExpenseId = expenseId,
                Dto = dto
            });
            return Ok(new { isSuccess = true, message = "Harcama başarıyla güncellendi." });
        }

        [HttpPost("CreateIrregular")]
        public async Task<IActionResult> CreateIrregular([FromBody] CreateIrregularExpenseRequest model)
        {
            var result = await _mediator.Send(new CreateIrregularExpenseCommand { Model = model });
            return Ok(result);
        }
    }
}
