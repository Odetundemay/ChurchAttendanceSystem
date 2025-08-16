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

    public async Task<ServiceResult<object>> LoginAsync(LoginDto dto)
    {
        var user = await _db.StaffUsers.FirstOrDefaultAsync(x => x.Email == dto.Email);
        
        if (user is null || !Password.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<object>.Unauthorized("Invalid email or password");

        var token = Jwt.IssueToken(user.Id, user.Email, user.Role, _configuration);
        var response = new 
        { 
            token, 
            user = new 
            { 
                user.Id, 
                user.FullName, 
                user.Email, 
                user.Role 
            } 
        };
        
        return ServiceResult<object>.Success(response);
    }

    public async Task<ServiceResult<Guid>> RegisterStaffAsync(RegisterStaffDto dto)
    {
        if (await _db.StaffUsers.AnyAsync(x => x.Email == dto.Email))
            return ServiceResult<Guid>.Conflict("Email already exists");

        var user = new StaffUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = Password.Hash(dto.Password),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Staff" : dto.Role
        };

        _db.StaffUsers.Add(user);
        await _db.SaveChangesAsync();

        return ServiceResult<Guid>.Success(user.Id, 201);
    }
}