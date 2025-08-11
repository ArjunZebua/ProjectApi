using API.Model;
using API.Model.DB;
using API.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class SupplierService
    {
        private readonly ApplicationContext _context;

        public SupplierService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> AddSupplierAsync(SupplierDTO supplierDTO)
        {
            try
            {
                var supplier = new Supplier
                {
                    CompanyName = supplierDTO.CompanyName,
                    ContactPerson = supplierDTO.ContactPerson,
                    Email = supplierDTO.Email,
                    Phone = supplierDTO.Phone,
                    Address = supplierDTO.Address,
                    City = supplierDTO.City,
                    IsActive = supplierDTO.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<SupplierDTO>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .Select(s => new SupplierDTO
                {
                    Id = s.Id,
                    CompanyName = s.CompanyName,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    City = s.City,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    ProductCount = s.Products.Count
                })
                .ToListAsync();
        }

        public async Task<SupplierDTO?> GetSupplierByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null) return null;

            return new SupplierDTO
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                ProductCount = supplier.Products.Count
            };
        }

        public async Task<bool> UpdateSupplierAsync(int id, SupplierDTO supplierDTO)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null) return false;

                supplier.CompanyName = supplierDTO.CompanyName;
                supplier.ContactPerson = supplierDTO.ContactPerson;
                supplier.Email = supplierDTO.Email;
                supplier.Phone = supplierDTO.Phone;
                supplier.Address = supplierDTO.Address;
                supplier.City = supplierDTO.City;
                supplier.IsActive = supplierDTO.IsActive;
                supplier.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null) return false;

                _context.Suppliers.Remove(supplier);
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