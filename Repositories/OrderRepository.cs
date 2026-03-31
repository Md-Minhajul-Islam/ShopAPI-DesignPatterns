using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Models;

namespace ShopAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Product) // joins Product table
            .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Order order)
        {
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
    }
}