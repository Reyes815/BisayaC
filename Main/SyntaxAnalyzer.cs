using static BisayaC.ErrorStatements;
using static BisayaC.ErrorStatements.ErrorType;
using LexicalAnalyzer;
namespace BisayaC
{
    public class SyntaxAnalyzer
    {
        private readonly List<Token> tokens;
        private readonly List<Statement> statements;
        private readonly Dictionary<string, TokenType> declaredVariables;

        private int pos;
        private bool isDisplay;
        private bool isConditional;
        private bool isIf;

        public SyntaxAnalyzer(List<Token> tokens)
        {
            this.tokens = tokens;
            this.statements = new List<Statement>();
            this.declaredVariables = new Dictionary<string, TokenType>();
            this.pos = 0;
            this.isDisplay = false;
            this.isConditional = false;
            this.isIf = false;
        }

        #region Parsing Methods

        public ProgramNode Parse()
        {
            try
            {
                ValidateStructure();
                ParseProgramStart();
                ParseStatements();
                ParseProgramEnd();

                return new ProgramNode(statements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
                return null; // Required for compilation, though this line will never be reached
            }
        }

        private void ParseProgramStart()
        {
            ReadToken(TokenType.SUGOD, MissingToken, "SUGOD", "at the start of the program");
            SkipEmptyLines();
        }

        private void ParseStatements()
        {
            while (!IsAtEnd() && !Check(TokenType.KATAPUSAN))
            {
                Statement stmt = StartParsing();
                if (stmt != null)
                {
                    SkipEmptyLines();
                    statements.Add(stmt);
                    SkipEmptyLines();
                }
            }
        }

        private void ParseProgramEnd()
        {
            ReadToken(TokenType.KATAPUSAN, MissingToken, "KATAPUSAN", "at the end of the program");
        }


        #endregion Public Parsing Methods

        #region Statement Parsing

        private Statement StartParsing()
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
            // Ensure a valid type is present after 'MUGNA'
            if (!Match(TokenType.NUMERO, TokenType.PULONG, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK))
            {
                ThrowError(Peek().Line, MissingToken, "a data type after 'MUGNA'");
            }

            TokenType varType = Previous().Type;
            var variables = new List<Variable>();

            // Parse one or more variable declarations separated by commas
            do
            {
                // Variable name must not be a reserved keyword
                if (IsReservedKeyword(Peek().Value))
                {
                    ThrowError(Peek().Line, KeywordIsReserved, Peek().Value);
                }

                // Get the variable name
                string name = ReadToken(TokenType.IDENTIFIER, MissingToken, "a variable name", $"got '{Peek().Value}' instead").Value;

                // Check for duplicate declarations
                if (declaredVariables.ContainsKey(name))
                {
                    ThrowError(Peek().Line, KeywordIsReserved, $"Variable '{name}' is already declared.");
                }

                // Optional initializer (e.g. MUGNA NUMERO x = 5)
                Expression initializer = null;
                if (Match(TokenType.ASAYNMENT))
                {
                    initializer = ParseExpression();
                }

                // Validate boolean values if type is TINUOD
                if (varType == TokenType.TINUOD && initializer is LiteralExpression lit && lit.Value is string boolStr)
                {
                    if (boolStr != "OO" && boolStr != "DILI")
                    {
                        ThrowError(Peek().Line, General, $"Boolean values must be 'OO' or 'DILI'. Found: {boolStr}");
                    }
                }

                // Save the variable
                variables.Add(new Variable(name, Previous().Line, initializer));
                declaredVariables.Add(name, varType);

            } while (Match(TokenType.KAMA)); // Loop if there's a comma

            // Handle common mistakes after declaration
            if (Peek().Type == TokenType.IDENTIFIER)
            {
                ThrowError(Peek().Line, MissingToken, ",", "for multiple declarations on one line");
            }

            if (Match(TokenType.NUMERO, TokenType.LETRA, TokenType.TINUOD, TokenType.TIPIK, TokenType.PULONG))
            {
                ThrowError(Peek().Line, General, $"Unexpected type: '{Previous().Value}'");
            }

            if (Match(TokenType.MUGNA))
            {
                ThrowError(Peek().Line, General, "Unexpected 'MUGNA' found again");
            }

            return new DeclarationStatement(varType, variables, Previous().Line);
        }


