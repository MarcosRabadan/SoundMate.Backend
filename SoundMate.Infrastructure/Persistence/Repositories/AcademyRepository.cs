using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class AcademyRepository : IAcademyRepository
{
    private readonly SoundMateDbContext _context;

    public AcademyRepository(SoundMateDbContext context) => _context = context;

    public Task<Academy?> GetByIdAsync(AcademyId id, CancellationToken cancellationToken = default)
        => _context.Academies.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Academy?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => _context.Academies.FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => _context.Academies.AnyAsync(a => a.Slug == slug, cancellationToken);

    public async Task AddAsync(Academy academy, CancellationToken cancellationToken = default)
        => await _context.Academies.AddAsync(academy, cancellationToken);

    public void Update(Academy academy) => _context.Academies.Update(academy);

    public void Remove(Academy academy) => _context.Academies.Remove(academy);
}
