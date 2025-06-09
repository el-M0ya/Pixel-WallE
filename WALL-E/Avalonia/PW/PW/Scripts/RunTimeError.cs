namespace PW;

using System;

public class RuntimeError : Exception
{
    public Token? token;

    public RuntimeError(Token token, string message) : base(message)
    {
        this.token = token;
    }
    public string getMessage()
    {
        return Message;
    }
}