        private Statement ParseStatement()
        {
            if (Match(TokenType.KUNG)) return ParseIfStatement();
            if (Match(TokenType.ALANG)) return ParseForStatement();
            if (Match(TokenType.SAMTANG)) return ParseWhileStatement();
            if (Match(TokenType.IPAKITA)) return ParseOutputStatement();
            if (Match(TokenType.DAWAT)) return ParseInputStatement();

            // Assignment or increment check
            if (Check(TokenType.IDENTIFIER) || Check(TokenType.KAMA))
            {
                // Handle chained assignment error
                if (statements.Count > 0 &&
                    Statement(statements.Count - 1) is AssignmentStatement &&
                    Previous().Type != TokenType.SUNODLINYA)
                {
                    ReadToken(TokenType.KAMA, MissingToken, ",", "after an assignment");
                }

                // Check if variable is declared
                if (!declaredVariables.ContainsKey(Peek().Value) && Check(TokenType.IDENTIFIER))
                {
                    ThrowError(Peek().Line, VariableNotDeclared, Peek().Value);
                }

                // Check for increment
                if (Match(TokenType.IDENTIFIER))
                {
                    Token id = Previous();
                    if (Match(TokenType.INCREMENT))
                    {
                        return new IncrementStatement(new Variable(id.Value, id.Line), id.Line);
                    }
                    pos--; // rollback if not increment
                }

                return ParseAssignmentStatement();
            }

            if (Check(TokenType.UNKNOWN))
            {
                ThrowError(Peek().Line, General, $"Unknown character '{Peek().Value}'");
            }

            if (isIf)
                return new EmptyStatement(Peek().Line);

            ThrowError(Peek().Line, General, $"Invalid statement. Cause: '{Peek().Value}'");
            return null;
        }

