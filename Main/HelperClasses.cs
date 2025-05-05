using LexicalAnalyzer;
namespace BisayaC
{

    public interface IAstNode { }
    
    public class ProgramNode : IAstNode
    {
        public List<Statement> Statements { get; }
        
        public ProgramNode(List<Statement> statements)
        {
            Statements = statements;
        }
    }

    public abstract class Statement : IAstNode
    {
        public int LineNumber { get; }
        
        protected Statement(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }
    
    public class EmptyStatement : Statement
    {
        public EmptyStatement(int lineNumber) : base(lineNumber) { }
    }
    
    public class DeclarationStatement : Statement
    {

        public TokenType Type { get; }


        public List<Variable> Variables { get; }
        
        public DeclarationStatement(TokenType type, List<Variable> variables, int lineNumber) : base(lineNumber)
        {
            Type = type;
            Variables = variables;
        }
    }
    
    public class AssignmentStatement : Statement
    {
        public Variable Variable { get; }

        public Token Operator { get; }


        public Expression Value { get; }
        
        public AssignmentStatement(Variable variable, Token operatorToken, Expression value, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
            Operator = operatorToken;
            Value = value;
        }
    }

    public class IncrementStatement : Statement
    {

        public Variable Variable { get; }
        
        public IncrementStatement(Variable variable, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
        }
    }


    public class IfStatement : Statement
    {

        public Expression Condition { get; }
        
        public List<Statement> ThenBranch { get; }

        public List<Statement> ElseBranch { get; }
        
        public IfStatement(Expression condition, List<Statement> thenBranch, List<Statement>? elseBranch, int lineNumber) : base(lineNumber)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch ?? new List<Statement>();
        }
    }


    public class ForLoopStatement : Statement
    {

        public Statement Initialization { get; }


        public Expression Condition { get; }
        
        public Expression Update { get; }


        public List<Statement> Body { get; }
        
        public ForLoopStatement(Statement initialization, Expression condition, Expression update, List<Statement> body, int lineNumber) : base(lineNumber)
        {
            Initialization = initialization;
            Condition = condition;
            Update = update;
            Body = body;
        }
    }


    public class WhileStatement : Statement
    {

        public Expression Condition { get; }
        
        public List<Statement> Body { get; }


        public WhileStatement(Expression condition, List<Statement> body, int lineNumber) : base(lineNumber)
        {
            Condition = condition;
            Body = body;
        }
    }


    public class OutputStatement : Statement
    {

        public List<Expression> Expressions { get; }
        
        public OutputStatement(List<Expression> expressions, int lineNumber) : base(lineNumber)
        {
            Expressions = expressions;
        }
    }


    public class InputStatement : Statement
    {

        public List<Variable> Variables { get; }

        public InputStatement(List<Variable> variables, int lineNumber) : base(lineNumber)
        {
            Variables = variables;
        }
    }
    
    public abstract class Expression : IAstNode
    {
        public int LineNumber { get; }
        
        protected Expression(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }
    
    public class BinaryExpression : Expression
    {

        public Expression Left { get; }
        
        public Token Operator { get; }
        
        public Expression Right { get; }
        
        public BinaryExpression(Expression left, Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Left = left;
            Operator = operatorToken;
            Right = right;
        }
    }
    
    public class UnaryExpression : Expression
    {
        public Token Operator { get; }
        
        public Expression Right { get; }
        
        public UnaryExpression(Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Operator = operatorToken;
            Right = right;
        }
    }
    
    public class LiteralExpression : Expression
    {
        public object Value { get; }

        public LiteralExpression(object value, int lineNumber) : base(lineNumber)
        {
            Value = value;
        }
    }
    
    public class Variable
    {
        public string Name { get; }
        public Expression? Initializer { get; }
        
        public int LineNumber { get; }
        
        public Variable(string name, int lineNumber, Expression? initializer = null)
        {
            Name = name;
            LineNumber = lineNumber;
            Initializer = initializer;
        }
    }
    
    public class VariableExpression : Expression
    {
        public string Name { get; }
        public VariableExpression(string name, int lineNumber) : base(lineNumber)
        {
            Name = name;
        }
    }
    public class AssignmentExpression : Expression
    {
        public Variable Variable { get; }
        public Token Operator { get; }
        
        public Expression Value { get; }
        
        public AssignmentExpression(Variable variable, Token operatorToken, Expression value, int lineNumber) : base(lineNumber)
        {
            Variable = variable;
            Operator = operatorToken;
            Value = value;
        }
    }
    
    public class LogicalExpression : Expression
    {
        public Expression Left { get; }
        
        public Token Operator { get; }
        
        public Expression Right { get; }
        
        public LogicalExpression(Expression left, Token operatorToken, Expression right, int lineNumber) : base(lineNumber)
        {
            Left = left;
            Operator = operatorToken;
            Right = right;
        }
    }
    
    public class GroupingExpression : Expression
    {
        public Expression InnerExpression { get; }
        
        public GroupingExpression(Expression expression, int lineNumber) : base(lineNumber)
        {
            InnerExpression = expression;
        }
    }
}
