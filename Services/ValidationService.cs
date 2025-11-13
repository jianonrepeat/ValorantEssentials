using System.Text.RegularExpressions;

namespace ValorantEssentials.Services
{
    public interface IValidationService
    {
        ValidationResult ValidateResolution(string widthText, string heightText);
        ValidationResult ValidatePath(string path, bool mustExist = true);
        ValidationResult ValidateUrl(string url);
        bool IsValidResolution(int width, int height);
    }

    public class ValidationService : IValidationService
    {
        private const int MIN_WIDTH = 800;
        private const int MAX_WIDTH = 7680;
        private const int MIN_HEIGHT = 600;
        private const int MAX_HEIGHT = 4320;
        private const int MAX_ASPECT_RATIO = 4; // 4:1 maximum aspect ratio

        public ValidationResult ValidateResolution(string widthText, string heightText)
        {
            if (string.IsNullOrWhiteSpace(widthText))
                return ValidationResult.Failure("Width cannot be empty");

            if (string.IsNullOrWhiteSpace(heightText))
                return ValidationResult.Failure("Height cannot be empty");

            if (!int.TryParse(widthText, out var width) || width <= 0)
                return ValidationResult.Failure("Width must be a positive integer");

            if (!int.TryParse(heightText, out var height) || height <= 0)
                return ValidationResult.Failure("Height must be a positive integer");

            return ValidateResolution(width, height);
        }

        public ValidationResult ValidateResolution(int width, int height)
        {
            if (width < MIN_WIDTH)
                return ValidationResult.Failure($"Width must be at least {MIN_WIDTH}px");

            if (width > MAX_WIDTH)
                return ValidationResult.Failure($"Width cannot exceed {MAX_WIDTH}px");

            if (height < MIN_HEIGHT)
                return ValidationResult.Failure($"Height must be at least {MIN_HEIGHT}px");

            if (height > MAX_HEIGHT)
                return ValidationResult.Failure($"Height cannot exceed {MAX_HEIGHT}px");

            var aspectRatio = Math.Max(width, height) / (double)Math.Min(width, height);
            if (aspectRatio > MAX_ASPECT_RATIO)
                return ValidationResult.Failure($"Aspect ratio is too extreme (max {MAX_ASPECT_RATIO}:1)");

            return ValidationResult.Success();
        }

        public ValidationResult ValidatePath(string path, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                return ValidationResult.Failure("Path cannot be empty");

            try
            {
                // Test if path is valid format
                Path.GetFullPath(path);

                if (mustExist && !Directory.Exists(path) && !File.Exists(path))
                    return ValidationResult.Failure("Path does not exist");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Invalid path format: {ex.Message}");
            }
        }

        public ValidationResult ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return ValidationResult.Failure("URL cannot be empty");

            try
            {
                var uri = new Uri(url);
                
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");

                if (string.IsNullOrWhiteSpace(uri.Host))
                    return ValidationResult.Failure("URL must have a valid host");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Invalid URL format: {ex.Message}");
            }
        }

        public bool IsValidResolution(int width, int height)
        {
            return ValidateResolution(width, height).IsValid;
        }
    }

    public record ValidationResult(bool IsValid, string? ErrorMessage = null)
    {
        public static ValidationResult Success() => new(true, null);
        public static ValidationResult Failure(string error) => new(false, error);

        public override string ToString() => IsValid ? "Valid" : ErrorMessage ?? "Invalid";
    }
}