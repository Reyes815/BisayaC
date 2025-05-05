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
            string message = $"Error at line: {lineNumber}.";
            switch (errorType)
            {
                case ErrorType.MissingToken:
                    message += $" Expected '{context}'";
                    break;
                case ErrorType.VariableNotDeclared:
                    message += $" Undeclared variable '{context}' is not defined.";
                    break;
                case ErrorType.KeywordIsReserved:
                    message += $" Reserved keyword '{context}' cannot be used.";
                    break;
                case ErrorType.AssignmentTargetInvalid:
                    message += $" Invalid assignment target.";
                    break;
                default:
                    message += $" {context}";
                    break;
            }
            if (!string.IsNullOrEmpty(extraInfo))
            {
                message += $" {extraInfo}";
            }
            throw new ArgumentException(message);
        }
    }
}