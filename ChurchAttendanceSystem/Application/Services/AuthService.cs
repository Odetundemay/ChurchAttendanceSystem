using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Domain;
using ChurchAttendanceSystem.Dto;
using ChurchAttendanceSystem.Infrastructure;

namespace ChurchAttendanceSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDb _db;
    private readonly IConfiguration _configuration;

    public AuthService(AppDb db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _db.StaffUsers.FirstOrDefaultAsync(x => x.Email == dto.Email);
        
        if (user is null || !Password.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<LoginResponseDto>.Unauthorized("Invalid email or password");

        var token = Jwt.IssueToken(user.Id, user.Email, user.Role, _configuration);
        
        var staffDto = new StaffDto(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToLower(),
            true
        );
        
        var response = new LoginResponseDto(token, staffDto);
        
        return ServiceResult<LoginResponseDto>.Success(response);
    }

    public async Task<ServiceResult<Guid>> RegisterStaffAsync(RegisterStaffDto dto)
    {
        if (await _db.StaffUsers.AnyAsync(x => x.Email == dto.Email))
            return ServiceResult<Guid>.Conflict("Email already exists");

        var user = new StaffUser
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = Password.Hash(dto.Password),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Staff" : dto.Role
        };

        _db.StaffUsers.Add(user);
        await _db.SaveChangesAsync();

        return ServiceResult<Guid>.Success(user.Id, 201);
    }
}