using static Bisaya__.ErrorHandler;
using static Bisaya__.ErrorHandler.ErrorCode;

namespace Bisaya__
{
    /// <summary>
    /// Parses a list of tokens into a ProgramNode AST.
    /// </summary>
    public class Parser
    {
        private readonly List<Token> tokens;
        private readonly List<Statement> statements = new List<Statement>();
        private readonly Dictionary<string, TokenType> declaredVariables = new Dictionary<string, TokenType>();
        private int current = 0;
        private bool isInsideDisplay = false; // Track if inside a display statement
        private bool isInsideConditional = false; // Track if inside a conditional statement
        private bool isInsideIfBlock = false; // Track if inside an if block

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse.</param>
        /// <param name="debugMode">If set to true, enables detailed logging.</param>
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        #region Public Parsing Methods

        /// <summary>
        /// Parses the token stream and returns the resulting AST.
        /// </summary>
        /// <returns>A <see cref="ProgramNode"/> representing the entire parsed program.</returns>
        /// <exception cref="Exception">Throws exception if parsing fails.</exception>
        public ProgramNode Parse()
        {
            try
            {
                // Validate the program structure before executing
                ValidateProgramStructure();

                // Expect program start marker "SUGOD"
                Consume(TokenType.SUGOD, ExpectedToken, "SUGOD", "at the beginning of the program");
                ConsumeNewlines();

                // Parse statements until program end marker "KATAPUSAN" is reached
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

                // Expect program end marker "KATAPUSAN"
                Consume(TokenType.KATAPUSAN, ExpectedToken, "KATAPUSAN", "at the end of the program");

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

        /// <summary>
        /// Entry point for parsing a single statement.
        /// </summary>
        /// <returns>A <see cref="Statement"/> or null if an error occurred.</returns>
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

        /// <summary>
        /// Parses a variable declaration statement (MUGNA).
        /// </summary>
        /// <returns>A <see cref="DeclarationStatement"/> representing the variable declaration.</returns>
        private Statement ParseDeclaration()
        {
            if (!Match(TokenType.NUMERO, TokenType.PULONG, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK))
            {
                RaiseError(Peek().Line, ExpectedToken, "variable type after 'MUGNA'");
            }

            TokenType type = Previous().Type;
            var variables = new List<Variable>();

            do
            {
                if (IsReservedKeyword(Peek().Value))
                {
                    RaiseError(Peek().Line, ReservedKeyword, Peek().Value);
                }

                string name = Consume(TokenType.IDENTIFIER, ExpectedToken, "a valid variable name", $"found '{Peek().Value}'").Value;
                if (declaredVariables.ContainsKey(name))
                {
                    RaiseError(Peek().Line, ReservedKeyword, name, "Variable already declared.");
                }

                Expression initializer = null;
                if (Match(TokenType.ASAYNMENT))
                {
                    initializer = ParseExpression();
                }

                // Validate boolean literal assignments for boolean type variables.
                if (type == TokenType.TINUOD && initializer is LiteralExpression lit && lit.Value is string)
                {
                    string boolValue = lit.Value.ToString();
                    if (boolValue != "OO" && boolValue != "DILI")
                    {
                        RaiseError(Peek().Line, Generic, $"Boolean values must be either 'OO' or 'DILI'. Found: {boolValue}");
                    }
                }

                variables.Add(new Variable(name, Previous().Line, initializer));
                declaredVariables.Add(name, type);
            } while (Match(TokenType.KAMA));

            if (Peek().Type == TokenType.IDENTIFIER)
            {
                RaiseError(Peek().Line, ExpectedToken, ",", "for multiple declarations on one line");
            }

            if (Match(TokenType.NUMERO, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK, TokenType.PULONG))
            {
                RaiseError(Peek().Line, Generic, $"Improper declaration. Cause: '{Previous().Value}'");
            }

            if (Match(TokenType.MUGNA))
            {
                RaiseError(Peek().Line, Generic, "Another 'MUGNA' statement detected");
            }

            return new DeclarationStatement(type, variables, Previous().Line);
        }

        /// <summary>
        /// Parses a regular statement.
        /// </summary>
        /// <returns>A <see cref="Statement"/> representing the parsed statement.</returns>
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
                // Ensure proper comma separation in multiple assignments.
                if (statements.Count > 0 &&
                    Statement(statements.Count - 1) is AssignmentStatement &&
                    Previous().Type != TokenType.SUNODLINYA)
                {
                    Consume(TokenType.KAMA, ExpectedToken, ",", "after an assignment");
                }

                // Check for undeclared variables in the current statement.
                if (!declaredVariables.ContainsKey(Peek().Value) && Check(TokenType.IDENTIFIER))
                {
                    RaiseError(Peek().Line, UndeclaredVariable, Peek().Value);
                }

                // Handle increment operator for variables.
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
                RaiseError(Peek().Line, Generic, $"Unknown character '{Peek().Value}'");
            }

            // Allow empty statements in if-else blocks.
            if (isInsideIfBlock)
                return new EmptyStatement(Peek().Line);

            RaiseError(Peek().Line, Generic, $"Invalid statement. Cause: '{Peek().Value}'");
            return null;
        }

