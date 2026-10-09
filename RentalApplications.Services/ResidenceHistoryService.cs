using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Services;

/// <inheritdoc cref="IResidenceHistoryService" />
public class ResidenceHistoryService : IResidenceHistoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ResidenceHistoryService> _logger;

    public ResidenceHistoryService(AppDbContext db, ILogger<ResidenceHistoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ResidenceHistory?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.ResidenceHistories
            .Include(r => r.RentalApplication)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<int> SaveAsync(ResidenceHistory residence, CancellationToken cancellationToken = default)
    {
        if (residence.Id == 0)
        {
            _db.ResidenceHistories.Add(residence);
        }
        else
        {
            _db.ResidenceHistories.Update(residence);
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Saved residence {ResidenceId} for application {ApplicationId}",
            residence.Id,
            residence.RentalApplicationId);
        return residence.Id;
    }

    /// <inheritdoc />
    public async Task<(bool Success, int ApplicationId)> TryDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var residence = await GetAsync(id, cancellationToken);
        if (residence is null)
        {
            _logger.LogWarning("Delete residence failed: residence {ResidenceId} not found", id);
            return (false, 0);
        }

        _db.ResidenceHistories.Remove(residence);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Deleted residence {ResidenceId} from application {ApplicationId}",
            id,
            residence.RentalApplicationId);
        return (true, residence.RentalApplicationId);
    }

    /// <inheritdoc />
    public async Task<bool> CanApplicantEditApplicationAsync(int applicationId, string userId, CancellationToken cancellationToken = default)
    {
        var application = await _db.RentalApplications.FindAsync([applicationId], cancellationToken);
        if (application is null)
        {
            return false;
        }

        return application.ApplicantUserId == userId && ApplicationPolicy.CanApplicantEdit(application.Status);
    }
}
