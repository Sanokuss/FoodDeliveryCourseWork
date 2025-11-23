using CourseWork.Models;

namespace CourseWork.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order order);
        void UpdateStatus(int orderId, string status);
    }
}

