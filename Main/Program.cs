namespace BisayaC
{
    /// <summary>
    /// Entry point for the Bisaya++ Compiler.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main method that starts the compiler.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            PrintHeader();
            bool debugMode = false;

            if (debugMode)
            {
                // Ask user if test cases should be run.
                if (GetOption("Run test cases? (Y/N): ") == "Y")
                {
                    TestCases.RunTests();
                }
                else
                {
                    ExecuteProgram();
                }
            }
            else
            {
                ExecuteProgram();
            }

            Console.WriteLine("\nExiting Bisaya++ Compiler. Goodbye!");
        }

        /// <summary>
        /// Repeatedly prompts the user to run code from "program.bisaya++" until the user chooses not to.
        /// </summary>
        private static void ExecuteProgram()
        {
            while (true)
            {
                if (GetOption("\nRun code in program.bisaya++ (Y/N): ") == "Y")
                {
                    try
                    {
                        CompileProgram();
                    }
                    catch (Exception ex)
                    {
                        PrintError($"Compilation failed: {ex.Message}");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Compiles the program by reading the source file, performing lexical and syntax analysis,
        /// displaying a token summary and AST, and interpreting the code.
        /// </summary>
        private static void CompileProgram()
        {
            // Determine the project folder by navigating three levels up.
            string currentDirectory = Environment.CurrentDirectory;
            var projectFolder = Directory.GetParent(
                                    Directory.GetParent(
                                        Directory.GetParent(currentDirectory).ToString()
                                    ).ToString()
                                );

            string filename = "C:/Users/Rowen/Desktop/BisayaC/Main/Editor.txt";
            string filePath = Path.Combine(projectFolder.ToString(), filename);

            if (!File.Exists(filename))
            {
                PrintError($"Error: File not found: {filePath}");
                return;
            }

            string code = File.ReadAllText(filename);
            Console.WriteLine($"Source file loaded: {filePath}");
            Console.WriteLine("----------------------------------------");

            try
            {
                // Lexical Analysis Phase
                Console.WriteLine("\n[1] Running lexical analysis...");
                List<Token> tokens = Lexer.Tokenize(code);
                Console.WriteLine($"Lexical analysis complete. Found {tokens.Count} tokens.");
                foreach (var token in tokens)
                {
                    Console.WriteLine($"Line: {token.Line}, Token: {token.Type}, Value: '{token.Value}'");
                }

                // Display token summary
                DisplayTokenSummary(tokens);

                // Syntax Analysis Phase
                Console.WriteLine("\n[2] Running syntax analysis...");
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                // Display the parsed AST
                Console.WriteLine("\nParsed Abstract Syntax Tree (AST):");
                foreach (var statement in ast.Statements)
                {
                    Console.WriteLine($"Line {statement.LineNumber}: {statement.GetType().Name}");
                }
                Console.WriteLine($"Syntax analysis complete. Program has {ast.Statements.Count} statements.");

                // Success message
                PrintSuccess("\nCompilation successful! ✓");

                // Interpret the AST
                var interpreter = new InterpreterClass();
                interpreter.Interpret(ast);
            }
            catch (Exception ex)
            {
                PrintError($"\nCompilation failed: {ex.Message}");
            }

            Console.WriteLine("\n----------------------------------------");
        }

        /// <summary>
        /// Displays a summary of the top 10 most common token types.
        /// </summary>
        /// <param name="tokens">The list of tokens generated during lexical analysis.</param>
        private static void DisplayTokenSummary(List<Token> tokens)
        {
            var tokenGroups = tokens
                .GroupBy(t => t.Type)
                .OrderByDescending(g => g.Count())
                .Take(10);

            Console.WriteLine("\nToken summary (top 10 types):");
            foreach (var group in tokenGroups)
            {
                Console.WriteLine($"  {group.Key}: {group.Count()} occurrences");
            }
        }

        /// <summary>
        /// Prompts the user with the given message and returns the uppercase response.
        /// </summary>
        /// <param name="prompt">The prompt message to display.</param>
        /// <returns>The user's response in uppercase, or "N" if no response is provided.</returns>
        private static string GetOption(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.ToUpper() ?? "N";
        }

        /// <summary>
        /// Prints the application header.
        /// </summary>
        private static void PrintHeader()
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("   Bisaya++ Compiler");
            Console.WriteLine("=========================================");
        }

        /// <summary>
        /// Prints an error message in red.
        /// </summary>
        /// <param name="message">The error message to print.</param>
        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Prints a success message in green.
        /// </summary>
        /// <param name="message">The success message to print.</param>
        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
