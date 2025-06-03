namespace PixelWallEInterpreter;

class Environment
{
    private Dictionary<string, object> values = new Dictionary<string, object>();
    private Dictionary<string, int> labels = new Dictionary<string, int>();

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
        values.Add(name, value);
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
    public int getLine(Token label)
    {
        if (labels.ContainsKey(label.lexeme)) return labels[label.lexeme];

        throw new RuntimeError(label, "Undefined label '" + label.lexeme + "'");
    }
    public void assignlabel(Token label, int line)
    {
        if (labels.ContainsKey(label.lexeme))
        {
            throw new RuntimeError(label, "Unexpected name: labels with the same name");
        }
        labels.Add(label.lexeme, line);
        return;
    }
}