        /// <summary>
        /// Parses an assignment statement.
        /// </summary>
        /// <returns>An <see cref="AssignmentStatement"/> representing the assignment.</returns>
        private AssignmentStatement ParseAssignmentStatement()
        {
            Token name = Consume(TokenType.IDENTIFIER, ExpectedToken, "variable name");
            if (IsReservedKeyword(name.Value))
            {
                RaiseError(Peek().Line, ReservedKeyword, name.Value, "cannot be used as a variable name");
            }

            Token operatorToken = null;
            if (Check(TokenType.ASAYNMENT))
            {
                operatorToken = Peek();
                Advance();
            }
            else
            {
                RaiseError(Peek().Line, ExpectedToken, "=", "after variable name");
            }

            if (IsReservedKeyword(Peek().Value) && Peek().Type == TokenType.IDENTIFIER)
            {
                RaiseError(Peek().Line, ReservedKeyword, Peek().Value, $"cannot be assigned to variable '{name.Value}'. Enclose boolean literals in double quotes.");
            }

            Expression value = ParseExpression();

            // Validate boolean and character assignments.
            if (name.Type == TokenType.IDENTIFIER && value is LiteralExpression literal && literal.Value is string)
            {
                if (GetVariableType(name.Value) == TokenType.TINUOD)
                {
                    string boolValue = literal.Value.ToString();
                    if (boolValue != "OO" && boolValue != "DILI")
                    {
                        RaiseError(Peek().Line, Generic, $"Boolean values must be either 'OO' or 'DILI'. Found: {boolValue}");
                    }
                }

                if (GetVariableType(name.Value) == TokenType.TIPIK)
                {
                    RaiseError(Peek().Line, Generic, $"Cannot assign a string literal to character variable '{name.Value}'. Use single quotes for characters.");
                }
            }

            return new AssignmentStatement(new Variable(name.Value, value.LineNumber), operatorToken, value, value.LineNumber);
        }

        /// <summary>
        /// Parses an output statement (IPAKITA).
        /// </summary>
        /// <returns>An <see cref="OutputStatement"/> representing the output statement.</returns>
        private Statement OutputStatement()
        {
            isInsideDisplay = true;
            Consume(TokenType.DUHATULDOK, ExpectedToken, ":", "after 'IPAKITA' statement");

            var expressions = new List<Expression>();

            if ((Check(TokenType.SUNODLINYA) && Peek().Value != "$") || Check(TokenType.KATAPUSAN))
            {
                RaiseError(Peek().Line, Generic, "Nothing to display.");
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
                RaiseError(Peek().Line, UndeclaredVariable, Previous().Value);
            }

            isInsideDisplay = false;
            return new OutputStatement(expressions, Previous().Line);
        }

