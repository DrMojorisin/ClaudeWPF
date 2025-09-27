using System.IO;
using FluentValidation;
using WPFBase.ViewModels.Documents;

namespace WPFBase.Validators;

/// <summary>
/// FluentValidation validator for TextDocumentViewModel
/// </summary>
public class TextDocumentValidator : AbstractValidator<TextDocumentViewModel>
{
    public TextDocumentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Document title is required")
            .MaximumLength(255).WithMessage("Document title cannot exceed 255 characters")
            .Must(title => !ContainsInvalidPathChars(title))
            .WithMessage("Document title contains invalid characters");

        RuleFor(x => x.FilePath)
            .Must(BeValidFilePath)
            .WithMessage("Invalid file path format")
            .When(x => !string.IsNullOrEmpty(x.FilePath));

        RuleFor(x => x.Content)
            .MaximumLength(10_000_000) // 10MB limit for content
            .WithMessage("Document content is too large (maximum 10MB)");

        RuleFor(x => x.Syntax)
            .NotEmpty().WithMessage("Syntax type is required")
            .Must(BeValidSyntaxType)
            .WithMessage("Invalid syntax type");
    }

    private static bool ContainsInvalidPathChars(string title)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return title.Any(c => invalidChars.Contains(c));
    }

    private static bool BeValidFilePath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return true; // Optional field

        try
        {
            // Check if the path is valid
            Path.GetFullPath(filePath);
            
            // Check for invalid characters
            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            
            if (filePath.Any(c => invalidPathChars.Contains(c)))
                return false;
                
            var fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName) && fileName.Any(c => invalidFileNameChars.Contains(c)))
                return false;
                
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidSyntaxType(string syntax)
    {
        var validSyntaxTypes = new[]
        {
            "Text", "C#", "XML", "JSON", "JavaScript", "HTML", "CSS", 
            "SQL", "Python", "Markdown", "XAML", "TypeScript"
        };
        
        return validSyntaxTypes.Contains(syntax);
    }
}