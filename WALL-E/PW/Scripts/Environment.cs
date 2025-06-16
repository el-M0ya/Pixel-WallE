namespace PW;

using System;
using System.Collections.Generic;

class Environment
{
    
    private Dictionary<string, object> values = new Dictionary<string, object>();
    private Dictionary<string , int> labels = new Dictionary<string, int>();

    public void Reset()
    {
        values.Clear();
        labels.Clear();
    }

    public object get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
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
        if (labels.TryGetValue(name.lexeme, out int line))
        {
            return line;
        }
        throw new RuntimeError(name, "Undefined label '" + name.lexeme + "'.");
    }

    public void assignLabel(Token name)
    {
        if (!labels.ContainsKey(name.lexeme))
        {
            // Guarda el lexema y la línea REAL del label
            labels.Add(name.lexeme, name.line); 
            return;
        }
        throw new RuntimeError(name, "Duplicate label '" + name.lexeme + "'.");
    }
}
    

