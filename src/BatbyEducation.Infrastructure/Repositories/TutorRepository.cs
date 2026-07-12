using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class TutorRepository : ITutorRepository
{
    private readonly BatbyEducationDbContext _context;

    public TutorRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<Tutor?> GetByIdAsync(Guid id)
    {
        return await _context.Tutors.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tutor?> GetByEmailAsync(string email)
    {
        var tutors = await _context.Tutors.ToListAsync();
        return tutors.FirstOrDefault(t => t.Email.Value == email);
    }

    public async Task<IReadOnlyList<Tutor>> GetAllAsync()
    {
        return await _context.Tutors.ToListAsync();
    }

    public async Task AddAsync(Tutor tutor)
    {
        await _context.Tutors.AddAsync(tutor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tutor tutor)
    {
        _context.Tutors.Update(tutor);
        await _context.SaveChangesAsync();
    }
}
