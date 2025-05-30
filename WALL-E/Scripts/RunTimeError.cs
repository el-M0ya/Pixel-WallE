namespace PixelWallEInterpreter;

class RuntimeError : Exception
{
    Token token;
   public RuntimeError(Token token, string message)
    {
        //super(message);
        this.token = token;
    }
}