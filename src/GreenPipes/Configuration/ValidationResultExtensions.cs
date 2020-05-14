namespace GreenPipes
{
    using Validation;


    public static class ValidationResultExtensions
    {
        public static ValidationResult Failure(this ISpecification configurator, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Failure, message);
        }

        public static ValidationResult Failure(this ISpecification configurator, string key, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Failure, key, message);
        }

        public static ValidationResult Failure(this ISpecification configurator, string key, string value, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Failure, key, value, message);
        }

        public static ValidationResult Warning(this ISpecification configurator, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Warning, message);
        }

        public static ValidationResult Warning(this ISpecification configurator, string key, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Warning, key, message);
        }

        public static ValidationResult Warning(this ISpecification configurator, string key, string value, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Warning, key, value, message);
        }

        public static ValidationResult Success(this ISpecification configurator, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Success, message);
        }

        public static ValidationResult Success(this ISpecification configurator, string key, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Success, key, message);
        }

        public static ValidationResult Success(this ISpecification configurator, string key, string value, string message)
        {
            return new ConfigurationValidationResult(ValidationResultDisposition.Success, key, value, message);
        }

        public static ValidationResult WithParentKey(this ValidationResult result, string parentKey)
        {
            //string key = result.Key.Contains(".") ? result.Key.Substring(result.Key.IndexOf('.')) : "";

            var key = parentKey + "." + result.Key;

            return new ConfigurationValidationResult(result.Disposition, key, result.Value, result.Message);
        }
    }
}
