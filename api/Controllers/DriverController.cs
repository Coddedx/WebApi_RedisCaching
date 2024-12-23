using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/driver")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ApplicationDBContext _context;
        public DriverController(ICacheService cacheService, ApplicationDBContext context)
        {
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetAllDrivers() 
        {
            var cachedData = _cacheService.GetData<IEnumerable<Driver>>("drivers"); //"drivers" is the key!! when you call drivers key its gonna get all the lis of driver 
            if(cachedData != null && cachedData.Count() > 0) return Ok(cachedData);

            cachedData = await _context.Drivers.ToListAsync();

            var expiryTime = DateTimeOffset.Now.AddSeconds(30); //AddMinutes!!!

            _cacheService.SetData<IEnumerable<Driver>>("drivers", cachedData, expiryTime);

            return Ok(cachedData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver value)
        {
            // var addedObj = await _context.Drivers.AddAsync(value);

            // var expiryTime = DateTimeOffset.Now.AddSeconds(30); //AddMinutes!!!

            // _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiryTime);

            // await _context.SaveChangesAsync();

            // return Ok(addedObj.Entity);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Drivers ON");
                var addedObj = await _context.Drivers.AddAsync(value);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Drivers OFF");
                await transaction.CommitAsync();

                var expiryTime = DateTimeOffset.Now.AddSeconds(30);
                _cacheService.SetData($"driver{value.Id}", addedObj.Entity, expiryTime);

                return Ok(addedObj.Entity);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        } 


        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _context.Drivers.FirstOrDefaultAsync(a => a.Id == id);

            if(exists != null)
            {
                _context.Remove(exists);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return NoContent();
        }
    
        [HttpGet("test")]
        public IActionResult TestCache()
        {
            // Veri ekleme
            var isSet = _cacheService.SetData("TestKeyRedis", "TestValue", DateTimeOffset.Now.AddMinutes(15));

            // Veri okuma
            var value = _cacheService.GetData<string>("TestKeyRedis");

            return Ok(new
            {
                Success = isSet,
                CachedValue = value
            });
        }
    }
}