namespace PW;

using System;
using System.Collections.Generic;

class Environment
{
    private Dictionary<string, object> values = new Dictionary<string, object>();
    private List<Token> labels = new List<Token>();


    public object get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
            MainWindow.SetStatus($"{values[name.lexeme]}", false);
            return values[name.lexeme];
        }
        throw new RuntimeError(name,
        "Undefined variable '" + name.lexeme + "'.");
    }
    public void define(string name, object value)
    {
        if (values.ContainsKey(name)) values[name] = value;
        else values.Add(name, value);
    }
    public void assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values.Add(name.lexeme, value);
            return;
        }
        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }
    public int getLine(Token name)
    {

        foreach (Token item in labels)
        {
            if (item.lexeme == name.lexeme) return name.line;
        }
        throw new RuntimeError(name,
        "Undefined label '" + name.lexeme + "'.");
    }
    public void assignLabel(Token name)
    {
        if (!labels.Contains(name))
        {
            labels.Add(name);
            return;
        }
        throw new RuntimeError(name, "Labels with the same name '" + name.lexeme + "'.");
    }
    

}