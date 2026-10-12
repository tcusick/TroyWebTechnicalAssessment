using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace RentalApplications.Mvc;

/// <summary>
/// Uses property names as human-readable labels when no <see cref="DisplayAttribute"/> name is set.
/// </summary>
public sealed class HumanizerDisplayMetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.MetadataKind != ModelMetadataKind.Property)
        {
            return;
        }

        var name = context.Key.Name;
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        foreach (var attribute in context.Attributes)
        {
            if (attribute is DisplayAttribute { Name: { Length: > 0 } })
            {
                return;
            }
        }

        context.DisplayMetadata.DisplayName = () => name.Humanize(LetterCasing.Title);
    }
}
