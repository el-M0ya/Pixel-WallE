namespace PixelWallEInterpreter;

public class Parser
{

    private class ParseError : Exception { }

    private List<Token> tokens;

    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    private Expr expression()
    {
        return equality();
    }
    private Expr equality()
    {
        Expr expr = comparison();

        while (match([TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL]))
        {
            Token oper = previous();
            Expr right = comparison();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }
    private Expr comparison()
    {
        Expr expr = term();
        while (match([TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL]))
        {
            Token oper = previous();
            Expr right = term();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }
    private bool match(TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }
        return false;
    }
    private bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return peek().type == type;
    }

    private bool isAtEnd()
    {
        return peek().type == TokenType.EOF;
    }
    private Token advance()
    {
        if (!isAtEnd()) current++;
        return previous();
    }
    private Token peek()
    {
        return tokens[current];
    }
    private Token previous()
    {
        return tokens[current - 1];
    }

    private Expr term()
    {
        Expr expr = factor();
        while (match([TokenType.MINUS, TokenType.PLUS]))
        {
            Token oper = previous();
            Expr right = factor();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }

    private Expr factor()
    {
        Expr expr = unary();
        while (match([TokenType.SLASH, TokenType.STAR]))
        {
            Token oper = previous();
            Expr right = unary();
            expr = new Expr.Binary(expr, oper, right);
        }
        return expr;
    }

    private Expr unary()
    {
        if (match([TokenType.BANG, TokenType.MINUS]))
        {
            Token oper = previous();
            Expr right = unary();
            return new Expr.Unary(oper, right);
        }
        return primary();
    }

    private Expr primary()
    {
        if (match([TokenType.NULL])) return new Expr.Literal(null);
        if (match([TokenType.NUMBER, TokenType.STRING])) return new Expr.Literal(previous().literal);
        if (match([TokenType.LEFT_PAREN]))
        {
            Expr expr = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw error(peek(), "Expect expression.");
    }

    private Token consume(TokenType type, string message)
    {
        if (check(type)) return advance();
        throw error(peek(), message);
    }

    // Error Handling-
    

    private ParseError error(Token token, string message)
    {
        PixelWallE.Error(token, message);
        return new ParseError();
    }
    
    private void synchronize()
    {
        advance();
        while (!isAtEnd())
         {
            if (previous().type == TokenType.SEMICOLON) return;
            switch (peek().type) {
            case TokenType.VAR:
            return;
            }
            advance();
            }
 }



    public Expr parse()
    {
        try
        {
            return expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

}

// expression → equality ;
// equality   → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term       → factor ( ( "-" | "+" ) factor )* ;
// factor     → unary ( ( "/" | "*" ) unary )* ;
// unary      → ( "!" | "-" ) unary | primary ;             
// primary    → NUMBER | STRING | "null" | "(" expression ")" ;