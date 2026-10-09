using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services.Abstractions;
using RentalApplications.Services.Models;

namespace RentalApplications.Services;

/// <inheritdoc cref="IRentalApplicationService" />
public class RentalApplicationService : IRentalApplicationService
{
    private readonly AppDbContext _db;
    private readonly ILeaseAvailabilityService _leaseAvailability;
    private readonly ILogger<RentalApplicationService> _logger;

    public RentalApplicationService(
        AppDbContext db,
        ILeaseAvailabilityService leaseAvailability,
        ILogger<RentalApplicationService> logger)
    {
        _db = db;
        _leaseAvailability = leaseAvailability;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationListRow>> ListAsync(
        ApplicationStatus? status,
        int? propertyId,
        string? applicantUserId,
        bool isPropertyManager,
        CancellationToken cancellationToken = default)
    {
        var query = _db.RentalApplications
            .AsNoTracking()
            .Include(a => a.Unit)
            .ThenInclude(u => u.Property)
            .AsQueryable();

        if (!isPropertyManager && applicantUserId is not null)
        {
            query = query.Where(a => a.ApplicantUserId == applicantUserId);
        }

        if (status is ApplicationStatus filterStatus)
        {
            query = query.Where(a => a.Status == filterStatus);
        }

        if (propertyId is int filterPropertyId)
        {
            query = query.Where(a => a.Unit.PropertyId == filterPropertyId);
        }

        return await query
            .OrderByDescending(a => a.Id)
            .Select(a => new ApplicationListRow(
                a.Id,
                a.ApplicantName,
                a.Unit.Property.Name,
                a.Unit.UnitNumber,
                a.Status,
                a.StatusHistories.Max(h => (DateTimeOffset?)h.ChangedAt) ?? DateTimeOffset.MinValue))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Property>> GetPropertiesForFilterAsync(CancellationToken cancellationToken = default) =>
        await _db.Properties.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<(bool Success, string? Error, int ApplicationId)> TryCreateAsync(
        int unitId,
        string userId,
        string displayName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var unit = await _db.Units
            .Include(u => u.Leases)
            .FirstOrDefaultAsync(u => u.Id == unitId, cancellationToken);

        if (unit is null)
        {
            _logger.LogWarning("Create application failed: unit {UnitId} not found for user {UserId}", unitId, userId);
            return (false, "Unit not found.", 0);
        }

        if (!_leaseAvailability.IsUnitAvailableOnDate(unit.Leases, today))
        {
            _logger.LogWarning("Create application failed: unit {UnitId} unavailable for user {UserId}", unitId, userId);
            return (false, "This unit is no longer available.", 0);
        }

        var application = new RentalApplication
        {
            UnitId = unitId,
            ApplicantUserId = userId,
            Status = ApplicationStatus.Draft,
            CurrentSection = ApplicationSection.ApplicantInformation,
            ApplicantName = displayName,
            Email = email
        };

        _db.RentalApplications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        _db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            RentalApplicationId = application.Id,
            FromStatus = ApplicationStatus.Draft,
            ToStatus = ApplicationStatus.Draft,
            ChangedByUserId = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created draft application {ApplicationId} for unit {UnitId} by user {UserId}",
            application.Id,
            unitId,
            userId);

        return (true, null, application.Id);
    }

    /// <inheritdoc />
    public async Task<RentalApplication?> GetApplicationDetailAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.RentalApplications
            .Include(a => a.Unit)
            .ThenInclude(u => u.Property)
            .Include(a => a.ResidenceHistories)
            .Include(a => a.Unit)
            .ThenInclude(u => u.Leases)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task SetCurrentSectionAsync(RentalApplication application, ApplicationSection section, CancellationToken cancellationToken = default)
    {
        application.CurrentSection = section;
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveApplicantInformationAsync(
        RentalApplication application,
        string name,
        string phone,
        string email,
        string address,
        CancellationToken cancellationToken = default)
    {
        application.ApplicantName = name;
        application.Phone = phone;
        application.Email = email;
        application.CurrentAddress = address;
        application.ApplicantInformationSaved = true;
        application.CurrentSection = ApplicationSection.ResidenceHistory;
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkResidenceHistoryCompleteAsync(RentalApplication application, CancellationToken cancellationToken = default)
    {
        application.ResidenceHistorySaved = true;
        application.CurrentSection = ApplicationSection.Summary;
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ChangeStatusAsync(
        RentalApplication application,
        ApplicationStatus toStatus,
        string changedByUserId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var fromStatus = application.Status;
        application.Status = toStatus;

        _db.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            RentalApplicationId = application.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTimeOffset.UtcNow,
            Comment = comment
        });

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Application {ApplicationId} status changed from {FromStatus} to {ToStatus} by user {UserId}",
            application.Id,
            fromStatus,
            toStatus,
            changedByUserId);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> TrySubmitAsync(
        RentalApplication application,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (!ApplicationPolicy.CanSubmit(application))
        {
            _logger.LogWarning("Submit failed for application {ApplicationId}: incomplete sections", application.Id);
            return (false, "Complete all sections before submitting.");
        }

        // Re-check lease at submit time; availability may have changed since the draft was started.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _db.Entry(application.Unit).Collection(u => u.Leases).LoadAsync(cancellationToken);
        if (_leaseAvailability.HasActiveLeaseOnDate(application.Unit.Leases, today))
        {
            _logger.LogWarning(
                "Submit failed for application {ApplicationId}: unit {UnitId} has active lease",
                application.Id,
                application.UnitId);
            return (false, "This unit is no longer available.");
        }

        await ChangeStatusAsync(application, ApplicationStatus.Submitted, userId, "Submitted for review.", cancellationToken);
        _logger.LogInformation("Application {ApplicationId} submitted by user {UserId}", application.Id, userId);
        return (true, null);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> TryReviewAsync(
        int applicationId,
        ReviewOutcome outcome,
        string? comment,
        DateOnly leaseStartDate,
        string reviewerUserId,
        CancellationToken cancellationToken = default)
    {
        var application = await GetApplicationDetailAsync(applicationId, cancellationToken);
        if (application is null || !ApplicationReviewPolicy.CanReview(application.Status))
        {
            _logger.LogWarning(
                "Review failed for application {ApplicationId}: not found or status {Status}",
                applicationId,
                application?.Status);
            return (false, "Application not found or not in review.");
        }

        // Same lease guard as submit; prevents a second lease if another was approved meanwhile.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (outcome == ReviewOutcome.Approve &&
            _leaseAvailability.HasActiveLeaseOnDate(application.Unit.Leases, today))
        {
            _logger.LogWarning(
                "Review approve failed for application {ApplicationId}: unit {UnitId} already leased",
                applicationId,
                application.UnitId);
            return (false, "This unit already has an active lease.");
        }

        var targetStatus = ApplicationReviewPolicy.GetTargetStatus(outcome);
        if (targetStatus is null)
        {
            _logger.LogWarning("Review failed for application {ApplicationId}: invalid outcome {Outcome}", applicationId, outcome);
            return (false, "Invalid review outcome.");
        }

        await ChangeStatusAsync(application, targetStatus.Value, reviewerUserId, comment, cancellationToken);

        if (outcome == ReviewOutcome.Approve)
        {
            _db.Leases.Add(new Lease
            {
                UnitId = application.UnitId,
                RentalApplicationId = application.Id,
                StartDate = leaseStartDate,
                EndDate = _leaseAvailability.CalculateEndDate(leaseStartDate)
            });
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Lease created for application {ApplicationId} on unit {UnitId} starting {LeaseStart}",
                application.Id,
                application.UnitId,
                leaseStartDate);
        }

        _logger.LogInformation(
            "Application {ApplicationId} reviewed with outcome {Outcome} by user {ReviewerUserId}",
            applicationId,
            outcome,
            reviewerUserId);

        return (true, null);
    }
}
