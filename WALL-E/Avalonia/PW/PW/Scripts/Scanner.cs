namespace PW;

using System;
using System.Collections.Generic;

public class Scanner
{
    private int start = 0;
    private int current = 0;
    private int line = 1;
    private string source;
    private List<Token> tokens = new List<Token>();
    private static Dictionary<string, TokenType>? keywords = new Dictionary<string, TokenType>()
    {
        { "var", TokenType.VAR},
        { "GoTo", TokenType.GOTO},
        {"Spawn", TokenType.SPAWN},
        {"Color", TokenType.COLOR},
        {"Size", TokenType.SIZE},
        {"DrawLine", TokenType.DRAWLINE},
        {"DrawCircle", TokenType.DRAWCIRCLE},
        {"DrawRectangle", TokenType.DRAWRECTANGLE},
        {"Fill", TokenType.FILL},
        {"GetActualX", TokenType.GETACTUALX},
        {"GetActualY", TokenType.GETACTUALY},
        {"GetCanvaSize", TokenType.GETCANVASIZE},
        {"GetCanvaColorCount", TokenType.GETCOLORCOUNT},
        {"IsCanvasColor", TokenType.ISCANVASCOLOR},
        {"IsBrushColor", TokenType.ISBRUSHCOLOR},
        {"IsBrushSize", TokenType.ISBRUSHSIZE},

    };

    public Scanner(string source)
    {
        this.source = source;
    }
    public List<Token> ScanTokens()
    {
        while (!isAtEnd())
        {
            start = current;
            ScanToken();
        }
        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }
    private bool isAtEnd()
    {
        return current >= source.Length;
    }
    private void ScanToken()
    {
        char c = advance();
        switch (c)
        {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '[': addToken(TokenType.LEFT_SQUARE); break;
            case ']': addToken(TokenType.RIGHT_SQUARE); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.JUMPLINE); line++;  break;
            

            case '&':
                if (Peek() == '&') addToken(TokenType.AND); break;

            case '=':
                if (Peek() == '=') addToken(TokenType.EQUAL_EQUAL); break;

            case '|':
                if (Peek() == '|') addToken(TokenType.OR); break;

            case '*':
                addToken(Match('*') ? TokenType.DOUBLE_STAR : TokenType.STAR); break;

            case '!':
                addToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;

            case '>':
                addToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

            case '<':
                if (Match('=')) addToken(TokenType.LESS_EQUAL);
                if (Match('-')) addToken(TokenType.ASIGNATION);
                else addToken(TokenType.LESS);
                break;

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !isAtEnd()) advance();
                }
                else
                {
                    addToken(TokenType.SLASH);
                }
                break;

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                addToken(TokenType.JUMPLINE);
                line++;
                break;

            case '"': String(); break;

            default:
                if (IsDigit(c)) Number();
                else if (IsAlpha(c))
                    Identifier();
                else
                    PixelWallE.Error(line, "Unexpected character.");
                break;
        }
    }
    private char advance()
    {
        current++;
        return source[current - 1];
    }
    private void addToken(TokenType type)
    {
        addToken(type, null);
    }
    private void addToken(TokenType type, object literal)
    {
        string text = SubString(source , start, current - 1);
        tokens.Add(new Token(type, text, literal, line));
        
    }
    private bool Match(char expected)
    {
        if (isAtEnd()) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }
    private char Peek()
    {
        if (isAtEnd()) return '\0';
        return source[current];
    }
    private void String()
    {
        while (Peek() != '"' && Peek() != '\n')
        {
            if (isAtEnd() || Peek() == '\n')
            {
                PixelWallE.Error(line, "Unterminated string.");
                return;
            }
            advance();
        }
        
        // The closing ".
        advance();
        // Trim the surrounding quotes.
        string value = SubString(source , start + 1, current - 2);
        addToken(TokenType.STRING, value);
    }
    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }
    private void Number()
    {
        while (IsDigit(Peek())) advance();
        addToken(TokenType.NUMBER, int.Parse(SubString(source , start, current - 1)));
    }
    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) advance();
        TokenType type ;
        string text = SubString(source , start, current - 1);
        if(!keywords.TryGetValue(text , out type)) type = TokenType.IDENTIFIER;
        addToken(type);
    }
    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
        (c >= 'A' && c <= 'Z') ||
        c == '_';
    }
    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    /*  expression → literal
                    | unary
                    | binary
                    | grouping ;
          literal  → NUMBER | STRING | "true" | "false" | "nil" ;
          grouping → "(" expression ")" ;
          unary    → ( "-" | "!" ) expression ;
          binary   → expression operator expression ;
          operator → "==" | "!=" | "<" | "<=" | ">" | ">="
                          | "+" | "-" | "*" | "/" ;
   */
    public string SubString(string str , int start , int end)
    {
        if(start > end) throw new Exception($"SubStringError: Start > end in line:{line}");
        string result = "";
        while(start <= end)
        {
            result += str[start];
            start++;
        }
        return result;
    } 


}


