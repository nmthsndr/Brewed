using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;

namespace Brewed.Services
{
    public interface IAddressService
    {
        Task<List<AddressDto>> GetUserAddressesAsync(int userId);
        Task<AddressDto> GetAddressByIdAsync(int addressId, int userId);
        Task<AddressDto> CreateAddressAsync(int userId, AddressCreateDto addressDto);
        Task<AddressDto> UpdateAddressAsync(int addressId, int userId, AddressCreateDto addressDto);
        Task<bool> DeleteAddressAsync(int addressId, int userId);
        Task<AddressDto> SetDefaultAddressAsync(int addressId, int userId);
    }

    public class AddressService : IAddressService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public AddressService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<AddressDto>> GetUserAddressesAsync(int userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();

            return _mapper.Map<List<AddressDto>>(addresses);
        }

        public async Task<AddressDto> GetAddressByIdAsync(int addressId, int userId)
        {
            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found");
            }

            if (address.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this address");
            }

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> CreateAddressAsync(int userId, AddressCreateDto addressDto)
        {
            // If this is set as default, unset other defaults
            if (addressDto.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                    _context.Addresses.Update(addr);
                }
            }

            var address = new Address
            {
                UserId = userId,
                FirstName = addressDto.FirstName,
                LastName = addressDto.LastName,
                AddressLine1 = addressDto.AddressLine1,
                AddressLine2 = addressDto.AddressLine2,
                City = addressDto.City,
                PostalCode = addressDto.PostalCode,
                Country = addressDto.Country,
                PhoneNumber = addressDto.PhoneNumber,
                IsDefault = addressDto.IsDefault,
                AddressType = addressDto.AddressType
            };

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAddressAsync(int addressId, int userId, AddressCreateDto addressDto)
        {
            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found");
            }

            if (address.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this address");
            }

            // If this is being set as default, unset other defaults
            if (addressDto.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId)
                    .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                    _context.Addresses.Update(addr);
                }
            }

            address.FirstName = addressDto.FirstName;
            address.LastName = addressDto.LastName;
            address.AddressLine1 = addressDto.AddressLine1;
            address.AddressLine2 = addressDto.AddressLine2;
            address.City = addressDto.City;
            address.PostalCode = addressDto.PostalCode;
            address.Country = addressDto.Country;
            address.PhoneNumber = addressDto.PhoneNumber;
            address.IsDefault = addressDto.IsDefault;
            address.AddressType = addressDto.AddressType;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<bool> DeleteAddressAsync(int addressId, int userId)
        {
            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found");
            }

            if (address.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this address");
            }

            // Check if address is used in any orders
            var isUsedInOrders = await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);

            if (isUsedInOrders)
            {
                throw new Exception("Cannot delete address that is used in existing orders");
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AddressDto> SetDefaultAddressAsync(int addressId, int userId)
        {
            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found");
            }

            if (address.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this address");
            }

            // Unset all other defaults
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
                _context.Addresses.Update(addr);
            }

            // Set this as default
            address.IsDefault = true;
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }
    }
}