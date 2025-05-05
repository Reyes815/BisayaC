namespace BisayaC
{
    public static class ErrorStatements
    {
        public enum ErrorType
        {
            General = 0,
            MissingToken = 1,
            VariableNotDeclared = 2,
            KeywordIsReserved = 3,
            AssignmentTargetInvalid = 4
        }

        public static void ThrowError(int lineNumber, ErrorType errorType, string context = null, string extraInfo = null)
        {
            string message = $"[Error] Line {lineNumber}:";

            switch (errorType)
            {
                case ErrorType.MissingToken:
                    message += $" A required token is missing. Expected '{context}' in the expression or statement.";
                    break;
                case ErrorType.VariableNotDeclared:
                    message += $" The variable '{context}' was used without being declared. Declare it before usage.";
                    break;
                case ErrorType.KeywordIsReserved:
                    message += $" The identifier '{context}' is a reserved keyword and cannot be used as a variable or function name.";
                    break;
                case ErrorType.AssignmentTargetInvalid:
                    message += $" The left-hand side of the assignment is not a valid target. Ensure you're assigning to a variable or valid expression.";
                    break;
                default:
                    message += $" An unspecified error occurred: {context}";
                    break;
            }

            if (!string.IsNullOrEmpty(extraInfo))
            {
                message += $" Additional Info: {extraInfo}";
            }

            throw new ArgumentException(message);
        }
    }
}