using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

using financesApi.services;
using financesApi.models;
using financesApi.utilities;
using System.Runtime;


namespace financesApi.controllers
{
    [ApiController]
    [Route("[controller]")]
    public class uploadsController : ControllerBase
    {
        [HttpPost("newAccount")]
        public async Task<IActionResult> newAccount(NewAccountRequest accountParameters)
        {
            return Ok();
        }

        [HttpGet("getAccounts")]
        public async Task<IActionResult> getAccounts()
        {
            return Ok();
        }

        [HttpPost("uploadTransactions")]
        public async Task<IActionResult> uploadTransactions()
        {
            return Ok();
        }
    }
}