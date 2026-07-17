using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Appointments.Client;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task PetScheduleLockAsync(int productId);
        Task<bool> HasConflictAsync(int productId, DateTimeOffset startAt, DateTimeOffset endAt, int? excludedAppointmentId = null);
        Task<Appointment?> GetDetailAsync(int appointmentId);
        Task<PagedData<Appointment>> GetByFilterAsync(AppointmentFilterDTO appointmentFilterDTO, int? memberId, int page, int pageSize);
    }
}