        /// <summary>
        /// Parses an input statement (DAWAT).
        /// </summary>
        /// <returns>An <see cref="InputStatement"/> representing the input statement.</returns>
        private Statement InputStatement()
        {
            Consume(TokenType.DUHATULDOK, ExpectedToken, ":", "after 'DAWAT' statement");

            var variables = new List<Variable>();

            do
            {
                if (!declaredVariables.ContainsKey(Peek().Value))
                {
                    if (IsReservedKeyword(Peek().Value))
                    {
                        RaiseError(Peek().Line, ReservedKeyword, Peek().Value);
                    }
                    bool isIdentifier = Peek().Type == TokenType.IDENTIFIER;
                    RaiseError(Peek().Line, isIdentifier ? UndeclaredVariable : Generic, Peek().Value);
                }

                variables.Add(new Variable(Consume(TokenType.IDENTIFIER, ExpectedToken, "variable name for input").Value, Previous().Line));
            } while (Match(TokenType.KAMA));

            if (Check(TokenType.IDENTIFIER))
                RaiseError(Peek().Line, ExpectedToken, "comma", $"between variables, received '{Peek().Value}'.");

            return new InputStatement(variables, Peek().Line);
        }

        /// <summary>
        /// Parses an if statement (KUNG), including optional else branches.
        /// </summary>
        /// <returns>An <see cref="IfStatement"/> representing the parsed if statement.</returns>
        private Statement IfStatement()
        {
            Consume(TokenType.ABLIKUTOB, ExpectedToken, "(", "after 'KUNG'");
            isInsideConditional = true;
            Expression condition = ParseExpression();
            isInsideConditional = false;
            Consume(TokenType.SIRAKUTOB, ExpectedToken, ")", "after condition");
            Advance();

            Consume(TokenType.PUNDOK, ExpectedToken, "PUNDOK");
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
                    Consume(TokenType.PUNDOK, ExpectedToken, "PUNDOK", "after 'WALA'");
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

        /// <summary>
        /// Parses a for loop statement (ALANG SA). (Loop body parsing is not implemented.)
        /// </summary>
        /// <returns>An <see cref="Statement"/> representing the for loop header.</returns>
        private Statement ForStatement()
        {
            Consume(TokenType.SA, ExpectedToken, "SA", "after 'ALANG'");
            Consume(TokenType.ABLIKUTOB, ExpectedToken, "(", "after 'SA' in for loop header");

            AssignmentStatement initialization = ParseAssignmentStatement();
            Consume(TokenType.KAMA, ExpectedToken, ",", "after initialization in for loop header");

            Expression condition = ParseExpression();
            Consume(TokenType.KAMA, ExpectedToken, ",", "after condition in for loop header");

            
            Expression update = ParseExpression();
            Consume(TokenType.SIRAKUTOB, ExpectedToken, ")", "after for loop header");

            ConsumeNewlines();
            Consume(TokenType.PUNDOK, ExpectedToken, "PUNDOK", "for for loop body");
            List<Statement> body = Block();

            // TODO: Implement for loop
            return new ForLoopStatement(initialization, condition, update, body, Previous().Line);
        }

        /// <summary>
        /// Parses a while loop statement (SAMTANG).
        /// </summary>
        /// <returns>A <see cref="WhileStatement"/> representing the while loop.</returns>
        private Statement WhileStatement()
        {
            Consume(TokenType.ABLIKUTOB, ExpectedToken, "(", "after 'SAMTANG'");
            isInsideConditional = true;
            Expression condition = ParseExpression();
            isInsideConditional = false;
            Consume(TokenType.SIRAKUTOB, ExpectedToken, ")", "after condition");
            Advance();

            Consume(TokenType.PUNDOK, ExpectedToken, "PUNDOK", "for while loop body");
            List<Statement> body = Block();

            return new WhileStatement(condition, body, Previous().Line);
        }

        /// <summary>
        /// Parses a block of statements enclosed in braces.
        /// </summary>
        /// <returns>A list of <see cref="Statement"/> representing the block.</returns>
        private List<Statement> Block()
        {
            var blockStatements = new List<Statement>();
            Consume(TokenType.SUGODKUNG, ExpectedToken, "{", "before block");
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

            Consume(TokenType.HUMANKUNG, ExpectedToken, "}", "after block");
            ConsumeNewlines();
            return blockStatements;
        }

        #endregion Statement Parsing

        #region Expression Parsing

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <returns>An <see cref="Expression"/> representing the parsed expression.</returns>
        private Expression ParseExpression() => Assignment();

        /// <summary>
        /// Parses an assignment expression.
        /// </summary>
        /// <returns>An <see cref="Expression"/> representing the assignment expression.</returns>
        private Expression Assignment()
        {
            Expression expr = LogicOr();

            if (Match(TokenType.ASAYNMENT))
            {
                if (isInsideConditional)
                {
                    RaiseError(Peek().Line, Generic, "Cannot use assignment operator '=' within conditional statements.");
                }

                Token op = Previous();
                Expression value = Assignment();

                if (expr is VariableExpression varExpr)
                {
                    return new AssignmentExpression(new Variable(varExpr.Name, varExpr.LineNumber), op, value, varExpr.LineNumber);
                }

                RaiseError(Peek().Line, InvalidAssignmentTarget);
            }

            return expr;
        }

        /// <summary>
        /// Parses logical OR expressions.
        /// </summary>
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

        /// <summary>
        /// Parses logical AND expressions.
        /// </summary>
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

        /// <summary>
        /// Parses equality expressions.
        /// </summary>
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

        /// <summary>
        /// Parses comparison expressions.
        /// </summary>
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

        /// <summary>
        /// Parses term expressions.
        /// </summary>
        private Expression Term()
        {
            Expression expr = Factor();
            while (Match(TokenType.DUGANG, TokenType.KUHA, TokenType.SUMPAY))
            {
                if (Previous().Type == TokenType.SUMPAY && !isInsideDisplay)
                {
                    RaiseError(Peek().Line, Generic, "Cannot perform concatenation '&' outside IPAKITA statement.");
                }
                Token op = Previous();
                Expression right = Factor();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }
            return expr;
        }

        /// <summary>
        /// Parses factor expressions.
        /// </summary>
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

        /// <summary>
        /// Parses unary expressions.
        /// </summary>
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

        /// <summary>
        /// Parses primary expressions such as literals, variables, or grouped expressions.
        /// </summary>
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
                Consume(TokenType.SIRAKUTOB, ExpectedToken, ")", "after expression");
                return new GroupingExpression(expr, Previous().Line);
            }

