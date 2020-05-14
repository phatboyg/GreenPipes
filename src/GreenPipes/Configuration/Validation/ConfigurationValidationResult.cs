namespace GreenPipes.Validation
{
    using System;


    [Serializable]
    public class ConfigurationValidationResult :
        ValidationResult
    {
        public ConfigurationValidationResult(ValidationResultDisposition disposition, string key, string value, string message)
        {
            Disposition = disposition;
            Key = key;
            Value = value;
            Message = message;
        }

        public ConfigurationValidationResult(ValidationResultDisposition disposition, string key, string message)
        {
            Disposition = disposition;
            Key = key;
            Message = message;
        }

        public ConfigurationValidationResult(ValidationResultDisposition disposition, string message)
        {
            Key = "";
            Disposition = disposition;
            Message = message;
        }

        public ValidationResultDisposition Disposition { get; }
        public string Key { get; }
        public string Value { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{Disposition}] {(string.IsNullOrEmpty(Key) ? Message : Key + " " + Message)}";
        }
    }
}
