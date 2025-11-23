using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Order order)
        {
            _db.Orders.Update(order);
        }

        public void UpdateStatus(int orderId, string status)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                _db.Orders.Update(order);
            }
        }
    }
}

