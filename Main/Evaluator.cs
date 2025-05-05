using System.Data;

namespace Bisaya__
{
    /// <summary>
    /// Maintains variable declarations and values during program execution.
    /// </summary>
    public class ExecutionContext
    {
        // Stores variables with their value and declared type.
        private readonly Dictionary<string, (object Value, TokenType Type)> variables = new();

        public void DeclareVariable(string name, object value, TokenType type, int lineNumber)
        {
            object typedValue;
            try
            {
                typedValue = type switch
                {
                    TokenType.NUMERO => InterpreterClass.ConvertToInt(value, lineNumber),
                    TokenType.TIPIK => Convert.ToSingle(value),
                    TokenType.LETRA => Convert.ToChar(value),
                    TokenType.TINUOD => Convert.ToBoolean(value),
                    _ => value
                };

                variables[name] = (typedValue, type);
            }
            catch
            {
                string valueString = InterpreterClass.ConvertToString(value);
                string actualType = RetrieveType(value).ToString();
                throw new ArgumentException($"Error at line: {lineNumber}. Type mismatch: Cannot declare '{name}' as {type} with value '{valueString}' type '{actualType}'.");
            }
        }

        public object GetVariable(string name, int lineNumber)
        {
            if (variables.TryGetValue(name, out var variable))
            {
                return variable.Value;
            }
            // Check if the name is a reserved keyword.
            if (Lexer.keywords.TryGetValue(name, out var _type))
            {
                throw new ArgumentException($"Error at line: {lineNumber}. Invalid use of reserved keyword '{name}'.");
            }
            throw new ArgumentException($"Variable '{name}' is not defined.");
        }
        
        public void SetVariable(string name, object value, int lineNumber)
        {
            if (variables.ContainsKey(name))
            {
                var (existingValue, type) = variables[name];
                object typedValue;

                try
                {
                    typedValue = ConvertToType(value, type, lineNumber);
                    if (!IsTypeCompatible(typedValue, type))
                    {
                        throw new InvalidOperationException($"Error at line: {lineNumber}. Incompatible type after conversion.");
                    }
                    variables[name] = (typedValue, type);
                }
                catch
                {
                    string valueString = InterpreterClass.ConvertToString(value);
                    string actualType = RetrieveType(value).ToString();
                    throw new ArgumentException($"Error at line: {lineNumber}. Type mismatch: Cannot assign value '{valueString}' type '{actualType}' to '{name}' type {type}.");
                }

                return;
            }

            if (Lexer.keywords.TryGetValue(name, out var _type))
            {
                throw new ArgumentException($"Error at line: {lineNumber}. Invalid use of reserved keyword '{name}'.");
            }
            throw new ArgumentException($"Variable '{name}' is not defined.");
        }
        private static object ConvertToType(object value, TokenType type, int lineNumber)
        {
            return type switch
            {
                TokenType.NUMERO => InterpreterClass.ConvertToInt(value, lineNumber),
                TokenType.TIPIK => Convert.ToSingle(value),
                TokenType.LETRA => Convert.ToChar(value),
                TokenType.TINUOD => Convert.ToBoolean(value),
                TokenType.PULONG => value.ToString(),
                _ => value,
            };
        }
        private static bool IsTypeCompatible(object value, TokenType type)
        {
            return type switch
            {
                TokenType.NUMERO => int.TryParse(value.ToString(), out _),
                TokenType.TIPIK => float.TryParse(value.ToString(), out _),
                TokenType.LETRA => value.ToString().Length == 1,
                TokenType.TINUOD => bool.TryParse(value.ToString(), out _),
                TokenType.PULONG => value is string,
                _ => false,
            };
        }