            if (Match(TokenType.UNKNOWN))
            {
                RaiseError(Peek().Line, Generic, $"Unknown character '{Previous().Value}'");
            }

            RaiseError(Peek().Line, Generic, "Invalid/empty expression.");
            return null;
        }

        #endregion Expression Parsing

        #region Helper Methods

        /// <summary>
        /// Validates the program structure by ensuring that both the SUGOD and KATAPUSAN tokens exist exactly once,
        /// and that there are no invalid tokens outside the defined program boundaries.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when either token is missing, duplicated, or when unexpected tokens are found outside the valid range.
        /// </exception>
        private void ValidateProgramStructure()
        {
            int sugodIndex = tokens.FindIndex(token => token.Type == TokenType.SUGOD);
            int katapusanIndex = tokens.FindLastIndex(token => token.Type == TokenType.KATAPUSAN);

            ConsumeNewlines();

            if (sugodIndex == -1 || katapusanIndex == -1)
            {
                string missingToken = sugodIndex == -1 ? "SUGOD" : "KATAPUSAN";
                RaiseError(Peek().Line, Generic, $"{missingToken} must exist for the program to run.");
            }

            if (tokens.Count(token => token.Type == TokenType.SUGOD) > 1 || tokens.Count(token => token.Type == TokenType.KATAPUSAN) > 1)
            {
                int duplicateSugodIndex = tokens.FindIndex(sugodIndex + 1, token => token.Type == TokenType.SUGOD);
                int duplicateKatapusanIndex = tokens.FindIndex(token => token.Type == TokenType.KATAPUSAN);
                int errorLine = duplicateSugodIndex > -1 ? tokens[duplicateSugodIndex].Line : tokens[duplicateKatapusanIndex].Line;
                string duplicateToken = duplicateSugodIndex > -1 ? "SUGOD" : "KATAPUSAN";
                RaiseError(errorLine, Generic, $"Only one {duplicateToken} should exist.");
            }

            if (sugodIndex > 0 || katapusanIndex < tokens.Count - 1)
            {
                if (sugodIndex > 0)
                {
                    var outOfBoundsToken = tokens.Take(sugodIndex).FirstOrDefault(token => token.Type != TokenType.STORYA && token.Type != TokenType.SUNODLINYA);
                    if (outOfBoundsToken != null)
                    {
                        RaiseError(outOfBoundsToken.Line, Generic, $"Invalid code or tokens outside SUGOD.");
                    }
                }
                if (katapusanIndex < tokens.Count - 1)
                {
                    var outOfBoundsToken = tokens.Skip(katapusanIndex + 1).FirstOrDefault(token => token.Type != TokenType.STORYA && token.Type != TokenType.SUNODLINYA && token.Type != TokenType.EOF);
                    if (outOfBoundsToken != null)
                    {
                        RaiseError(outOfBoundsToken.Line, Generic, $"Invalid code or tokens after KATAPUSAN.");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the declared type of a variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns>The <see cref="TokenType"/> associated with the variable.</returns>
        /// <exception cref="ArgumentException">Thrown if the variable is not declared.</exception>
        private TokenType GetVariableType(string variableName)
        {
            if (declaredVariables.TryGetValue(variableName, out TokenType type))
            {
                return type;
            }
            RaiseError(Peek().Line, UndeclaredVariable, variableName);
            return default; // Unreachable
        }

        /// <summary>
        /// Determines whether the parser has reached the end of the token list.
        /// </summary>
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        /// <summary>
        /// Returns the current token without advancing.
        /// </summary>
        private Token Peek() => tokens[current];

        /// <summary>
        /// Returns the previous token.
        /// </summary>
        private Token Previous() => tokens[current - 1];

        /// <summary>
        /// Advances the token pointer and returns the previous token.
        /// </summary>
        private Token Advance()
        {
            if (!IsAtEnd())
                current++;
            return Previous();
        }

        /// <summary>
        /// Checks if the current token matches the specified type.
        /// </summary>
        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        /// <summary>
        /// If the current token matches any of the specified types, advances the token pointer.
        /// </summary>
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

        /// <summary>
        /// Consumes the current token if it matches the expected type; otherwise, throws an error.
        /// </summary>
        private Token Consume(TokenType type, ErrorCode errorCode, string expected, string additionalInfo = null)
        {
            if (Check(type))
                return Advance();
            RaiseError(Peek().Line, errorCode, expected, additionalInfo);
            return null;
        }

        /// <summary>
        /// Consumes all consecutive newline tokens.
        /// </summary>
        private void ConsumeNewlines()
        {
            while (Match(TokenType.SUNODLINYA)) { }
        }

        /// <summary>
        /// Checks if a given name is a reserved keyword.
        /// </summary>
        private static bool IsReservedKeyword(string name) => Lexer.keywords.ContainsKey(name);

        /// <summary>
        /// Safely returns a previously parsed statement by index.
        /// </summary>
        private Statement Statement(int index)
        {
            try
            {
                return statements[index];
            }
            catch (Exception ex)
            {
                RaiseError(Peek().Line, Generic, ex.Message);
                return null;
            }
        }

        #endregion Helper Methods
    }
}
