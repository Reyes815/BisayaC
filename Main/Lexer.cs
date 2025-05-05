using static BisayaC.ErrorHandler;
using static BisayaC.ErrorHandler.ErrorCode;

namespace BisayaC
{
    /// <summary>
    /// Enumerates the types of tokens that can be produced.
    /// </summary>
    public enum TokenType
    {
        // Code Blocks
        SUGOD,              // Start of program
        KATAPUSAN,          // End of program
        SUGODKUNG,          // '{' - Start block
        HUMANKUNG,          // '}' - End block
        PUNDOK,             // Group a block of codes

        // Data Types
        NUMERO,             // Integer (no decimal)
        LETRA,              // Character
        TINUOD,             // Boolean
        TIPIK,              // Floating point
        PULONG,             // String

        // Literals
        IDENTIFIER,         // Variable names etc.
        INTEGERLITERAL,     // e.g. 5
        STRINGLITERAL,      // e.g. "Hello, World!"
        CHARACTERLITERAL,   // e.g. 'n'
        OO,                 // Boolean true literal
        DILI,               // Boolean false literal & "NOT" operator
        FLOATLITERAL,       // e.g. 3.14

        // Operators
        ASAYNMENT,          // '='
        DUGANG,             // '+'
        KUHA,               // '-'
        PADAGHAN,           // '*'
        BAHIN,              // '/'
        SOBRA,              // '%'
        LABAW,              // '>'
        UBOS,               // '<'
        LABAWSA,            // '>='
        UBOSSA,             // '<='
        PAREHAS,            // '=='
        LAHI,               // '<>'
        UG,                 // Logical AND
        O,                  // Logical OR
        SUMPAY,             // '&' (concatenation)

        // Delimiters
        SUNODLINYA,         // Newline or '$'
        STORYA,             // Comment delimiter
        DUHATULDOK,         // ':'
        KAMA,               // ','
        ABLIKUTOB,          // '('
        SIRAKUTOB,          // ')'
        ABLIPAHID,          // '[' - Start escape sequence
        SIRAPAHID,          // ']' - End escape sequence

        // Keywords
        MUGNA,              // Declaration
        KUNG,               // If
        WALA,               // Else (if not)
        ALANG,              // For-loop keyword
        SA,                 // For-loop part 2
        SAMTANG,            // While
        IPAKITA,            // Display/output
        DAWAT,              // Input/scan

        // Special and Additional
        UNKNOWN,            // Unrecognized token
        EOF,                // End of File
        PI,                 // 3.14159
        INCREMENT,          // ++
        MODASSIGNMENT,      // %=
        ADDASSIGNMENT,      // +=
        SUBASSIGNMENT,      // -=
        MULASSIGNMENT,      // *=
        DIVASSIGNMENT       // /=
    }

