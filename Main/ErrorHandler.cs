namespace BisayaC
{
    /// <summary>
    /// Centralized error handling.
    /// </summary>
    public static class ErrorHandler
    {
        public enum ErrorCode
        {
            Generic = 0,
            ExpectedToken = 1,         // e.g. "Expected '{token}'."
            UndeclaredVariable = 2,      // e.g. "Undeclared variable '{variable}' is not defined."
            ReservedKeyword = 3,         // e.g. "Reserved keyword '{keyword}' cannot be used."
            InvalidAssignmentTarget = 4, // e.g. "Invalid assignment target."
        }

        public static void RaiseError(int line, ErrorCode code, string expected = null, string additionalInfo = null)
        {
            string message = $"Error at line: {line}.";
            switch (code)
            {
                case ErrorCode.ExpectedToken:
                    message += $" Expected '{expected}'";
                    break;
                case ErrorCode.UndeclaredVariable:
                    message += $" Undeclared variable '{expected}' is not defined.";
                    break;
                case ErrorCode.ReservedKeyword:
                    message += $" Reserved keyword '{expected}' cannot be used.";
                    break;
                case ErrorCode.InvalidAssignmentTarget:
                    message += $" Invalid assignment target.";
                    break;
                default:
                    message += $" {expected}";
                    break;
            }
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += $" {additionalInfo}";
            }
            throw new ArgumentException(message);
        }
    }
}
