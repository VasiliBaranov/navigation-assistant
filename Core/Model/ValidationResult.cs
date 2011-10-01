using System.Collections.Generic;
using Core.Utilities;

namespace Core.Model
{
    public class ValidationResult
    {
        public List<string> ErrorKeys { get; set; }

        public bool IsValid
        {
            get { return ListUtility.IsNullOrEmpty(ErrorKeys); }
        }

        public ValidationResult()
        {
            ErrorKeys = new List<string>();
        }

        public ValidationResult(List<string> errorKeys)
        {
            ErrorKeys = errorKeys;
        }
    }
}
