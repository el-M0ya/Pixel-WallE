namespace PixelWallEInterpreter;

public class PixelWallE
{
    private static  Interpreter interpreter = new Interpreter();
    public static bool HadError { get; private set; } = false;
    public static bool HadRuntimeError { get; private set; } = false;
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cpw [script]");
            System.Environment.Exit(64); // Bad Implementation Error
        }
        else if (args.Length == 1) RunFile(args[0]);

        else RunPrompt();
    }
    // Execute a .pw file
    private static void RunFile(string path)
    {
        string code = File.ReadAllText(path);
        Run(code);
        if (HadError) System.Environment.Exit(65);
        if (HadRuntimeError) System.Environment.Exit(70);
    }
    // Interactive Mode
    private static void RunPrompt()
    {

        while (true)
        {
            Console.Write("--> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            HadError = false;
        }
    }
    private static void Run(string source)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.parse();
        // Stop if there was a syntax error.
        if (HadError) return;
        interpreter.interpret(statements);


    }
    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }
    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }
    private static void Report(int line, string where, string message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        HadError = true;
    }
    public static void runtimeError(RuntimeError error)
    {
        Console.WriteLine(error.getMessage()   +  "\n[line " + error.token.line + "]");
        HadRuntimeError = true;
    }
}