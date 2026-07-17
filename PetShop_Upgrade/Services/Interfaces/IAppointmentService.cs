using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Appointments.Admin;
using PetShop_Upgrade.DTOS.Appointments.Client;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AdminAppointmentDetailDTO> CreateAsync(int memberId, CreateAppointmentDTO createAppointmentDTO);
        Task<AppointmentDetailDTO> GetDetailAsync(int memberId, int appointmentId);
        Task<AdminAppointmentDetailDTO> AdminGetDetailAsync(int appointmentId);
        Task<PagedData<AppointmentItemDTO>> GetAppointmentsAsync(int memberId, AppointmentFilterDTO appointmentFilterDTO, int page, int pageSize);
        Task<PagedData<AdminAppointmentItemDTO>> AdminGetAppointmentsAsync(AppointmentFilterDTO appointmentFilterDTO, int page, int pageSize);
        Task RescheduleAsync(int memberId, int appointmentId, RescheduleAppointmentDTO rescheduleAppointmentDTO);
        Task CancelByCustomerAsync(int memberId, int appointmentId, CancelAppointmentDTO cancelAppointmentDTO);
        Task UpdateStatusAsync(int appointmentId, UpdateAppointmentStatusDTO updateAppointmentDTO);
    }
}
