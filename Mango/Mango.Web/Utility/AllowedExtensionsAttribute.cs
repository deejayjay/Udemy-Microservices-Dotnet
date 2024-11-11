using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility;

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;
    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;

        if(file is not null)
        {
            var extension = Path.GetExtension(file.FileName);

            if(!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult("Invalid file format!");
            }
        }

        return ValidationResult.Success;
    }
}
