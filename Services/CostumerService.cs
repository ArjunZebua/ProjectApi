using API.Model;
using API.Model.DB;
using API.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class CustomerService
    {
        private readonly ApplicationContext _context;

        public CustomerService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> AddCustomerAsync(CustomerDTO customerDTO)
        {
            try
            {
                var customer = new Customer
                {
                    FirstName = customerDTO.FirstName,
                    LastName = customerDTO.LastName,
                    Email = customerDTO.Email,
                    Phone = customerDTO.Phone,
                    Address = customerDTO.Address,
                    City = customerDTO.City,
                    PostalCode = customerDTO.PostalCode,
                    IsActive = customerDTO.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDTO)
        {
            var customer = new Customer
            {
                FirstName = customerDTO.FirstName,
                LastName = customerDTO.LastName,
                Email = customerDTO.Email,
                Phone = customerDTO.Phone,
                Address = customerDTO.Address,
                City = customerDTO.City,
                PostalCode = customerDTO.PostalCode,
                IsActive = customerDTO.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return new CustomerDTO
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                PostalCode = customer.PostalCode,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }

        public async Task<List<CustomerDTO>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Select(c => new CustomerDTO
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    City = c.City,
                    PostalCode = c.PostalCode,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    TotalOrders = c.Orders.Count,
                    TotalSpent = c.Orders.Sum(o => o.TotalAmount)
                })
                .ToListAsync();
        }

        public async Task<CustomerDTO?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return null;

            return new CustomerDTO
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                PostalCode = customer.PostalCode,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                TotalOrders = customer.Orders.Count,
                TotalSpent = customer.Orders.Sum(o => o.TotalAmount)
            };
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerDTO customerDTO)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                customer.FirstName = customerDTO.FirstName;
                customer.LastName = customerDTO.LastName;
                customer.Email = customerDTO.Email;
                customer.Phone = customerDTO.Phone;
                customer.Address = customerDTO.Address;
                customer.City = customerDTO.City;
                customer.PostalCode = customerDTO.PostalCode;
                customer.IsActive = customerDTO.IsActive;
                customer.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // HAPUS METHOD INI - sudah dihapus
        // public async Task<bool> UpdateCustomerAsync(CustomerDTO customerDTO)
        // {
        //     return await UpdateCustomerAsync(customerDTO.Id, customerDTO);
        // }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}