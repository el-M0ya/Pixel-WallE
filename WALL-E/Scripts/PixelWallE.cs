namespace PixelWallEInterpreter;
public class PixelWallE
{
    public static bool HadError{get; private set;} = false;
    public static void Main(String[] args)
    {  
        if (args.Length > 1) 
        {
            Console.WriteLine("Usage: cpw [script]");
            Environment.Exit(64); // Bad Implementation Error
        } 
        else if (args.Length == 1) RunFile(args[0]);

        else RunPrompt();
    }
    // Execute a .pw file
    private static void RunFile(string path)
    {
        string code = File.ReadAllText(path);
        Run(code);
    }
    // Interactive Mode
    private static void RunPrompt()
    {
        
        while(true)
        {
            Console.WriteLine("--> ");
            string? line = Console.ReadLine();
            if(line == null) break;
            Run(line);
        }
    }
    private static void Run(String source) 
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        // For now, just print the tokens.
        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }
    private static void Error(int line , string message)
    {
        Report(line , "" , message);
    }
    private static void Report(int line , string where, string message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        HadError = true;
    }
}