using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObririUssd.Data;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly UssdDataContext _context;

    public TransactionsController(UssdDataContext context)
    {
        _context = context;
    }

    // GET: api/transactions
    [HttpGet("get-transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        try
        {
            var transactions = await _context.Trans.ToListAsync();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}