using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Appointments.Admin;
using PetShop_Upgrade.DTOS.Appointments.Client;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private int memberId => int.Parse(User.Claims.First(c => c.Type == "id").Value);

        public AppointmentController(IAppointmentService appointmentService) => _appointmentService = appointmentService;

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDTO createAppointmentDTO)
        {
            var result = await _appointmentService.CreateAsync(memberId, createAppointmentDTO);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> GetAppointmentsByMemberId([FromQuery] AppointmentFilterDTO filter, int page = 1, int pageSize = 10)
        {
            var result = await _appointmentService.GetAppointmentsAsync(memberId, filter, page, pageSize);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{id}/self")]
        public async Task<IActionResult> GetAppointmentDetailByMemberId(int id)
        {
            var result = await _appointmentService.GetDetailAsync(memberId, id);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleAppointmentDTO rescheduleAppointmentDTO)
        {
            await _appointmentService.RescheduleAsync(memberId, id, rescheduleAppointmentDTO);
            return NoContent();
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelAppointmentDTO cancelAppointmentDTO)
        {
            await _appointmentService.CancelByCustomerAsync(memberId, id, cancelAppointmentDTO);
            return NoContent();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin")]
        public async Task<IActionResult> AdminGetAppointments([FromQuery] AppointmentFilterDTO filter, int page = 1, int pageSize = 10)
        {
            var result = await _appointmentService.AdminGetAppointmentsAsync(filter, page, pageSize);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> AdminGetApointmentDetail(int id)
        {
            var result = await _appointmentService.AdminGetDetailAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("admin/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDTO updateAppointmentDTO)
        {
            await _appointmentService.UpdateStatusAsync(id, updateAppointmentDTO);
            return NoContent();
        }
    }
}