        private static object RetrieveType(object value)
        {
            return value switch
            {
                int _ => "NUMERO",
                float _ => "TIPIK",
                char _ => "LETRA",
                bool _ => "TINUOD",
                string _ => "PULONG",
                _ => value.GetType().Name.ToUpper()
            };
        }
    }
    public class InterpreterClass
    {
        private readonly ExecutionContext context = new();
        public void Interpret(ProgramNode program)
        {
            try
            {
                if (program == null || program.Statements == null)
                {
                    throw new ArgumentException("No program to interpret.");
                }

                foreach (var statement in program.Statements)
                {
                    ExecuteStatement(statement);
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        private void ExecuteStatement(Statement statement)
        {
            switch (statement)
            {
                case DeclarationStatement decl:
                    foreach (var variable in decl.Variables)
                    {
                        var value = variable.Initializer != null ? EvaluateExpression(variable.Initializer) : GetDefaultValue(decl.Type);
                        context.DeclareVariable(variable.Name, value, decl.Type, variable.LineNumber);
                    }
                    break;
                case AssignmentStatement assign:
                    var assignValue = EvaluateExpression(assign.Value);
                    var variableValue = context.GetVariable(assign.Variable.Name, assign.LineNumber);
                    if (assign.Operator.Type != TokenType.ASAYNMENT)
                    {
                        assignValue = EvaluateBinaryExpression(variableValue, assign.Operator, assignValue);
                    }
                    context.SetVariable(assign.Variable.Name, assignValue, assign.LineNumber);
                    break;
                case IncrementStatement increment:
                    IncrementVariable(increment.Variable);
                    break;
                case InputStatement inputStmt:
                    foreach (var variable in inputStmt.Variables)
                    {
                        var input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input))
                        {
                            throw new ArgumentException($"Error at line: {inputStmt.LineNumber}. Input is invalid, try again.");
                        }
                        context.SetVariable(variable.Name, input, variable.LineNumber);
                    }
                    break;
                case OutputStatement output:
                    foreach (var expression in output.Expressions)
                    {
                        Console.Write(ConvertToString(EvaluateExpression(expression)));
                    }
                    break;
                case IfStatement ifStmt:
                    bool condition = (bool)EvaluateExpression(ifStmt.Condition);
                    if (condition)
                    {
                        foreach (var thenStmt in ifStmt.ThenBranch)
                        {
                            ExecuteStatement(thenStmt);
                        }
                    }
                    else
                    {
                        foreach (var elseStmt in ifStmt.ElseBranch)
                        {
                            ExecuteStatement(elseStmt);
                        }
                    }
                    break;
                case ForLoopStatement forLoop:
                    ExecuteStatement(forLoop.Initialization);

                    while (IsTruthy(EvaluateExpression(forLoop.Condition)))
                    {
                        foreach (var stmt in forLoop.Body)
                        {
                            ExecuteStatement(stmt);
                        }

                        if (forLoop.Update is UnaryExpression unaryExpr &&
                            unaryExpr.Operator.Type == TokenType.INCREMENT &&
                            unaryExpr.Right is VariableExpression varExpr)
                        {
                            var variable = new Variable(varExpr.Name, forLoop.Update.LineNumber);

                            IncrementVariable(variable);
                        }
                        else
                        {
                            EvaluateExpression(forLoop.Update);
                        }
                    }
                    break;     
                case WhileStatement whileStmt:
                    while ((bool)EvaluateExpression(whileStmt.Condition))
                    {
                        foreach (var bodyStmt in whileStmt.Body)
                        {
                            ExecuteStatement(bodyStmt);
                        }
                    }
                    break;
                case EmptyStatement:
                    break;
                default:
                    throw new NotImplementedException($"Error at line: {statement.LineNumber}. Execution not implemented for statement type {statement.GetType().Name}");
            }
        }

        private object EvaluateExpression(Expression expression)
        {
            switch (expression)
            {
                case LiteralExpression lit:
                    return lit.Value;
                case VariableExpression varExpr:
                    return context.GetVariable(varExpr.Name, varExpr.LineNumber);
                case BinaryExpression binExpr:
                    var left = EvaluateExpression(binExpr.Left);
                    var right = EvaluateExpression(binExpr.Right);
                    return EvaluateBinaryExpression(left, binExpr.Operator, right);
                case UnaryExpression unaryExpr:
                    return EvaluateUnaryExpression(unaryExpr);
                case LogicalExpression logExpr:
                    var leftLogic = EvaluateExpression(logExpr.Left);
                    var rightLogic = EvaluateExpression(logExpr.Right);
                    return EvaluateLogicalExpression(leftLogic, logExpr.Operator, rightLogic);
                case AssignmentExpression assignExpr:
                    var value = EvaluateExpression(assignExpr.Value);
                    context.SetVariable(assignExpr.Variable.Name, value, assignExpr.LineNumber);
                    return value;
                case GroupingExpression groupExpr:
                    return EvaluateExpression(groupExpr.InnerExpression);
                default:
                    throw new NotImplementedException($"Error at line: {expression.LineNumber}. Evaluation not implemented for expression type {expression.GetType().Name}");
            }
        }
        private object EvaluateBinaryExpression(object left, Token operatorToken, object right)
        {
            if (operatorToken.Type == TokenType.SUMPAY)
            {
                string leftStr = ConvertToString(left);
                string rightStr = ConvertToString(right);
                return leftStr + rightStr;
            }

            left = ConvertIfString(left);
            right = ConvertIfString(right);

            // Handle cases where left or right might be a VariableExpression.
            left = left is VariableExpression leftVar ? context.GetVariable(leftVar.Name, leftVar.LineNumber) : left;
            right = right is VariableExpression rightVar ? context.GetVariable(rightVar.Name, rightVar.LineNumber) : right;

            bool isLeftFloat = left is float;
            bool isRightFloat = right is float;

            // Ensure consistency when dealing with doubles.
            if (left is double || right is double)
            {
                left = Convert.ToSingle(left);
                right = Convert.ToSingle(right);
            }

            if (left is int && isRightFloat)
            {
                left = Convert.ToSingle(left);
            }
            if (right is int && isLeftFloat)
            {
                right = Convert.ToSingle(right);
            }

            return operatorToken.Type switch
            {
                TokenType.DUGANG => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.KUHA => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.PADAGHAN => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.BAHIN => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.SOBRA => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.LABAW => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.UBOS => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.LABAWSA => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.UBOSSA => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.PAREHAS => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.LAHI => PerformOperation(left, right, operatorToken.Value, operatorToken.Line),
                TokenType.ADDASSIGNMENT => PerformOperation(left, right, "+", operatorToken.Line),
                TokenType.SUBASSIGNMENT => PerformOperation(left, right, "-", operatorToken.Line),
                TokenType.MULASSIGNMENT => PerformOperation(left, right, "*", operatorToken.Line),
                TokenType.DIVASSIGNMENT => PerformOperation(left, right, "/", operatorToken.Line),
                TokenType.MODASSIGNMENT => PerformOperation(left, right, "%", operatorToken.Line),
                _ => throw new NotImplementedException($"Error at line: {operatorToken.Line}. Operator {operatorToken.Type} is not implemented."),
            };
        }

        private static object EvaluateLogicalExpression(object left, Token operatorToken, object right)
        {
            if (left is bool leftBool && right is bool rightBool)
            {
                return operatorToken.Type switch
                {
                    TokenType.UG => leftBool && rightBool,
                    TokenType.O => leftBool || rightBool,
                    _ => throw new NotImplementedException($"Error at line: {operatorToken.Line}. Logical operator {operatorToken.Type} is not implemented."),
                };
            }
            throw new ArgumentException($"Error at line: {operatorToken.Line}. Invalid operands for logical expression. Found: '{left}' and '{right}'");
        }

        private object EvaluateUnaryExpression(UnaryExpression expr)
        {
            object right = EvaluateExpression(expr.Right);
            right = EnsureCorrectType(right, expr.Operator.Type);

            return expr.Operator.Type switch
            {
                TokenType.KUHA => right switch
                {
                    float rightFloat => -rightFloat,
                    int rightInt => -rightInt,
                    _ => throw new ArgumentException($"Error at line: {expr.LineNumber}. Unary '-' expects a numeric operand.")
                },
                TokenType.DUGANG => right switch
                {
                    float rightFloat => rightFloat,
                    int rightInt => rightInt,
                    _ => throw new ArgumentException($"Error at line: {expr.LineNumber}. Unary '+' expects a numeric operand.")
                },
                TokenType.DILI => right is bool rightBool ? !rightBool : 
                         throw new ArgumentException($"Error at line: {expr.LineNumber}. Unary 'NOT' expects a boolean operand. Found: '{right}'"),
                TokenType.INCREMENT => right switch
                {
                    int rightInt => HandleIntegerOverflow(rightInt, 1, "+", expr.LineNumber),
                    _ => throw new ArgumentException($"Error at line: {expr.LineNumber}. Can only use increment operator on integers.")
                },
                _ => throw new NotImplementedException($"Error at line: {expr.LineNumber}. Unary operator {expr.Operator.Type} not implemented.")
            };
        }

        #region Helper Methods

        private static object EnsureCorrectType(object value, TokenType operationType)
        {
            switch (operationType)
            {
                case TokenType.KUHA or TokenType.DUGANG:
                    if (value is string stringValue)
                    {
                        if (float.TryParse(stringValue, out float floatValue))
                        {
                            return floatValue;
                        }
                        if (int.TryParse(stringValue, out int intValue))
                        {
                            return intValue;
                        }
                    }
                    break;
                case TokenType.DILI:
                    if (value is string strValue && bool.TryParse(strValue, out bool boolValue))
                    {
                        return boolValue;
                    }
                    break;
            }
            return value;
        }
        private static object PerformOperation(object left, object right, string operation, int lineNumber)
        {
            if (left is float leftFloat && right is float rightFloat)
            {
                if (rightFloat == 0 && (operation == "/" || operation == "%"))
                    throw new ArgumentException($"Error at line: {lineNumber}. Error: Division by zero.");
                return operation switch
                {
                    "+" => leftFloat + rightFloat,
                    "-" => leftFloat - rightFloat,
                    "*" => leftFloat * rightFloat,
                    "/" => leftFloat / rightFloat,
                    "%" => leftFloat % rightFloat,
                    "==" => leftFloat == rightFloat,
                    "<>" => leftFloat != rightFloat,
                    ">" => leftFloat > rightFloat,
                    "<" => leftFloat < rightFloat,
                    ">=" => leftFloat >= rightFloat,
                    "<=" => leftFloat <= rightFloat,
                    _ => throw new ArgumentException($"Error at line: {lineNumber}. Invalid operator '{operation}' for float operands.")
                };
            }
            if (left is int leftInt && right is int rightInt)
            {
                if (rightInt == 0 && (operation == "/" || operation == "%"))
                    throw new ArgumentException($"Error at line: {lineNumber}. Error: Division by zero.");
                return operation switch
                {
                    "+" => HandleIntegerOverflow(leftInt, rightInt, operation, lineNumber),
                    "-" => HandleIntegerOverflow(leftInt, rightInt, operation, lineNumber),
                    "*" => HandleIntegerOverflow(leftInt, rightInt, operation, lineNumber),
                    "/" => leftInt / rightInt,
                    "%" => leftInt % rightInt,
                    "==" => leftInt == rightInt,
                    "<>" => leftInt != rightInt,
                    ">" => leftInt > rightInt,
                    "<" => leftInt < rightInt,
                    ">=" => leftInt >= rightInt,
                    "<=" => leftInt <= rightInt,
                    _ => throw new ArgumentException($"Error at line: {lineNumber}. Invalid operator '{operation}' for integer operands.")
                };
            }

            if ((left is char leftChar && right is char rightChar) && (operation == "==" || operation == "<>"))
            {
                return operation == "==" ? left.Equals(right) : !left.Equals(right);
            }
            if ((left is string leftString && right is string rightString) && (operation == "==" || operation == "<>"))
            {
                return operation == "==" ? leftString.Equals(rightString) : !leftString.Equals(rightString);
            }
            if ((left is bool leftBool && right is bool rightBool) && (operation == "==" || operation == "<>"))
            {
                return operation == "==" ? leftBool == rightBool : leftBool != rightBool;
            }

            throw new ArgumentException($"Error at line: {lineNumber}. Invalid operands for operation '{operation}'. Found: '{left}' and '{right}'.");
        }
        private void IncrementVariable(Variable variable)
        {
            object value = context.GetVariable(variable.Name, variable.LineNumber);

            if (value is int intValue)
            {
                context.SetVariable(
                    variable.Name,
                    HandleIntegerOverflow(intValue, 1, "+", variable.LineNumber),
                    variable.LineNumber);
            }
            else
            {
                throw new ArgumentException($"Error at line: {variable.LineNumber}. Can only use increment operator on integers.");
            }
        }

        private static object HandleIntegerOverflow(int left, int right, string operation, int lineNumber)
        {
            try
            {
                return operation switch
                {
                    "+" => checked(left + right),
                    "-" => checked(left - right),
                    "*" => checked(left * right),
                    _ => left / right
                };
            }
            catch (OverflowException)
            {
                throw new ArgumentException($"Error at line: {lineNumber}. Error: Integer overflow.");
            }
        }

        public static int ConvertToInt(object value, int lineNumber)
        {
            if (value is float floatValue)
            {
                return (int)floatValue;
            }
            if (value is double doubleValue)
            {
                return (int)doubleValue;
            }
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, out float result))
                {
                    return (int)result;
                }
                else
                {
                    throw new ArgumentException($"Error at line: {lineNumber}. Cannot convert '{stringValue}' to int.");
                }
            }
            if (value is char charValue)
            {
                if (char.IsDigit(charValue))
                {
                    return charValue - '0';
                }
                else
                {
                    return Convert.ToInt32(value);
                }
            }
            return Convert.ToInt32(value);
        }
        private static object ConvertIfString(object value)
        {
            if (value is string stringValue)
            {
                if (int.TryParse(stringValue, out int intValue))
                {
                    return intValue;
                }
                if (float.TryParse(stringValue, out float floatValue))
                {
                    return floatValue;
                }
                if (double.TryParse(stringValue, out double doubleValue))
                {
                    return doubleValue;
                }
            }
            return value;
        }
        public static string ConvertToString(object value)
        {
            return value switch
            {
                bool boolValue => boolValue ? "OO" : "DILI",
                float floatValue => floatValue % 1 == 0 ? $"{floatValue}.0" : floatValue.ToString(),
                null => "",
                _ => value.ToString()
            };
        }
        private object GetDefaultValue(TokenType type)
        {
            return type switch
            {
                TokenType.NUMERO => 0,
                TokenType.TIPIK => 0.0f,
                TokenType.LETRA => '\0',
                TokenType.TINUOD => false,
                TokenType.PULONG => "",
                _ => null
            };
        }
        private bool IsTruthy(object value)
        {
            if (value is bool b)
                return b;

            if (value is string str)
                return str == "OO";

            return value != null;
        }

        #endregion HELPER METHODS
    }
}
