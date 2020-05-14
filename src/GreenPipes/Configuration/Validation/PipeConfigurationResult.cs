namespace GreenPipes.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class PipeConfigurationResult :
        IPipeConfigurationResult
    {
        readonly List<ValidationResult> _results;

        public PipeConfigurationResult(IEnumerable<ValidationResult> results)
        {
            _results = results.ToList();
        }

        public bool ContainsFailure
        {
            get { return _results.Any(x => x.Disposition == ValidationResultDisposition.Failure); }
        }

        public string GetMessage(string header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header));

            var message = header +
                Environment.NewLine +
                string.Join(Environment.NewLine, _results.Select(x => x.ToString()).ToArray());

            return message;
        }

        public bool Any()
        {
            return _results.Count > 0;
        }
    }
}
