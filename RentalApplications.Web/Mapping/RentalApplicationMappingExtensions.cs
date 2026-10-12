using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Models.Applications;
using RentalApplications.Models.Shared;

namespace RentalApplications.Mapping;

/// <summary>
/// Role and section flags come from the HTTP request; they are not stored on the entity.
/// </summary>
public readonly record struct ApplicationFormPresentation(
    ApplicationSection? Section = null,
    bool IsApplicant = false,
    bool IsPropertyManager = false);

/// <summary>
/// Maps rental application entities to the multi-step edit form view model.
/// </summary>
public static class RentalApplicationMappingExtensions
{
    /// <summary>
    /// Builds the full edit wizard view model, including role-based read-only and action flags.
    /// </summary>
    public static RentalApplicationFormViewModel ToEditFormViewModel(
        this RentalApplication application,
        ApplicationFormPresentation presentation)
    {
        var currentSection = presentation.Section ?? application.CurrentSection;
        var readOnly = !ApplicationPolicy.CanApplicantEdit(application.Status) || presentation.IsPropertyManager;

        return new RentalApplicationFormViewModel
        {
            Id = application.Id,
            UnitId = application.UnitId,
            PropertyName = application.Unit.Property.Name,
            UnitNumber = application.Unit.UnitNumber,
            Status = application.Status,
            CurrentSection = currentSection,
            IsReadOnly = readOnly,
            CanSubmit = ApplicationPolicy.CanSubmit(application) && !readOnly,
            CanWithdraw = ApplicationPolicy.CanWithdraw(application.Status) && presentation.IsApplicant,
            ShowManagerHistory = presentation.IsPropertyManager,
            ApplicantInformation = application.ToApplicantInformationViewModel(),
            ResidenceHistory = application.ToResidenceHistorySectionViewModel()
        };
    }

    /// <summary>
    /// Refreshes display metadata on a posted form after validation errors without rebuilding nested sections.
    /// </summary>
    public static void RefreshEditFormShell(
        this RentalApplicationFormViewModel model,
        RentalApplication application,
        ApplicationFormPresentation presentation)
    {
        model.PropertyName = application.Unit.Property.Name;
        model.UnitNumber = application.Unit.UnitNumber;
        model.Status = application.Status;
        model.IsReadOnly = !ApplicationPolicy.CanApplicantEdit(application.Status);
        model.CanSubmit = ApplicationPolicy.CanSubmit(application);
        model.CanWithdraw = ApplicationPolicy.CanWithdraw(application.Status) && presentation.IsApplicant;
        model.ShowManagerHistory = presentation.IsPropertyManager;
        model.CurrentSection = application.CurrentSection;
        if (model.CurrentSection != ApplicationSection.ApplicantInformation)
        {
            model.ApplicantInformation = application.ToApplicantInformationViewModel();
        }
        else if (string.IsNullOrWhiteSpace(model.ApplicantInformation.CurrentAddress.StreetAddress)
                 && !string.IsNullOrWhiteSpace(application.CurrentStreetAddress))
        {
            model.ApplicantInformation.CurrentAddress = application.ToCurrentAddressViewModel();
        }

        model.ResidenceHistory.Items = application.ResidenceHistories
            .OrderByDescending(r => r.MoveInDate)
            .Select(r => r.ToItemViewModel())
            .ToList();
    }

    private static ApplicantInformationSectionViewModel ToApplicantInformationViewModel(this RentalApplication application) => new()
    {
        ApplicantName = application.ApplicantName,
        Phone = application.Phone,
        Email = application.Email,
        CurrentAddress = application.ToCurrentAddressViewModel()
    };

    private static StructuredAddressViewModel ToCurrentAddressViewModel(this RentalApplication application) => new()
    {
        StreetAddress = application.CurrentStreetAddress,
        City = application.CurrentCity,
        State = application.CurrentState,
        PostalCode = application.CurrentPostalCode
    };

    private static ResidenceHistorySectionViewModel ToResidenceHistorySectionViewModel(this RentalApplication application) => new()
    {
        Items = application.ResidenceHistories
            .OrderByDescending(r => r.MoveInDate)
            .Select(r => r.ToItemViewModel())
            .ToList()
    };
}
