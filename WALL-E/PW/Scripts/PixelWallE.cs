namespace PW;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

public class PixelWallE
{
    private static Interpreter interpreter = new Interpreter();
    public static bool HadError { get; private set; } = false;
    public static bool HadRuntimeError { get; private set; } = false;

    public PixelWallE() { }
    public async Task Run(string source , CancellationToken cancellationToken = default)
    {
        HadError = false;
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.parse();
        // Stop if there was a syntax error.
        if (HadError) return;

        await interpreter.interpret(statements , cancellationToken);
        
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
        MainWindow.SetStatus("[line " + line + "] Error" + where + ": " + message, true);
        HadError = true;
    }
    public static void runtimeError(RuntimeError error)
    {
        MainWindow.SetStatus(error.getMessage() + "\n[line " + error.token.line + "]", true);
        HadRuntimeError = true;
    }
}