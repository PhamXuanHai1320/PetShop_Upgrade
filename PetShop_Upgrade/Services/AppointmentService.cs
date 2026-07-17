using AutoMapper;
using Microsoft.Extensions.Options;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Appointments;
using PetShop_Upgrade.DTOS.Appointments.Admin;
using PetShop_Upgrade.DTOS.Appointments.Client;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Options;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppointmentOptions _options;
        private readonly TimeZoneInfo _timeZone;
        private readonly IMapper _mapper;

        public AppointmentService(IUnitOfWork unitOfWork, IOptions<AppointmentOptions> options, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
            _timeZone = ResolveTimeZone(_options.TimeZoneId);
            _mapper = mapper;
        }

        // hàm lấy thời gian hiện tại theo múi giờ của cửa hàng
        private DateTimeOffset Now() => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZone);
        // hàm resolve múi giờ từ id, nếu không tìm thấy thì mặc định là Asia
        private static TimeZoneInfo ResolveTimeZone(string id)
        {
            try { 
                return TimeZoneInfo.FindSystemTimeZoneById(id); 
            } catch (TimeZoneNotFoundException) 
            { 
                return TimeZoneInfo.FindSystemTimeZoneById("Asia"); 
            }
        }
        public async Task<AdminAppointmentDetailDTO> CreateAsync(int memberId, CreateAppointmentDTO createAppointmentDTO)
        {
            var (startAt, endAt) = ValidateSchedule(createAppointmentDTO.StartAt);
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.AppointmentRepository.PetScheduleLockAsync(createAppointmentDTO.ProductId);
                var product = await ValidatePetAsync(createAppointmentDTO.ProductId, createAppointmentDTO.ProductColorId);
                if (await _unitOfWork.AppointmentRepository.HasConflictAsync(createAppointmentDTO.ProductId, startAt, endAt))
                    throw new ConflictException("Thú cưng đã có lịch xem trong khung giờ này.");

                var now = Now();
                var appointment = new Appointment
                {
                    StartAt = startAt,
                    EndAt = endAt,
                    Status = AppointmentStatus.PENDING,
                    CustomerNotes = createAppointmentDTO.Notes?.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now,
                    MemberId = memberId,
                    PetViewingAppointment = new PetViewingAppointment
                    {
                        ProductId = product.Id,
                        ProductColorId = createAppointmentDTO.ProductColorId
                    }
                };
                _unitOfWork.AppointmentRepository.Add(appointment);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return await AdminGetDetailAsync(appointment.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RescheduleAsync(int memberId, int appointmentId, RescheduleAppointmentDTO rescheduleAppointmentDTO)
        {
            var (startAt, endAt) = ValidateSchedule(rescheduleAppointmentDTO.StartAt);
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var appointment = await GetOwnedAppointmentAsync(memberId, appointmentId);
                if (appointment.Status is not AppointmentStatus.PENDING and not AppointmentStatus.CONFIRMED)
                    throw new ConflictException("Trạng thái hiện tại không cho phép thay đổi lịch.");

                var productId = appointment.PetViewingAppointment.ProductId;
                await _unitOfWork.AppointmentRepository.PetScheduleLockAsync(productId);
                await ValidatePetAsync(productId, appointment.PetViewingAppointment.ProductColorId);

                if (await _unitOfWork.AppointmentRepository.HasConflictAsync(productId, startAt, endAt, appointmentId))
                    throw new ConflictException("Thú cưng đã có lịch xem trong khung giờ này.");

                appointment.StartAt = startAt;
                appointment.EndAt = endAt;
                appointment.Status = AppointmentStatus.PENDING;
                appointment.ConfirmedAt = null;
                appointment.UpdatedAt = Now();
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelByCustomerAsync(int memberId, int appointmentId, CancelAppointmentDTO cancelAppointmentDTO)
        {
            var appointment = await GetOwnedAppointmentAsync(memberId, appointmentId);
            if (appointment.Status is not AppointmentStatus.PENDING and not AppointmentStatus.CONFIRMED)
                throw new ConflictException("Trạng thái hiện tại không cho phép thay đổi lịch.");
            if (appointment.StartAt <= Now())
                throw new ConflictException("Không thể hủy lịch đã bắt đầu hoặc đã qua.");
            appointment.Status = AppointmentStatus.CANCELLED;
            appointment.CancellationReason = cancelAppointmentDTO.Reason.Trim();
            appointment.CancelledAt = Now();
            appointment.UpdatedAt = Now();
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int appointmentId, UpdateAppointmentStatusDTO updateAppointmentDTO)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetDetailAsync(appointmentId);
            if (appointment == null)
                throw new NotFoundException("Không tìm thấy lịch hẹn.");
            var now = Now();
            switch (appointment.Status, updateAppointmentDTO.Status)
            {
                case (AppointmentStatus.PENDING, AppointmentStatus.CONFIRMED):
                    if (appointment.StartAt <= now) 
                        throw new ConflictException("Không thể xác nhận lịch đã bắt đầu hoặc đã qua.");
                    appointment.ConfirmedAt = now;
                    break;
                case (AppointmentStatus.PENDING, AppointmentStatus.REJECTED):
                    if (string.IsNullOrWhiteSpace(updateAppointmentDTO.Reason))
                        throw new BadRequestException("Vui lòng nhập lý do.");
                    appointment.CancellationReason = updateAppointmentDTO.Reason!.Trim();
                    appointment.CancelledAt = now;
                    break;
                case (AppointmentStatus.PENDING, AppointmentStatus.CANCELLED):
                case (AppointmentStatus.CONFIRMED, AppointmentStatus.CANCELLED):
                    if (string.IsNullOrWhiteSpace(updateAppointmentDTO.Reason)) 
                        throw new BadRequestException("Vui lòng nhập lý do.");
                    appointment.CancellationReason = updateAppointmentDTO.Reason!.Trim();
                    appointment.CancelledAt = now;
                    break;
                case (AppointmentStatus.CONFIRMED, AppointmentStatus.COMPLETED):
                    if (now < appointment.StartAt) 
                        throw new ConflictException("Không thể hoàn thành lịch trước giờ hẹn.");
                    appointment.CompletedAt = now;
                    break;
                case (AppointmentStatus.CONFIRMED, AppointmentStatus.NO_SHOW):
                    if (now < appointment.EndAt) 
                        throw new ConflictException("Chỉ có thể đánh dấu không đến sau khi buổi hẹn kết thúc.");
                    break;
                default:
                    throw new ConflictException($"Không thể chuyển trạng thái từ {appointment.Status} sang {updateAppointmentDTO.Status}.");
            }
            appointment.Status = updateAppointmentDTO.Status;
            appointment.StaffNotes = updateAppointmentDTO.StaffNotes?.Trim();
            appointment.UpdatedAt = now;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AppointmentDetailDTO> GetDetailAsync(int memberId, int appointmentId)
        {
            var appointment = await GetOwnedAppointmentAsync(memberId, appointmentId);
            if (appointment == null)
                throw new NotFoundException("Không tìm thấy lịch hẹn.");
            return _mapper.Map<AppointmentDetailDTO>(appointment);
        }

        public async Task<AdminAppointmentDetailDTO> AdminGetDetailAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetDetailAsync(appointmentId);
            if (appointment == null)
                throw new NotFoundException("Không tìm thấy lịch hẹn.");
            return _mapper.Map<AdminAppointmentDetailDTO>(appointment);
        }

        public async Task<PagedData<AppointmentItemDTO>> GetAppointmentsAsync(
            int memberId, 
            AppointmentFilterDTO appointmentFilterDTO, 
            int page, 
            int pageSize)
        {
            var result = await _unitOfWork.AppointmentRepository.GetByFilterAsync(appointmentFilterDTO, memberId, page, pageSize);
            return new PagedData<AppointmentItemDTO> 
            { 
                Items = _mapper.Map<IEnumerable<AppointmentItemDTO>>(result.Items),
                TotalItems = result.TotalItems 
            };
        }

        public async Task<PagedData<AdminAppointmentItemDTO>> AdminGetAppointmentsAsync(
            AppointmentFilterDTO appointmentFilterDTO, 
            int page, 
            int pageSize)
        {
            var result = await _unitOfWork.AppointmentRepository.GetByFilterAsync(appointmentFilterDTO, null, page, pageSize);
            return new PagedData<AdminAppointmentItemDTO>
            {
                Items = _mapper.Map<IEnumerable<AdminAppointmentItemDTO>>(result.Items),
                TotalItems = result.TotalItems
            };
        }
        // hàm validate thú cưng - kiểm tra các điều kiện tồn tại, Pet Category, Active, màu, số lượng
        private async Task<Product> ValidatePetAsync(int productId, int productColorId)
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Không tìm thấy thú cưng.");
            if (product.Type != ProductType.Pet) 
                throw new BadRequestException("Sản phẩm không phải thú cưng.");
            if (product.IsActive != IsActive.ACTIVE)
                throw new ConflictException("Thú cưng đang bị khóa hoặc không được phép đặt xem.");

            var productColor = product.ProductColors.FirstOrDefault(x => x.Id == productColorId);
            if (productColor is null) 
                throw new BadRequestException("Màu không thuộc thú cưng đã chọn.");
            if (productColor.Quantity <= 0) 
                throw new ConflictException("Thú cưng hiện đã hết số lượng.");
            return product;
        }
        // hàm validate lịch hẹn, trả về thời gian bắt đầu và kết thúc đã được chuyển sang múi giờ của cửa hàng
        private (DateTimeOffset StartAt, DateTimeOffset EndAt) ValidateSchedule(DateTimeOffset requestedStart)
        {
            var startAt = TimeZoneInfo.ConvertTime(requestedStart, _timeZone);
            var endAt = startAt.AddMinutes(_options.SlotDurationMinutes);
            if (startAt <= Now()) 
                throw new BadRequestException("Không được đặt lịch trong quá khứ.");
            if (startAt.DayOfWeek == DayOfWeek.Sunday) 
                throw new BadRequestException("Cửa hàng không nhận lịch vào Chủ nhật.");
            if (startAt.TimeOfDay < _options.OpeningTime || endAt.TimeOfDay > _options.ClosingTime)
                throw new BadRequestException("Lịch hẹn phải nằm hoàn toàn trong giờ làm việc 08:00-18:00.");
            return (startAt, endAt);
        }
        // hàm lấy lịch hẹn theo memberId và appointmentId
        private async Task<Appointment> GetOwnedAppointmentAsync(int memberId, int id)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetDetailAsync(id);
            if (appointment == null) 
                throw new NotFoundException("Không tìm thấy lịch hẹn.");
            if (appointment.MemberId != memberId) 
                throw new ForbiddenException("Bạn không có quyền truy cập lịch hẹn này.");
            return appointment;
        }
    }
}
