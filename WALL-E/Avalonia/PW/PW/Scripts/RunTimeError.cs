namespace PW;

using System;

public class RuntimeError : Exception
{
    public Token? token;

    public RuntimeError(Token token, string message) : base(message)
    {
        this.token = token;
    }
    public RuntimeError(string message) : base(message){ MainWindow.SetStatus(message, true); }
    public string getMessage()
    {
        return Message;
    }
}

