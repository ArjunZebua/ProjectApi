using Microsoft.AspNetCore.Mvc;
using API.Model.DTO;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
       
        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set ID dari URL parameter (hapus validasi mismatch)
            customerDto.Id = id;

            var result = await _customerService.UpdateCustomerAsync(id, customerDto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Customer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}