    /// <summary>
    /// Represents a token produced during lexical analysis.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets the token type.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Gets the token's string value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the line number where the token appears.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="value">The token value.</param>
        /// <param name="line">The line number.</param>
        public Token(TokenType type, string value, int line)
        {
            Type = type;
            Value = value;
            Line = line;
        }
    }

    /// <summary>
    /// Provides methods to tokenize Bisaya++ source code.
    /// </summary>
    public static class Lexer
    {
        private static int _index = 0;
        private static int _line = 1;
        private static string? _code;

        /// <summary>
        /// A dictionary of reserved keywords and their corresponding token types.
        /// </summary>
        public static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            {"SUGOD", TokenType.SUGOD},
            {"KATAPUSAN", TokenType.KATAPUSAN},
            {"PUNDOK", TokenType.PUNDOK},
            {"MUGNA", TokenType.MUGNA},
            {"NUMERO", TokenType.NUMERO},
            {"TIPIK", TokenType.TIPIK},
            {"LETRA", TokenType.LETRA},
            {"PULONG", TokenType.PULONG},
            {"TINUOD", TokenType.TINUOD},
            {"IPAKITA", TokenType.IPAKITA},
            {"DAWAT", TokenType.DAWAT},
            {"KUNG", TokenType.KUNG},
            {"WALA", TokenType.WALA},
            {"DILI", TokenType.DILI},
            {"ALANG", TokenType.ALANG},
            {"SA", TokenType.SA},
            {"SAMTANG", TokenType.SAMTANG},
            {"UG", TokenType.UG},
            {"O", TokenType.O},
            {"OO", TokenType.OO},
        };

        /// <summary>
        /// Gets the current source code ensuring it is not null.
        /// </summary>
        private static string Code => _code ?? throw new InvalidOperationException("_code is null");

        /// <summary>
        /// Gets the current character in the code.
        /// </summary>
        private static char CurrentChar => Code[_index];

        /// <summary>
        /// Tokenizes the provided source code.
        /// </summary>
        /// <param name="code">The Bisaya++ source code.</param>
        /// <returns>A list of tokens representing the source code.</returns>
        public static List<Token> Tokenize(string code)
        {
            _code = code;
            _line = 1;
            _index = 0;
            var tokens = new List<Token>();

            try
            {
                while (_index < Code.Length)
                {
                    char currentChar = CurrentChar;
                    switch (currentChar)
                    {
                        #region SPECIAL
                        case '\n':
                            tokens.Add(new Token(TokenType.SUNODLINYA, "\\n", _line));
                            _index++;
                            _line++;
                            break;
                        case ' ':
                        case '\t':
                        case '\r':
                            _index++;
                            break;
                        #endregion SPECIAL

                        #region OPERATORS
                        case '=':
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '%':
                        case '>':
                        case '<':
                            tokens.Add(HandleOperator(currentChar));
                            _index += AddIndex(currentChar);
                            break;
                        case '&':
                            if (ScanNextAndPrev())
                            {
                                _index++;
                            }
                            else
                            {
                                tokens.Add(HandleOperator(currentChar));
                                _index++;
                            }
                            break;
                        #endregion OPERATORS

                        #region DELIMITERS
                        case '$':
                        case ':':
                        case ',':
                        case '(':
                        case ')':
                        case '{':
                        case '}':
                            tokens.Add(HandleDelimiter(currentChar));
                            _index++;
                            break;
                        case '[':
                            tokens.Add(ScanEscape());
                            break;
                        #endregion DELIMITERS

                        case '"':
                            tokens.Add(ScanString());
                            break;
                        case '\'':
                            tokens.Add(ScanCharacter());
                            break;
                        default:
                            if (char.IsLetter(currentChar) || currentChar == '_')
                            {
                                tokens.Add(ScanIdentifier());
                            }
                            else if (char.IsDigit(currentChar))
                            {
                                tokens.Add(FloatOrInteger());
                            }
                            else
                            {
                                tokens.Add(new Token(TokenType.UNKNOWN, currentChar.ToString(), _line));
                                _index++;
                            }
                            break;
                    }
                }
                tokens.Add(new Token(TokenType.EOF, "END OF LINE", _line));
                return tokens;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
                return new List<Token>();
            }
        }

        #region HELPER METHODS

        /// <summary>
        /// Handles comment tokens by skipping characters until the end of the line.
        /// </summary>
        /// <returns>A newline token representing the end of the comment.</returns>
        private static Token HandleComment()
        {
            SkipComment();
            _line++;
            return new Token(TokenType.SUNODLINYA, "\\n", _line - 1);
        }

        /// <summary>
        /// Handles operator tokens based on the current character.
        /// </summary>
        /// <param name="currentChar">The current operator character.</param>
        /// <returns>The token corresponding to the operator.</returns>
        private static Token HandleOperator(char currentChar)
        {
            switch (currentChar)
            {
                case '=':
                    return PeekChar(1) == '=' ? new Token(TokenType.PAREHAS, "==", _line)
                                              : new Token(TokenType.ASAYNMENT, "=", _line);
                case '+':
                    return PeekChar(1) == '+' ? new Token(TokenType.INCREMENT, "++", _line)
                                              : new Token(TokenType.DUGANG, "+", _line);
                case '-':
                    return PeekChar(1) == '-' ? HandleComment()
                                              : new Token(TokenType.KUHA, "-", _line);
                case '*':
                    return new Token(TokenType.PADAGHAN, "*", _line);
                case '/':
                    return new Token(TokenType.BAHIN, "/", _line);
                case '%':
                    return new Token(TokenType.SOBRA, "%", _line);
                case '>':
                    return PeekChar(1) == '=' ? new Token(TokenType.LABAWSA, ">=", _line)
                                              : new Token(TokenType.LABAW, ">", _line);
                case '<':
                    if (PeekChar(1) == '=')
                        return new Token(TokenType.UBOSSA, "<=", _line);
                    if (PeekChar(1) == '>')
                        return new Token(TokenType.LAHI, "<>", _line);
                    return new Token(TokenType.UBOS, "<", _line);
                case '&':
                    return new Token(TokenType.SUMPAY, "&", _line);
                default:
                    return new Token(TokenType.UNKNOWN, "Unknown Operator", _line);
            }
        }

        /// <summary>
        /// Handles delimiter tokens based on the current character.
        /// </summary>
        /// <param name="currentChar">The current delimiter character.</param>
        /// <returns>The token corresponding to the delimiter.</returns>
        private static Token HandleDelimiter(char currentChar)
        {
            switch (currentChar)
            {
                case '$':
                    return new Token(TokenType.SUNODLINYA, "$", _line);
                case ':':
                    return new Token(TokenType.DUHATULDOK, ":", _line);
                case ',':
                    return new Token(TokenType.KAMA, ",", _line);
                case '(':
                    return new Token(TokenType.ABLIKUTOB, "(", _line);
                case ')':
                    return new Token(TokenType.SIRAKUTOB, ")", _line);
                case '{':
                    return new Token(TokenType.SUGODKUNG, "{", _line);
                case '}':
                    return new Token(TokenType.HUMANKUNG, "}", _line);
                default:
                    return new Token(TokenType.UNKNOWN, "Unknown Delimiter", _line);
            }
        }

        /// <summary>
        /// Scans a number and returns a token representing either an integer or a floating-point literal.
        /// </summary>
        private static Token FloatOrInteger()
        {
            string number = "";
            bool isFloat = false;

            while (_index < Code.Length && (char.IsDigit(CurrentChar) || CurrentChar == '.'))
            {
                if (CurrentChar == '.')
                {
                    if (isFloat)
                        break;
                    isFloat = true;
                    number += '.';
                }
                else
                {
                    number += CurrentChar;
                }
                _index++;
            }

            return isFloat
                ? new Token(TokenType.FLOATLITERAL, number, _line)
                : new Token(TokenType.INTEGERLITERAL, number, _line);
        }

        /// <summary>
        /// Skips characters until the end of the current comment.
        /// </summary>
        private static void SkipComment()
        {
            while (_index < Code.Length && CurrentChar != '\n')
            {
                _index++;
            }
            if (_index < Code.Length && CurrentChar == '\n')
            {
                _index++;
            }
        }

        /// <summary>
        /// Scans an escape sequence enclosed in square brackets and returns it as a string literal token.
        /// </summary>
        private static Token ScanEscape()
        {
            int start = _index + 1;
            int lastClosedBracket = -1;
            _index++; // Skip the opening '['

            while (_index < Code.Length)
            {
                if (CurrentChar == '[')
                {
                    if (lastClosedBracket != -1)
                    {
                        _index = lastClosedBracket + 1;
                        break;
                    }
                }
                else if (CurrentChar == ']')
                {
                    lastClosedBracket = _index;
                }
                _index++;
                if (_index == Code.Length && lastClosedBracket != -1)
                {
                    _index = lastClosedBracket + 1;
                    break;
                }
            }

            if (lastClosedBracket == -1)
            {
                RaiseError(_line, Generic, "Unterminated escape sequence");
            }

            string content = Code[start..lastClosedBracket];
            return new Token(TokenType.STRINGLITERAL, content, _line);
        }

        /// <summary>
        /// Scans a character literal and returns the corresponding token.
        /// </summary>
        private static Token ScanCharacter()
        {
            if (Code[_index + 2] != '\'')
            {
                if (Code[_index + 1] == '\'')
                {
                    RaiseError(_line, Generic, "Empty character literal.");
                }
                RaiseError(_line, Generic, "Invalid or unterminated character literal.");
            }
            _index++; // Skip the opening quote
            string character = Code.Substring(_index, 1);
            _index += 2; // Skip the character and the closing quote
            return new Token(TokenType.CHARACTERLITERAL, character, _line);
        }

        /// <summary>
        /// Scans a string literal and returns the corresponding token.
        /// </summary>
        private static Token ScanString()
        {
            int start = _index;
            _index++; // Skip the opening quote
            while (_index < Code.Length && CurrentChar != '"')
            {
                _index++;
            }
            if (_index == Code.Length)
            {
                RaiseError(_line, Generic, "Unterminated string literal.");
            }
            _index++; // Skip the closing quote
            string str = Code[(start + 1)..(_index - 1)];
            // If the string contains boolean indicators, return the corresponding token.
            if (str.Contains("OO") || str.Contains("DILI"))
            {
                return str.Contains("OO")
                    ? new Token(TokenType.OO, str, _line)
                    : new Token(TokenType.DILI, str, _line);
            }
            return new Token(TokenType.STRINGLITERAL, str, _line);
        }

        /// <summary>
        /// Scans an identifier and returns the corresponding token.
        /// </summary>
        private static Token ScanIdentifier()
        {
            int start = _index;
            while (_index < Code.Length && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
            {
                _index++;
            }
            string value = Code[start.._index];
            if (keywords.TryGetValue(value, out TokenType type) && (value != "OO" && value != "DILI"))
            {
                return new Token(type, value, _line);
            }
            if (value == "DILI")
            {
                // Return a token with value "NOT" for the reserved keyword "DILI"
                return new Token(TokenType.DILI, "NOT", _line);
            }
            return new Token(TokenType.IDENTIFIER, value, _line);
        }

        /// <summary>
        /// Checks adjacent non-whitespace characters to determine a special case.
        /// </summary>
        /// <returns>True if a '$' is found before or after the current index; otherwise, false.</returns>
        private static bool ScanNextAndPrev()
        {
            int peekIndexLeft = _index - 1;
            int peekIndexRight = _index + 1;

            while (peekIndexLeft >= 0 && char.IsWhiteSpace(Code[peekIndexLeft]))
            {
                peekIndexLeft--;
            }
            while (peekIndexRight < Code.Length && char.IsWhiteSpace(Code[peekIndexRight]))
            {
                peekIndexRight++;
            }
            return (peekIndexLeft >= 0 && Code[peekIndexLeft] == '$') ||
                   (peekIndexRight < Code.Length && Code[peekIndexRight] == '$');
        }

        /// <summary>
        /// Peeks a character at a given offset from the current index.
        /// </summary>
        /// <param name="offset">The offset from the current index.</param>
        /// <returns>The character at the specified offset, or '\0' if out of bounds.</returns>
        private static char PeekChar(int offset)
        {
            int peekIndex = _index + offset;
            if (peekIndex < 0 || peekIndex >= Code.Length)
            {
                return '\0';
            }
            return Code[peekIndex];
        }

        /// <summary>
        /// Determines how many characters to advance based on the current operator.
        /// </summary>
        /// <param name="currentChar">The current operator character.</param>
        /// <returns>The number of characters to advance.</returns>
        private static int AddIndex(char currentChar)
        {
            switch (currentChar)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                    if (PeekChar(1) == '=')
                        return 2;
                    if (PeekChar(1) == '+' && currentChar == '+')
                        return 2;
                    if (PeekChar(1) == '-' && currentChar == '-')
                        return 2;
                    return 1;
                case '=':
                    if (PeekChar(1) == '=')
                        return 2;
                    return 1;
                case '>':
                    if (PeekChar(1) == '=')
                        return 2;
                    return 1;
                case '<':
                    if (PeekChar(1) == '=' || PeekChar(1) == '>')
                        return 2;
                    return 1;
                default:
                    return 1;
            }
        }

        #endregion HELPER METHODS
    }
}
