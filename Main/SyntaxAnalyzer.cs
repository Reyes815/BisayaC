using static BisayaC.ErrorStatements;
using static BisayaC.ErrorStatements.ErrorType;
using LexicalAnalyzer;
namespace BisayaC
{
    public class SyntaxAnalyzer
    {
        private readonly List<Token> tokens;
        private readonly List<Statement> statements = new List<Statement>();
        private readonly Dictionary<string, TokenType> declaredVariables = new Dictionary<string, TokenType>();
        private int current = 0;
        private bool isInsideDisplay = false; 
        private bool isInsideConditional = false; 
        private bool isInsideIfBlock = false;
        
        public SyntaxAnalyzer(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        #region Public Parsing Methods
        
        public ProgramNode Parse()
        {
            try
            {
                ValidateProgramStructure();
                
                Consume(TokenType.SUGOD, MissingToken, "SUGOD", "at the beginning of the program");
                ConsumeNewlines();
                
                while (!IsAtEnd() && !Check(TokenType.KATAPUSAN))
                {
                    Statement stmt = BeginParsing();
                    if (stmt != null)
                    {
                        ConsumeNewlines();
                        statements.Add(stmt);
                        ConsumeNewlines();
                    }
                }
                
                Consume(TokenType.KATAPUSAN, MissingToken, "KATAPUSAN", "at the end of the program");

                return new ProgramNode(statements);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Environment.Exit(1);
                return null;
            }
        }

        #endregion Public Parsing Methods

        #region Statement Parsing
        
        private Statement BeginParsing()
        {
            try
            {
                if (Match(TokenType.MUGNA))
                {
                    return ParseDeclaration();
                }
                return ParseStatement();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private Statement ParseDeclaration()
        {
            if (!Match(TokenType.NUMERO, TokenType.PULONG, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK))
            {
                ThrowError(Peek().Line, MissingToken, "variable type after 'MUGNA'");
            }

            TokenType type = Previous().Type;
            var variables = new List<Variable>();

            do
            {
                if (IsReservedKeyword(Peek().Value))
                {
                    ThrowError(Peek().Line, KeywordIsReserved, Peek().Value);
                }

                string name = Consume(TokenType.IDENTIFIER, MissingToken, "a valid variable name", $"found '{Peek().Value}'").Value;
                if (declaredVariables.ContainsKey(name))
                {
                    ThrowError(Peek().Line, KeywordIsReserved, name, "Variable already declared.");
                }

                Expression initializer = null;
                if (Match(TokenType.ASAYNMENT))
                {
                    initializer = ParseExpression();
                }

                if (type == TokenType.TINUOD && initializer is LiteralExpression lit && lit.Value is string)
                {
                    string boolValue = lit.Value.ToString();
                    if (boolValue != "OO" && boolValue != "DILI")
                    {
                        ThrowError(Peek().Line, General, $"Boolean values must be either 'OO' or 'DILI'. Found: {boolValue}");
                    }
                }

                variables.Add(new Variable(name, Previous().Line, initializer));
                declaredVariables.Add(name, type);
            } while (Match(TokenType.KAMA));

            if (Peek().Type == TokenType.IDENTIFIER)
            {
                ThrowError(Peek().Line, MissingToken, ",", "for multiple declarations on one line");
            }

            if (Match(TokenType.NUMERO, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK, TokenType.PULONG))
            {
                ThrowError(Peek().Line, General, $"Improper declaration. Cause: '{Previous().Value}'");
            }

            if (Match(TokenType.MUGNA))
            {
                ThrowError(Peek().Line, General, "Another 'MUGNA' statement detected");
            }

            return new DeclarationStatement(type, variables, Previous().Line);
        }
        
        private Statement ParseStatement()
        {
            if (Match(TokenType.KUNG))
            {
                return IfStatement();
            }
            if (Match(TokenType.ALANG))
            {
                return ForStatement();
            }
            if (Match(TokenType.SAMTANG))
            {
                return WhileStatement();
            }
            if (Match(TokenType.IPAKITA))
            {
                return OutputStatement();
            }
            if (Match(TokenType.DAWAT))
            {
                return InputStatement();
            }

            if (Check(TokenType.IDENTIFIER) || Check(TokenType.KAMA))
            {
                if (statements.Count > 0 &&
                    Statement(statements.Count - 1) is AssignmentStatement &&
                    Previous().Type != TokenType.SUNODLINYA)
                {
                    Consume(TokenType.KAMA, MissingToken, ",", "after an assignment");
                }

                if (!declaredVariables.ContainsKey(Peek().Value) && Check(TokenType.IDENTIFIER))
                {
                    ThrowError(Peek().Line, VariableNotDeclared, Peek().Value);
                }
                
                if (Match(TokenType.IDENTIFIER))
                {
                    Token identifierToken = Previous();
                    if (Match(TokenType.INCREMENT))
                    {
                        return new IncrementStatement(new Variable(identifierToken.Value, Previous().Line), Previous().Line);
                    }
                    current--;
                }

                return ParseAssignmentStatement();
            }

            if (Check(TokenType.UNKNOWN))
            {
                ThrowError(Peek().Line, General, $"Unknown character '{Peek().Value}'");
            }
            
            if (isInsideIfBlock)
                return new EmptyStatement(Peek().Line);

            ThrowError(Peek().Line, General, $"Invalid statement. Cause: '{Peek().Value}'");
            return null;
        }
        
        private AssignmentStatement ParseAssignmentStatement()
        {
            Token name = Consume(TokenType.IDENTIFIER, MissingToken, "variable name");
            if (IsReservedKeyword(name.Value))
            {
                ThrowError(Peek().Line,KeywordIsReserved, name.Value, "cannot be used as a variable name");
            }

            Token operatorToken = null;
            if (Check(TokenType.ASAYNMENT))
            {
                operatorToken = Peek();
                Advance();
            }
            else
            {
                ThrowError(Peek().Line, MissingToken, "=", "after variable name");
            }

            if (IsReservedKeyword(Peek().Value) && Peek().Type == TokenType.IDENTIFIER)
            {
                ThrowError(Peek().Line, KeywordIsReserved, Peek().Value, $"cannot be assigned to variable '{name.Value}'. Enclose boolean literals in double quotes.");
            }

            Expression value = ParseExpression();

            if (name.Type == TokenType.IDENTIFIER && value is LiteralExpression literal && literal.Value is string)
            {
                if (GetVariableType(name.Value) == TokenType.TINUOD)
                {
                    string boolValue = literal.Value.ToString();
                    if (boolValue != "OO" && boolValue != "DILI")
                    {
                        ThrowError(Peek().Line, General, $"Boolean values must be either 'OO' or 'DILI'. Found: {boolValue}");
                    }
                }

                if (GetVariableType(name.Value) == TokenType.TIPIK)
                {
                    ThrowError(Peek().Line, General, $"Cannot assign a string literal to character variable '{name.Value}'. Use single quotes for characters.");
                }
            }

            return new AssignmentStatement(new Variable(name.Value, value.LineNumber), operatorToken, value, value.LineNumber);
        }
        
        private Statement OutputStatement()
        {
            isInsideDisplay = true;
            Consume(TokenType.DUHATULDOK, MissingToken, ":", "after 'IPAKITA' statement");

            var expressions = new List<Expression>();

            if ((Check(TokenType.SUNODLINYA) && Peek().Value != "$") || Check(TokenType.KATAPUSAN))
            {
                ThrowError(Peek().Line, General, "Nothing to display.");
            }

            do
            {
                if (Check(TokenType.SUNODLINYA))
                {
                    Advance();
                    expressions.Add(new LiteralExpression("\n", Previous().Line));
                    continue;
                }
                expressions.Add(ParseExpression());
            } while (!Check(TokenType.KATAPUSAN) && !IsAtEnd() && !Peek().Value.Contains("\\n"));

            if (!declaredVariables.ContainsKey(Previous().Value) && Previous().Type == TokenType.IDENTIFIER)
            {
                ThrowError(Peek().Line, VariableNotDeclared, Previous().Value);
            }

            isInsideDisplay = false;
            return new OutputStatement(expressions, Previous().Line);
        }
        
        private Statement InputStatement()
        {
            Consume(TokenType.DUHATULDOK, MissingToken, ":", "after 'DAWAT' statement");

            var variables = new List<Variable>();

            do
            {
                if (!declaredVariables.ContainsKey(Peek().Value))
                {
                    if (IsReservedKeyword(Peek().Value))
                    {
                        ThrowError(Peek().Line, KeywordIsReserved, Peek().Value);
                    }
                    bool isIdentifier = Peek().Type == TokenType.IDENTIFIER;
                    ThrowError(Peek().Line, isIdentifier ? VariableNotDeclared : General, Peek().Value);
                }

                variables.Add(new Variable(Consume(TokenType.IDENTIFIER, MissingToken, "variable name for input").Value, Previous().Line));
            } while (Match(TokenType.KAMA));

            if (Check(TokenType.IDENTIFIER))
                ThrowError(Peek().Line, MissingToken, "comma", $"between variables, received '{Peek().Value}'.");

            return new InputStatement(variables, Peek().Line);
        }
        
        private Statement IfStatement()
        {
            Consume(TokenType.ABLIKUTOB, MissingToken, "(", "after 'KUNG'");
            isInsideConditional = true;
            Expression condition = ParseExpression();
            isInsideConditional = false;
            Consume(TokenType.SIRAKUTOB, MissingToken, ")", "after condition");
            Advance();

            Consume(TokenType.PUNDOK, MissingToken, "PUNDOK");
            isInsideIfBlock = true;
            List<Statement> thenBranch = Block();
            List<Statement> elseBranch = null;

            ConsumeNewlines();
            if (Match(TokenType.KUNG))
            {
                ConsumeNewlines();
                if (Match(TokenType.DILI))
                {
                    elseBranch = new List<Statement> { IfStatement() };
                }
                else if (Match(TokenType.WALA))
                {
                    ConsumeNewlines();
                    Consume(TokenType.PUNDOK, MissingToken, "PUNDOK", "after 'WALA'");
                    elseBranch = Block();
                }
                else
                {
                    tokens.Add(Previous());
                    current--;
                }
            }
            isInsideIfBlock = false;
            return new IfStatement(condition, thenBranch, elseBranch, Previous().Line);
        }
        
        private Statement ForStatement()
        {
            Consume(TokenType.SA, MissingToken, "SA", "after 'ALANG'");
            Consume(TokenType.ABLIKUTOB, MissingToken, "(", "after 'SA' in for loop header");

            AssignmentStatement initialization = ParseAssignmentStatement();
            Consume(TokenType.KAMA, MissingToken, ",", "after initialization in for loop header");

            Expression condition = ParseExpression();
            Consume(TokenType.KAMA, MissingToken, ",", "after condition in for loop header");

            
            Expression update = ParseExpression();
            Consume(TokenType.SIRAKUTOB, MissingToken, ")", "after for loop header");

            ConsumeNewlines();
            Consume(TokenType.PUNDOK, MissingToken, "PUNDOK", "for for loop body");
            List<Statement> body = Block();

            // TODO: Implement for loop
            return new ForLoopStatement(initialization, condition, update, body, Previous().Line);
        }
        
        private Statement WhileStatement()
        {
            Consume(TokenType.ABLIKUTOB, MissingToken, "(", "after 'SAMTANG'");
            isInsideConditional = true;
            Expression condition = ParseExpression();
            isInsideConditional = false;
            Consume(TokenType.SIRAKUTOB, MissingToken, ")", "after condition");
            Advance();

            Consume(TokenType.PUNDOK, MissingToken, "PUNDOK", "for while loop body");
            List<Statement> body = Block();

            return new WhileStatement(condition, body, Previous().Line);
        }
        
        private List<Statement> Block()
        {
            var blockStatements = new List<Statement>();
            Consume(TokenType.SUGODKUNG, VariableNotDeclared, "{", "before block");
            ConsumeNewlines();

            while (!Check(TokenType.HUMANKUNG) && !Check(TokenType.KATAPUSAN) && !IsAtEnd())
            {
                Statement stmt = BeginParsing();
                if (stmt != null)
                {
                    ConsumeNewlines();
                    blockStatements.Add(stmt);
                    ConsumeNewlines();
                }
            }

            Consume(TokenType.HUMANKUNG, VariableNotDeclared, "}", "after block");
            ConsumeNewlines();
            return blockStatements;
        }

        #endregion Statement Parsing

        #region Expression Parsing
        
        private Expression ParseExpression() => Assignment();
        
        private Expression Assignment()
        {
            Expression expr = LogicOr();

            if (Match(TokenType.ASAYNMENT))
            {
                if (isInsideConditional)
                {
                    ThrowError(Peek().Line, General, "Cannot use assignment operator '=' within conditional statements.");
                }

                Token op = Previous();
                Expression value = Assignment();

                if (expr is VariableExpression varExpr)
                {
                    return new AssignmentExpression(new Variable(varExpr.Name, varExpr.LineNumber), op, value, varExpr.LineNumber);
                }

                ThrowError(Peek().Line,  AssignmentTargetInvalid);
            }

            return expr;
        }
        
        private Expression LogicOr()
        {
            Expression expr = LogicAnd();
            while (Match(TokenType.O))
            {
                Token op = Previous();
                Expression right = LogicAnd();
                expr = new LogicalExpression(expr, op, right, op.Line);
            }
            return expr;
        }
        
        private Expression LogicAnd()
        {
            Expression expr = Equality();
            while (Match(TokenType.UG))
            {
                Token op = Previous();
                Expression right = Equality();
                expr = new LogicalExpression(expr, op, right, op.Line);
            }
            return expr;
        }

        private Expression Equality()
        {
            Expression expr = Comparison();
            while (Match(TokenType.PAREHAS, TokenType.LAHI))
            {
                Token op = Previous();
                Expression right = Comparison();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }
            return expr;
        }
        
        private Expression Comparison()
        {
            Expression expr = Term();
            while (Match(TokenType.LABAW, TokenType.UBOS, TokenType.LABAWSA, TokenType.UBOSSA))
            {
                Token op = Previous();
                Expression right = Term();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }
            return expr;
        }
        
        private Expression Term()
        {
            Expression expr = Factor();
            while (Match(TokenType.DUGANG, TokenType.KUHA, TokenType.SUMPAY))
            {
                if (Previous().Type == TokenType.SUMPAY && !isInsideDisplay)
                {
                    ThrowError(Peek().Line, General, "Cannot perform concatenation '&' outside IPAKITA statement.");
                }
                Token op = Previous();
                Expression right = Factor();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }
            return expr;
        }


        private Expression Factor()
        {
            Expression expr = Unary();
            while (Match(TokenType.PADAGHAN, TokenType.BAHIN, TokenType.SOBRA))
            {
                Token op = Previous();
                Expression right = Unary();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }
            return expr;
        }


        private Expression Unary()
        {
            if (Check(TokenType.DILI) && Peek().Value == "DILI")
            {
                return Primary();
            }

            if (Match(TokenType.KUHA, TokenType.DILI))
            {
                Token op = Previous();
                Expression right = Unary();
                return new UnaryExpression(op, right, op.Line);
            }

            return Primary();
        }

        private Expression Primary()
        {
            if (Match(TokenType.OO, TokenType.DILI))
            {
                bool value = Previous().Type == TokenType.OO;
                return new LiteralExpression(value, Previous().Line);
            }

            if (Match(TokenType.INTEGERLITERAL))
            {
                int value = int.Parse(Previous().Value);
                return new LiteralExpression(value, Previous().Line);
            }

            if (Match(TokenType.FLOATLITERAL))
            {
                float value = float.Parse(Previous().Value);
                return new LiteralExpression(value, Previous().Line);
            }

            if (Match(TokenType.CHARACTERLITERAL))
            {
                char value = Previous().Value[0];
                return new LiteralExpression(value, Previous().Line);
            }

            if (Match(TokenType.STRINGLITERAL))
            {
                string value = Previous().Value;
                return new LiteralExpression(value, Previous().Line);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                VariableExpression varExpr = new VariableExpression(Previous().Value, Previous().Line);
                if (Match(TokenType.INCREMENT))
                {
                    Token operatorToken = Previous();
                    return new UnaryExpression(operatorToken, varExpr, Previous().Line);
                }
                return new VariableExpression(Previous().Value, Previous().Line);
            }

            if (Match(TokenType.ABLIKUTOB))
            {
                Expression expr = ParseExpression();
                Consume(TokenType.SIRAKUTOB,  MissingToken, ")", "after expression");
                return new GroupingExpression(expr, Previous().Line);
            }

            if (Match(TokenType.UNKNOWN))
            {
                ThrowError(Peek().Line, General, $"Unknown character '{Previous().Value}'");
            }

            ThrowError(Peek().Line, General, "Invalid/empty expression.");
            return null;
        }

        #endregion Expression Parsing

        #region Helper Methods
        private void ValidateProgramStructure()
        {
            int sugodIndex = tokens.FindIndex(token => token.Type == TokenType.SUGOD);
            int katapusanIndex = tokens.FindLastIndex(token => token.Type == TokenType.KATAPUSAN);

            ConsumeNewlines();

            if (sugodIndex == -1 || katapusanIndex == -1)
            {
                string missingToken = sugodIndex == -1 ? "SUGOD" : "KATAPUSAN";
                ThrowError(Peek().Line, General, $"{missingToken} must exist for the program to run.");
            }

            if (tokens.Count(token => token.Type == TokenType.SUGOD) > 1 || tokens.Count(token => token.Type == TokenType.KATAPUSAN) > 1)
            {
                int duplicateSugodIndex = tokens.FindIndex(sugodIndex + 1, token => token.Type == TokenType.SUGOD);
                int duplicateKatapusanIndex = tokens.FindIndex(token => token.Type == TokenType.KATAPUSAN);
                int errorLine = duplicateSugodIndex > -1 ? tokens[duplicateSugodIndex].Line : tokens[duplicateKatapusanIndex].Line;
                string duplicateToken = duplicateSugodIndex > -1 ? "SUGOD" : "KATAPUSAN";
                ThrowError(errorLine, General, $"Only one {duplicateToken} should exist.");
            }

            if (sugodIndex > 0 || katapusanIndex < tokens.Count - 1)
            {
                if (sugodIndex > 0)
                {
                    var outOfBoundsToken = tokens.Take(sugodIndex).FirstOrDefault(token => token.Type != TokenType.STORYA && token.Type != TokenType.SUNODLINYA);
                    if (outOfBoundsToken != null)
                    {
                        ThrowError(outOfBoundsToken.Line, General, $"Invalid code or tokens outside SUGOD.");
                    }
                }
                if (katapusanIndex < tokens.Count - 1)
                {
                    var outOfBoundsToken = tokens.Skip(katapusanIndex + 1).FirstOrDefault(token => token.Type != TokenType.STORYA && token.Type != TokenType.SUNODLINYA && token.Type != TokenType.EOF);
                    if (outOfBoundsToken != null)
                    {
                        ThrowError(outOfBoundsToken.Line, General, $"Invalid code or tokens after KATAPUSAN.");
                    }
                }
            }
        }
        
        private TokenType GetVariableType(string variableName)
        {
            if (declaredVariables.TryGetValue(variableName, out TokenType type))
            {
                return type;
            }
            ThrowError(Peek().Line, VariableNotDeclared, variableName);
            return default; 
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Peek() => tokens[current];

        private Token Previous() => tokens[current - 1];

        private Token Advance()
        {
            if (!IsAtEnd())
                current++;
            return Previous();
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        
        private Token Consume(TokenType type, ErrorType errorCode, string expected, string additionalInfo = null)
        {
            if (Check(type))
                return Advance();
            ThrowError(Peek().Line, errorCode, expected, additionalInfo);
            return null;
        }
        
        private void ConsumeNewlines()
        {
            while (Match(TokenType.SUNODLINYA)) { }
        }

        private static bool IsReservedKeyword(string name) => LexerAnalyzer.keywords.ContainsKey(name);

        private Statement Statement(int index)
        {
            try
            {
                return statements[index];
            }
            catch (Exception ex)
            {
                ThrowError(Peek().Line, General, ex.Message);
                return null;
            }
        }

        #endregion Helper Methods
    }
}
