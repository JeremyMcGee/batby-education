using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly BatbyEducationDbContext _context;

    public StudentRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students
            .Include(s => s.AuditHistory)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        var students = await _context.Students.ToListAsync();
        return students.FirstOrDefault(s => s.Email.Value == email);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }
}
