using Microsoft.AspNetCore.Identity;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly UserManager<Member> _userManager;
        public MemberService(IUnitOfWork unitOfWork, UserManager<Member> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task BlockMemberAsync(string memberId)
        {
            var member = await _userManager.FindByIdAsync(memberId);
            if (member == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }
            // Kiểm tra người dùng đã bị block từ trước chưa
            var isAlreadyLocked = await _userManager.IsLockedOutAsync(member);
            if (isAlreadyLocked || (member.LockoutEnd.HasValue && member.LockoutEnd > DateTimeOffset.UtcNow))
            {
                throw new ConflictException("Tài khoản này đã bị khóa từ trước.");
            }

            await _userManager.SetLockoutEnabledAsync(member, true);
            // Cài đặt thời gian khóa đến vĩnh viễn (Khóa tài khoản)
            var lockoutResult = await _userManager.SetLockoutEndDateAsync(member, DateTimeOffset.MaxValue);

            if (!lockoutResult.Succeeded)
            {
                throw new BadRequestException("Không thể khóa tài khoản này.");
            }

            // Đá văng thiết bị
            if (int.TryParse(memberId, out int newmemberId))
            {
                await _unitOfWork.RefreshTokenRepository.RevokeAllMemberTokensAsync(newmemberId);
                await _unitOfWork.SaveChangesAsync(); // Lưu thay đổi của Refresh Token
            }
        }
    }
}
