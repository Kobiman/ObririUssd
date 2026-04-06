using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObririUssd.Data;
using ObririUssd.Models;

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

    // POST: api/transactions/update-lock
    [HttpPost("update-lock")]
    public async Task<IActionResult> UpdateUssdLock( UssdLock ussdLock)
    {
        if (ussdLock == null)
        {
            return BadRequest("Invalid UssdLock data or missing ID.");
        }

        try
        {
            // Assign a random 8-digit ticket number
            var random = new Random();
            ussdLock.Game_TicketNo = random.Next(10000000, 100000000).ToString();
           ussdLock.StartTime =  ussdLock.StartTime;
           ussdLock.EndTime = ussdLock.EndTime;
           ussdLock.GameType = ussdLock.GameType;
           ussdLock.Disabled = ussdLock.Disabled;

           _context.UssdLock.Attach(ussdLock);
          //MAP ALL FIELDS
           _context.Entry(ussdLock).Property(x => x.StartTime).IsModified = true;
            _context.Entry(ussdLock).Property(x => x.EndTime).IsModified = true;
             _context.Entry(ussdLock).Property(x => x.GameType).IsModified = true;
               _context.Entry(ussdLock).Property(x => x.Game_TicketNo).IsModified = true;
          _context.Entry(ussdLock).Property(x => x.Disabled).IsModified = true;

           //_context.UssdLock.Update(ussdLock);
           await _context.SaveChangesAsync();

            return Ok(ussdLock);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/transactions/get-locks
    [HttpGet("get-locks")]
    public async Task<IActionResult> GetUssdLocks()
    {
        try
        {
            var ussdLocks = await _context.UssdLock.ToListAsync();
            return Ok(ussdLocks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}