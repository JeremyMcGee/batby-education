using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class StudentAccountRepository : IStudentAccountRepository
{
    private readonly BatbyEducationDbContext _context;

    public StudentAccountRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAccount?> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.StudentAccounts
            .FirstOrDefaultAsync(sa => sa.StudentId == studentId);
    }

    public async Task AddAsync(StudentAccount account)
    {
        await _context.StudentAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(StudentAccount account)
    {
        _context.StudentAccounts.Update(account);
        await _context.SaveChangesAsync();
    }
}
