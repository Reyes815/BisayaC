using static BisayaC.ErrorStatements;
using static BisayaC.ErrorStatements.ErrorType;

namespace LexicalAnalyzer
{
    public enum TokenType
    {
        SUGOD, KATAPUSAN, SUGODKUNG, HUMANKUNG, PUNDOK, NUMERO, LETRA, TIPIK, PULONG, INTEGERLITERAL,     
        STRINGLITERAL, CHARACTERLITERAL,   
        OO, DILI, FLOATLITERAL, ASAYNMENT, DUGANG, SOBRA, LABAW, UBOS, LABAWSA, UBOSSA, PAREHAS, LAHI,               
        UG, O,TINUOD,KUHA,PADAGHAN,BAHIN,                
        SUMPAY, SUNODLINYA, STORYA, DUHATULDOK, KAMA, ABLIKUTOB, SIRAKUTOB, MUGNA, KUNG, WALA, ALANG, SA,                 
        SAMTANG, IPAKITA, DAWAT, UNKNOWN, EOF, PI, INCREMENT, 
        MODASSIGNMENT, ADDASSIGNMENT, SUBASSIGNMENT, MULASSIGNMENT, DIVASSIGNMENT,IDENTIFIER       
    }
    
    public class Token
    {
        public TokenType Type { get; }

        public string Value { get; }


        public int Line { get; }
        
        public Token(TokenType type, string value, int line)
        {
            Type = type;
            Value = value;
            Line = line;
        }
    }
    
    public static class LexerAnalyzer
    {
        private static int _index = 0;
        private static int _line = 1;
        private static string? _code;
        
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
        
        private static string Code => _code ?? throw new InvalidOperationException("_code is null");


        private static char CurrentChar => Code[_index];
        
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
        
        private static Token HandleComment()
        {
            SkipComment();
            _line++;
            return new Token(TokenType.SUNODLINYA, "\\n", _line - 1);
        }
        
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
                ThrowError(_line, General, "Unterminated escape sequence");
            }

            string content = Code[start..lastClosedBracket];
            return new Token(TokenType.STRINGLITERAL, content, _line);
        }
        
        private static Token ScanCharacter()
        {
            if (Code[_index + 2] != '\'')
            {
                if (Code[_index + 1] == '\'')
                {
                    ThrowError(_line, General, "Empty character literal.");
                }
                ThrowError(_line, General, "Invalid or unterminated character literal.");
            }
            _index++; 
            string character = Code.Substring(_index, 1);
            _index += 2;
            return new Token(TokenType.CHARACTERLITERAL, character, _line);
        }
        
        private static Token ScanString()
        {
            int start = _index;
            _index++; 
            while (_index < Code.Length && CurrentChar != '"')
            {
                _index++;
            }
            if (_index == Code.Length)
            {
                ThrowError(_line, General, "Unterminated string literal.");
            }
            _index++;
            string str = Code[(start + 1)..(_index - 1)];
            if (str.Contains("OO") || str.Contains("DILI"))
            {
                return str.Contains("OO")
                    ? new Token(TokenType.OO, str, _line)
                    : new Token(TokenType.DILI, str, _line);
            }
            return new Token(TokenType.STRINGLITERAL, str, _line);
        }
        
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
                return new Token(TokenType.DILI, "NOT", _line);
            }
            return new Token(TokenType.IDENTIFIER, value, _line);
        }
        
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
        
        private static char PeekChar(int offset)
        {
            int peekIndex = _index + offset;
            if (peekIndex < 0 || peekIndex >= Code.Length)
            {
                return '\0';
            }
            return Code[peekIndex];
        }
        
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
