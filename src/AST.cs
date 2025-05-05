namespace Bisaya__
{
    /// <summary>
    /// Interface for all nodes in the abstract syntax tree (AST).
    /// </summary>
    public interface IAstNode { }

    /// <summary>
    /// Represents the entire program as a root AST node.
    /// </summary>
    public class ProgramNode : IAstNode
    {
        /// <summary>
        /// Gets the list of statements in the program.
        /// </summary>
        public List<Statement> Statements { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramNode"/> class.
        /// </summary>
        /// <param name="statements">The list of program statements.</param>
        public ProgramNode(List<Statement> statements)
        {
            Statements = statements;
        }
    }

    /// <summary>
    /// Base class for all statement nodes.
    /// </summary>
    public abstract class Statement : IAstNode
    {
        /// <summary>
        /// Gets the line number where the statement appears.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class.
        /// </summary>
        /// <param name="lineNumber">The line number of the statement.</param>
        protected Statement(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }

    /// <summary>
    /// Represents an empty statement (no operation).
    /// </summary>
    public class EmptyStatement : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyStatement"/> class.
        /// </summary>
        /// <param name="lineNumber">The line number of the empty statement.</param>
        public EmptyStatement(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a variable declaration statement.
    /// </summary>
    public class DeclarationStatement : Statement
    {
        /// <summary>
        /// Gets the type of the variable being declared.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Gets the list of variables declared.
        /// </summary>
        public List<Variable> Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarationStatement"/> class.
        /// </summary>
        /// <param name="type">The variable type.</param>
        /// <param name="variables">The list of variables.</param>
        /// <param name="lineNumber">The line number of the declaration.</param>
        public DeclarationStatement(TokenType type, List<Variable> variables, int lineNumber) : base(lineNumber)
        {
            Type = type;
            Variables = variables;
        }
    }

    /// <summary>
    /// Represents an assignment statement.
    /// </summary>
    public class AssignmentStatement : Statement
    {
        /// <summary>
        /// Gets the variable that is being assigned a value.
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Gets the assignment operator token.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the expression that provides the new value.
        /// </summary>
        public Expression Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentStatement"/> class.
        /// </summary>
        /// <param name="variable">The variable on the left-hand side.</param>
        /// <param name="operatorToken">The assignment operator token.</param>
        /// <param name="value">The value expression.</param>
        /// <param name="lineNumber">The line number of the assignment.</param>
        public AssignmentStatement(Variable variable, Token operatorToken, Expression value, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
            Operator = operatorToken;
            Value = value;
        }
    }

    /// <summary>
    /// Represents an increment statement for a variable.
    /// </summary>
    public class IncrementStatement : Statement
    {
        /// <summary>
        /// Gets the variable that is to be incremented.
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementStatement"/> class.
        /// </summary>
        /// <param name="variable">The variable to be incremented.</param>
        /// <param name="lineNumber">The line number in the source code where this statement is located.</param>
        public IncrementStatement(Variable variable, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
        }
    }

    /// <summary>
    /// Represents an if statement, including its condition, then branch, and optional else branch.
    /// </summary>
    public class IfStatement : Statement
    {
        /// <summary>
        /// Gets the condition expression of the if statement.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Gets the list of statements in the then branch.
        /// </summary>
        public List<Statement> ThenBranch { get; }

        /// <summary>
        /// Gets the list of statements in the else branch.
        /// </summary>
        public List<Statement> ElseBranch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IfStatement"/> class.
        /// </summary>
        /// <param name="condition">The condition expression.</param>
        /// <param name="thenBranch">The then branch statements.</param>
        /// <param name="elseBranch">The else branch statements (optional).</param>
        /// <param name="lineNumber">The line number of the if statement.</param>
        public IfStatement(Expression condition, List<Statement> thenBranch, List<Statement>? elseBranch, int lineNumber) : base(lineNumber)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch ?? new List<Statement>();
        }
    }

    /// <summary>
    /// Represents a for-loop statement.
    /// </summary>
    public class ForLoopStatement : Statement
    {
        /// <summary>
        /// Gets the initialization statement.
        /// </summary>
        public Statement Initialization { get; }

        /// <summary>
        /// Gets the condition expression.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Gets the update expression.
        /// </summary>
        public Expression Update { get; }

        /// <summary>
        /// Gets the list of statements in the loop body.
        /// </summary>
        public List<Statement> Body { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForLoopStatement"/> class.
        /// </summary>
        /// <param name="initialization">The initialization statement.</param>
        /// <param name="condition">The loop condition expression.</param>
        /// <param name="update">The update expression.</param>
        /// <param name="body">The loop body statements.</param>
        /// <param name="lineNumber">The line number of the for-loop.</param>
        public ForLoopStatement(Statement initialization, Expression condition, Expression update, List<Statement> body, int lineNumber) : base(lineNumber)
        {
            Initialization = initialization;
            Condition = condition;
            Update = update;
            Body = body;
        }
    }

    /// <summary>
    /// Represents a while loop statement.
    /// </summary>
    public class WhileStatement : Statement
    {
        /// <summary>
        /// Gets the loop condition expression.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Gets the list of statements in the loop body.
        /// </summary>
        public List<Statement> Body { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhileStatement"/> class.
        /// </summary>
        /// <param name="condition">The loop condition expression.</param>
        /// <param name="body">The loop body statements.</param>
        /// <param name="lineNumber">The line number of the while loop.</param>
        public WhileStatement(Expression condition, List<Statement> body, int lineNumber) : base(lineNumber)
        {
            Condition = condition;
            Body = body;
        }
    }

    /// <summary>
    /// Represents an output (display) statement.
    /// </summary>
    public class OutputStatement : Statement
    {
        /// <summary>
        /// Gets the list of expressions to be output.
        /// </summary>
        public List<Expression> Expressions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputStatement"/> class.
        /// </summary>
        /// <param name="expressions">The expressions to output.</param>
        /// <param name="lineNumber">The line number of the output statement.</param>
        public OutputStatement(List<Expression> expressions, int lineNumber) : base(lineNumber)
        {
            Expressions = expressions;
        }
    }

    /// <summary>
    /// Represents an input (scan) statement.
    /// </summary>
    public class InputStatement : Statement
    {
        /// <summary>
        /// Gets the list of variables that will receive input.
        /// </summary>
        public List<Variable> Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputStatement"/> class.
        /// </summary>
        /// <param name="variables">The variables to receive input.</param>
        /// <param name="lineNumber">The line number of the input statement.</param>
        public InputStatement(List<Variable> variables, int lineNumber) : base(lineNumber)
        {
            Variables = variables;
        }
    }

    /// <summary>
    /// Base class for all expression nodes.
    /// </summary>
    public abstract class Expression : IAstNode
    {
        /// <summary>
        /// Gets the line number where the expression appears.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="lineNumber">The line number of the expression.</param>
        protected Expression(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }

    /// <summary>
    /// Represents a binary operation expression.
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// Gets the left-hand side expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Gets the operator token.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the right-hand side expression.
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="operatorToken">The operator token.</param>
        /// <param name="right">The right expression.</param>
        /// <param name="lineNumber">The line number of the expression.</param>
        public BinaryExpression(Expression left, Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Left = left;
            Operator = operatorToken;
            Right = right;
        }
    }

    /// <summary>
    /// Represents a unary operation expression.
    /// </summary>
    public class UnaryExpression : Expression
    {
        /// <summary>
        /// Gets the operator token.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the expression to which the operator is applied.
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryExpression"/> class.
        /// </summary>
        /// <param name="operatorToken">The operator token.</param>
        /// <param name="right">The operand expression.</param>
        /// <param name="lineNumber">The line number of the expression.</param>
        public UnaryExpression(Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Operator = operatorToken;
            Right = right;
        }
    }

    /// <summary>
    /// Represents a literal value expression.
    /// </summary>
    public class LiteralExpression : Expression
    {
        /// <summary>
        /// Gets the literal value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralExpression"/> class.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <param name="lineNumber">The line number of the literal.</param>
        public LiteralExpression(object value, int lineNumber) : base(lineNumber)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Represents a variable declaration.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the optional initializer expression.
        /// </summary>
        public Expression? Initializer { get; }

        /// <summary>
        /// Gets the line number where the variable is declared.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="lineNumber">The line number of the declaration.</param>
        /// <param name="initializer">The optional initializer expression.</param>
        public Variable(string name, int lineNumber, Expression? initializer = null)
        {
            Name = name;
            LineNumber = lineNumber;
            Initializer = initializer;
        }
    }

    /// <summary>
    /// Represents a variable usage expression.
    /// </summary>
    public class VariableExpression : Expression
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableExpression"/> class.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="lineNumber">The line number where the variable is used.</param>
        public VariableExpression(string name, int lineNumber) : base(lineNumber)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Represents an assignment expression.
    /// </summary>
    public class AssignmentExpression : Expression
    {
        /// <summary>
        /// Gets the variable that is being assigned to.
        /// </summary>
        public Variable Variable { get; }

        /// <summary>
        /// Gets the assignment operator token.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the expression providing the new value.
        /// </summary>
        public Expression Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentExpression"/> class.
        /// </summary>
        /// <param name="variable">The variable on the left-hand side.</param>
        /// <param name="operatorToken">The assignment operator token.</param>
        /// <param name="value">The value expression.</param>
        /// <param name="lineNumber">The line number of the expression.</param>
        public AssignmentExpression(Variable variable, Token operatorToken, Expression value, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
            Operator = operatorToken;
            Value = value;
        }
    }

    /// <summary>
    /// Represents a logical operation expression.
    /// </summary>
    public class LogicalExpression : Expression
    {
        /// <summary>
        /// Gets the left-hand side expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Gets the operator token.
        /// </summary>
        public Token Operator { get; }

        /// <summary>
        /// Gets the right-hand side expression.
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalExpression"/> class.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="operatorToken">The operator token.</param>
        /// <param name="right">The right expression.</param>
        /// <param name="lineNumber">The line number of the expression.</param>
        public LogicalExpression(Expression left, Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Left = left;
            Operator = operatorToken;
            Right = right;
        }
    }

    /// <summary>
    /// Represents a grouped (parenthesized) expression.
    /// </summary>
    public class GroupingExpression : Expression
    {
        /// <summary>
        /// Gets the inner expression.
        /// </summary>
        public Expression InnerExpression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression inside the grouping.</param>
        /// <param name="lineNumber">The line number of the grouped expression.</param>
        public GroupingExpression(Expression expression, int lineNumber) : base(lineNumber)
        {
            InnerExpression = expression;
        }
    }
}
