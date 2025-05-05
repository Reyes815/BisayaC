using LexicalAnalyzer;
namespace BisayaC
{
    internal static class mainprogram
    {
        public static void Main(string[] args)
        {
            bool debugMode = false;

            if (debugMode)
            {
                if (PromptUser("\nType 'Y' to start Interpreter || Type 'N' to stop Interpreter:\n") == "Y")
                {
                }
                else
                {
                    RunInterpreterLoop();
                }
            }
            else
            {
                RunInterpreterLoop();
            }
        }

        private static void RunInterpreterLoop()
        {
            while (true)
            {
                string userInput = PromptUser("\nType 'Y' to start Interpreter || Type 'N' to stop Interpreter:\n");
                if (userInput == "Y")
                {
                    try
                    {
                        CompileAndInterpret();
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Failed: {ex.Message}");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static void CompileAndInterpret()
        {
            string sourcePath = "C:\\Users\\Rowen\\Desktop\\BisayaC\\Main\\inputhere.txt";

            if (!File.Exists(sourcePath))
            {
                ShowError("Source file not found.");
                return;
            }

            string sourceCode = File.ReadAllText(sourcePath);

            try
            {
                var tokens = LexerAnalyzer.Tokenize(sourceCode);
                var parser = new SyntaxAnalyzer(tokens);
                var ast = parser.Parse();

                ShowSuccess(".............................\nCompiled successfully\n");

                var interpreter = new Interpreter();
                interpreter.Interpret(ast);
            }
            catch (Exception ex)
            {
                ShowError($"Failed: {ex.Message}");
            }
        }

        private static string PromptUser(string message)
        {
            Console.Write(message);
            return Console.ReadLine()?.ToUpper() ?? "N";
        }

        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
