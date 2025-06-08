namespace PW;

using System;
using System.Collections.Generic;
using System.IO;

public class PixelWallE
{
    private static  Interpreter interpreter = new Interpreter();
    public static bool HadError { get; private set; } = false;
    public static bool HadRuntimeError { get; private set; } = false;

    public PixelWallE() { }
   
    public  void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cpw [script]");
            System.Environment.Exit(64); // Bad Implementation Error
        }
        else if (args.Length == 1) LoadFile(args[0]);

        else RunPrompt();
    }
    // Interactive Mode
    private  void RunPrompt()
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
    public  void Run(string source)
    {
        HadError = false;
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.parse();
        // Stop if there was a syntax error.
        if (HadError) return;
        
        interpreter.interpret(statements);
        MainWindow.SetStatus($"Ejecutando", false); 
    }

    public static void SaveFile(string code, string path)
    {
        try
        {
            if (!path.EndsWith(".pw", StringComparison.OrdinalIgnoreCase))
            {
                path += ".pw";
            }
            File.WriteAllText(path, code);
            System.Console.WriteLine($"Saved file: {path}");
        }
        catch (Exception error)
        {

            Console.WriteLine($"Export Error:{error.Message}");
        }
    }

    public static string LoadFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new RuntimeError($"File not found: {path}");
            }
            string code = File.ReadAllText(path);
            return code;
        }
        catch (Exception error)
        {

            MainWindow.SetStatus($"Import Error:{error.Message}" , true);
            return null;
        }
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
        MainWindow.SetStatus("[line " + line + "] Error" + where + ": " + message , true);
        HadError = true;
    }
    public static void runtimeError(RuntimeError error)
    {
        MainWindow.SetStatus(error.getMessage()   +  "\n[line " + error.token.line + "]" , true);
        HadRuntimeError = true;
    }
}