        private AssignmentStatement ParseAssignmentStatement()
        {
            Token name = ReadToken(TokenType.IDENTIFIER, MissingToken, "variable name");

            if (IsReservedKeyword(name.Value))
            {
                ThrowError(name.Line, KeywordIsReserved, name.Value, "cannot be used as a variable name");
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

        private Statement ParseOutputStatement()
        {
            isDisplay = true;
            ReadToken(TokenType.DUHATULDOK, MissingToken, ":", "after 'IPAKITA'");

            var expressions = new List<Expression>();

            if ((Check(TokenType.SUNODLINYA) && Peek().Value != "$") || Check(TokenType.KATAPUSAN))
            {
                ThrowError(Peek().Line, General, "Nothing to display.");
            }

            while (!Check(TokenType.KATAPUSAN) && !IsAtEnd() && !Peek().Value.Contains("\\n"))
            {
                if (Check(TokenType.SUNODLINYA))
                {
                    Advance();
                    expressions.Add(new LiteralExpression("\n", Previous().Line));
                    continue;
                }

                expressions.Add(ParseExpression());
            }

            if (Previous().Type == TokenType.IDENTIFIER && !declaredVariables.ContainsKey(Previous().Value))
            {
                ThrowError(Previous().Line, VariableNotDeclared, Previous().Value);
            }

            isDisplay = false;
            return new OutputStatement(expressions, Previous().Line);
        }

        private Statement ParseInputStatement()
        {
            ReadToken(TokenType.DUHATULDOK, MissingToken, ":", "after 'DAWAT'");

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

                string varName = ReadToken(TokenType.IDENTIFIER, MissingToken, "input variable name").Value;
                variables.Add(new Variable(varName, Previous().Line));

            } while (Match(TokenType.KAMA));

            if (Check(TokenType.IDENTIFIER))
            {
                ThrowError(Peek().Line, MissingToken, "comma", $"between variables, got '{Peek().Value}' instead.");
            }

            return new InputStatement(variables, Peek().Line);
        }

        private Statement ParseIfStatement()
        {
            ReadToken(TokenType.ABLIKUTOB, MissingToken, "(", "after 'KUNG'");
            isConditional = true;
            Expression condition = ParseExpression();
            isConditional = false;
            ReadToken(TokenType.SIRAKUTOB, MissingToken, ")", "after condition");
            Advance(); // move past ')'

            ReadToken(TokenType.PUNDOK, MissingToken, "PUNDOK", "to begin 'KUNG' block");
            isIf = true;
            List<Statement> thenBranch = ParseBlock();
            List<Statement> elseBranch = null;

            SkipEmptyLines();

            // Handle 'KUNG DILI' or 'KUNG WALA'
            if (Match(TokenType.KUNG))
            {
                SkipEmptyLines();

                if (Match(TokenType.DILI))
                {
                    elseBranch = new List<Statement> { ParseIfStatement() };
                }
                else if (Match(TokenType.WALA))
                {
                    SkipEmptyLines();
                    ReadToken(TokenType.PUNDOK, MissingToken, "PUNDOK", "after 'WALA'");
                    elseBranch = ParseBlock();
                }
                else
                {
                    // Unexpected token after KUNG, rollback
                    tokens.Add(Previous());
                    pos--;
                }
            }

            isIf = false;
            return new IfStatement(condition, thenBranch, elseBranch, Previous().Line);
        }

        private Statement ParseForStatement()
        {
            ReadToken(TokenType.SA, MissingToken, "SA", "after 'ALANG'");
            ReadToken(TokenType.ABLIKUTOB, MissingToken, "(", "after 'SA' in loop header");

            AssignmentStatement initialization = ParseAssignmentStatement();
            ReadToken(TokenType.KAMA, MissingToken, ",", "after initialization");

            Expression condition = ParseExpression();
            ReadToken(TokenType.KAMA, MissingToken, ",", "after condition");

            Expression update = ParseExpression();
            ReadToken(TokenType.SIRAKUTOB, MissingToken, ")", "after update expression");

            SkipEmptyLines();
            ReadToken(TokenType.PUNDOK, MissingToken, "PUNDOK", "to begin loop body");
            List<Statement> body = ParseBlock();

            return new ForLoopStatement(initialization, condition, update, body, Previous().Line);
        }

        private Statement ParseWhileStatement()
        {
            ReadToken(TokenType.ABLIKUTOB, MissingToken, "(", "after 'SAMTANG'");
            isConditional = true;
            Expression condition = ParseExpression();
            isConditional = false;
            ReadToken(TokenType.SIRAKUTOB, MissingToken, ")", "after condition");
            Advance(); // skip past ')'

            ReadToken(TokenType.PUNDOK, MissingToken, "PUNDOK", "to begin while loop body");
            List<Statement> body = ParseBlock();

            return new WhileStatement(condition, body, Previous().Line);
        }

        private List<Statement> ParseBlock()
        {
            var statements = new List<Statement>();
            ReadToken(TokenType.SUGODKUNG, VariableNotDeclared, "{", "to start block");
            SkipEmptyLines();

            while (!Check(TokenType.HUMANKUNG) && !Check(TokenType.KATAPUSAN) && !IsAtEnd())
            {
                Statement stmt = StartParsing();
                if (stmt != null)
                {
                    SkipEmptyLines();
                    statements.Add(stmt);
                    SkipEmptyLines();
                }
            }

            ReadToken(TokenType.HUMANKUNG, VariableNotDeclared, "}", "to end block");
            SkipEmptyLines();

            return statements;
        }

        #endregion Statement Parsing

        #region Expression Parsing

        public Expression ParseExpression() => ParseAssignment();

        private Expression ParseAssignment()
        {
            var expr = ParseLogicOr();

            if (Match(TokenType.ASAYNMENT))
            {
                if (isConditional)
                    ThrowError(Peek().Line, General, "Cannot use '=' in a conditional statement.");

                Token equals = Previous();
                Expression value = ParseAssignment();

                if (expr is VariableExpression varExpr)
                {
                    return new AssignmentExpression(new Variable(varExpr.Name, varExpr.LineNumber), equals, value, varExpr.LineNumber);
                }

                ThrowError(equals.Line, AssignmentTargetInvalid);
            }

            return expr;
        }

        private Expression ParseLogicOr()
        {
            var expr = ParseLogicAnd();

            while (Match(TokenType.O))
            {
                Token op = Previous();
                Expression right = ParseLogicAnd();
                expr = new LogicalExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseLogicAnd()
        {
            var expr = ParseEquality();

            while (Match(TokenType.UG))
            {
                Token op = Previous();
                Expression right = ParseEquality();
                expr = new LogicalExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseEquality()
        {
            var expr = ParseComparison();

            while (Match(TokenType.PAREHAS, TokenType.LAHI))
            {
                Token op = Previous();
                Expression right = ParseComparison();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseComparison()
        {
            var expr = ParseTerm();

            while (Match(TokenType.LABAW, TokenType.UBOS, TokenType.LABAWSA, TokenType.UBOSSA))
            {
                Token op = Previous();
                Expression right = ParseTerm();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseTerm()
        {
            var expr = ParseFactor();

            while (Match(TokenType.DUGANG, TokenType.KUHA, TokenType.SUMPAY))
            {
                Token op = Previous();

                if (op.Type == TokenType.SUMPAY && !isDisplay)
                    ThrowError(op.Line, General, "'&' (concatenation) is only allowed in IPAKITA statements.");

                Expression right = ParseFactor();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseFactor()
        {
            var expr = ParseUnary();

            while (Match(TokenType.PADAGHAN, TokenType.BAHIN, TokenType.SOBRA))
            {
                Token op = Previous();
                Expression right = ParseUnary();
                expr = new BinaryExpression(expr, op, right, op.Line);
            }

            return expr;
        }

        private Expression ParseUnary()
        {
            // Treat "DILI" as a literal boolean if it's a standalone value
            if (Check(TokenType.DILI) && Peek().Value == "DILI")
                return ParsePrimary();

            if (Match(TokenType.KUHA, TokenType.DILI))
            {
                Token op = Previous();
                Expression right = ParseUnary();
                return new UnaryExpression(op, right, op.Line);
            }

            return ParsePrimary();
        }

        private Expression ParsePrimary()
        {
            if (Match(TokenType.OO, TokenType.DILI))
                return new LiteralExpression(Previous().Type == TokenType.OO, Previous().Line);

            if (Match(TokenType.INTEGERLITERAL))
                return new LiteralExpression(int.Parse(Previous().Value), Previous().Line);

            if (Match(TokenType.FLOATLITERAL))
                return new LiteralExpression(float.Parse(Previous().Value), Previous().Line);

            if (Match(TokenType.CHARACTERLITERAL))
                return new LiteralExpression(Previous().Value[0], Previous().Line);

            if (Match(TokenType.STRINGLITERAL))
                return new LiteralExpression(Previous().Value, Previous().Line);

            if (Match(TokenType.IDENTIFIER))
            {
                var variable = new VariableExpression(Previous().Value, Previous().Line);

                if (Match(TokenType.INCREMENT))
                {
                    Token op = Previous();
                    return new UnaryExpression(op, variable, op.Line);
                }

                return variable;
            }

            if (Match(TokenType.ABLIKUTOB))
            {
                Expression expr = ParseExpression();
                ReadToken(TokenType.SIRAKUTOB, MissingToken, ")", "after expression");
                return new GroupingExpression(expr, Previous().Line);
            }

            if (Match(TokenType.UNKNOWN))
                ThrowError(Peek().Line, General, $"Unknown character '{Previous().Value}'");

            ThrowError(Peek().Line, General, "Invalid or empty expression.");
            return null;
        }

        #endregion

        #region Helper Methods
        private void ValidateStructure()
        {
            SkipEmptyLines();

            int sugodIndex = FindFirstToken(TokenType.SUGOD);
            int katapusanIndex = FindLastToken(TokenType.KATAPUSAN);

            ValidatePresenceOfProgramBounds(sugodIndex, katapusanIndex);
            ValidateSingleProgramBounds();
            ValidateNoCodeOutsideBounds(sugodIndex, katapusanIndex);
        }

        private int FindFirstToken(TokenType type) =>
            tokens.FindIndex(token => token.Type == type);

        private int FindLastToken(TokenType type) =>
            tokens.FindLastIndex(token => token.Type == type);

        private void ValidatePresenceOfProgramBounds(int sugodIndex, int katapusanIndex)
        {
            if (sugodIndex == -1 || katapusanIndex == -1)
            {
                string missingToken = sugodIndex == -1 ? "SUGOD" : "KATAPUSAN";
                ThrowError(Peek().Line, General, $"{missingToken} must exist for the program to run.");
            }
        }

        private void ValidateSingleProgramBounds()
        {
            int sugodCount = tokens.Count(t => t.Type == TokenType.SUGOD);
            int katapusanCount = tokens.Count(t => t.Type == TokenType.KATAPUSAN);

            if (sugodCount > 1 || katapusanCount > 1)
            {
                int duplicateIndex = sugodCount > 1
                    ? tokens.FindIndex(1, t => t.Type == TokenType.SUGOD)
                    : tokens.FindIndex(1, t => t.Type == TokenType.KATAPUSAN);

                string duplicateToken = sugodCount > 1 ? "SUGOD" : "KATAPUSAN";
                int errorLine = tokens[duplicateIndex].Line;

                ThrowError(errorLine, General, $"Only one {duplicateToken} should exist.");
            }
        }

        private void ValidateNoCodeOutsideBounds(int sugodIndex, int katapusanIndex)
        {
            // Before SUGOD
            if (sugodIndex > 0)
            {
                var invalidToken = tokens.Take(sugodIndex)
                    .FirstOrDefault(t => t.Type != TokenType.STORYA && t.Type != TokenType.SUNODLINYA);

                if (invalidToken != null)
                {
                    ThrowError(invalidToken.Line, General, $"Invalid code or tokens outside SUGOD.");
                }
            }

            // After KATAPUSAN
            if (katapusanIndex < tokens.Count - 1)
            {
                var invalidToken = tokens.Skip(katapusanIndex + 1)
                    .FirstOrDefault(t => t.Type != TokenType.STORYA && t.Type != TokenType.SUNODLINYA && t.Type != TokenType.EOF);

                if (invalidToken != null)
                {
                    ThrowError(invalidToken.Line, General, $"Invalid code or tokens after KATAPUSAN.");
                }
            }
        }


        // Retrieves the declared type of a variable or throws an error if not found.
        private TokenType GetVariableType(string variableName)
        {
            if (declaredVariables.TryGetValue(variableName, out TokenType type))
            {
                return type;
            }

            ThrowError(Peek().Line, VariableNotDeclared, variableName);
            return default; // Unreachable due to ThrowError, but required syntactically
        }

        // Checks if we've reached the end of the token stream.
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        // Returns the current token without consuming it.
        private Token Peek() => tokens[pos];

        // Returns the previously consumed token.
        private Token Previous() => tokens[pos - 1];

        // Consumes the current token and moves to the next one.
        private Token Advance()
        {
            if (!IsAtEnd())
                pos++;
            return Previous();
        }

        // Checks if the current token matches the expected type without consuming it.
        private bool Check(TokenType type) =>
            !IsAtEnd() && Peek().Type == type;

        // Tries to match the current token with any of the given types.
        // If a match is found, the token is consumed and true is returned.
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        // Consumes a token of the expected type or throws an error if it doesn't match.
        private Token ReadToken(TokenType expectedType, ErrorType errorCode, string expectedDescription, string additionalInfo = null)
        {
            if (Check(expectedType))
                return Advance();

            ThrowError(Peek().Line, errorCode, expectedDescription, additionalInfo);
            return null; // Required for compiler; unreachable due to ThrowError.
        }

        // Consumes and discards all newline tokens (SUNODLINYA) in sequence.
        private void SkipEmptyLines()
        {
            while (Match(TokenType.SUNODLINYA)) { }
        }

        // Checks if a name is a reserved keyword.
        private static bool IsReservedKeyword(string name) =>
            LexerAnalyzer.keywords.ContainsKey(name);

        // Safely retrieves a statement by index; throws a parser-level error if invalid.
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
