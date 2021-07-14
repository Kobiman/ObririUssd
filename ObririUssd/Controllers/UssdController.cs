using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObririUssd.Data;
using ObririUssd.Models;
using ObririUssd.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ObririUssd.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
    {
        private IUssdService _ussdService;
        public UssdController(IUssdService ussdService)
        {
            _ussdService = ussdService;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] UssdRequest request)
        {
            var result = await _ussdService.ProcessRequest(request);
            return Ok(result);
        }

        //[HttpPost("recieve-payment`")]
        //public async Task<IActionResult> RecievePayment([FromBody] PaymentRequest request)
        //{
        //    await new MessageService().SendSms(request.Data.customer.phone_number, $"Succus");
        //    return Ok();
        //}
    }
}
