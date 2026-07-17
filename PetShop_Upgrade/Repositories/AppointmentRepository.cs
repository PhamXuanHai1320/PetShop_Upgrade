using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Appointments;
using PetShop_Upgrade.DTOS.Appointments.Client;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task PetScheduleLockAsync(int productId)
        {
            if (_context.Database.CurrentTransaction is null)
                throw new InvalidOperationException("Application lock phải được lấy bên trong transaction.");

            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.Transaction = _context.Database.CurrentTransaction.GetDbTransaction();
            command.CommandText = @"
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock
                    @Resource = @resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 10000;
                SELECT @result;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@resource";
            parameter.Value = $"appointment:pet:{productId}";
            command.Parameters.Add(parameter);

            var result = Convert.ToInt32(await command.ExecuteScalarAsync());
            if (result < 0)
                throw new InvalidOperationException("Không thể khóa lịch của thú cưng. Vui lòng thử lại.");
        }

        public Task<bool> HasConflictAsync(
            int productId, 
            DateTimeOffset startAt, 
            DateTimeOffset endAt, 
            int? excludedAppointmentId = null)
        {
            return _context.PetViewingAppointments.AnyAsync(x =>
                x.ProductId == productId &&
                (!excludedAppointmentId.HasValue || x.AppointmentId != excludedAppointmentId.Value) &&
                (x.Appointment.Status == AppointmentStatus.PENDING || x.Appointment.Status == AppointmentStatus.CONFIRMED) &&
                startAt < x.Appointment.EndAt && endAt > x.Appointment.StartAt);
        }

        public async Task<Appointment?> GetDetailAsync(int appointmentId)
        {
            return await _context.Appointments
            .Include(x => x.Member)
            .Include(x => x.PetViewingAppointment)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.ProductImages)
            .Include(x => x.PetViewingAppointment)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.ProductColors)
                        .ThenInclude(x => x.Color)
            .FirstOrDefaultAsync(x => x.Id == appointmentId);
        }

        public async Task<PagedData<Appointment>> GetByFilterAsync(AppointmentFilterDTO appointmentFilterDTO, int? memberId, int page, int pageSize)
        {
            var query = _context.Appointments.AsNoTracking()
                .Include(x => x.Member)
                .Include(x => x.PetViewingAppointment).ThenInclude(x => x.Product)
                .AsQueryable();
            if (memberId.HasValue) 
                query = query.Where(x => x.MemberId == memberId.Value);
            if (appointmentFilterDTO.Status.HasValue) 
                query = query.Where(x => x.Status == appointmentFilterDTO.Status.Value);
            if (appointmentFilterDTO.From.HasValue) 
                query = query.Where(x => x.StartAt >= appointmentFilterDTO.From.Value);
            if (appointmentFilterDTO.To.HasValue) 
                query = query.Where(x => x.StartAt <= appointmentFilterDTO.To.Value);
            if (appointmentFilterDTO.ProductId.HasValue) 
                query = query.Where(x => x.PetViewingAppointment.ProductId == appointmentFilterDTO.ProductId.Value);

            var totalItems = await query.CountAsync();

            var items = await query.OrderByDescending(x => x.StartAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedData<Appointment> 
            { 
                Items = items, 
                TotalItems = totalItems 
            };
        }
    }
}
