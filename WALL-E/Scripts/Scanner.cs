namespace PixelWallEInterpreter;
public class Scanner
{
    public string? source {get; private set;}
    public Scanner(string source)
    {
        this.source = source;
    }
    public List<Token> ScanTokens()
    {
        return new List<Token>();
    }
}