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
        return assignment();
    }
    private Stmt declaration()
    {
        try
        {
            if (match([TokenType.IDENTIFIER]))
            {
                if (match([TokenType.ASIGNATION]))
                {
                    current -= 2;
                    varDeclaration();
                }
                else GoToStatement();
            }
            return statement();
        }
        catch (ParseError error)
        {
            synchronize();
            return null;
        }
    }

    private Stmt varDeclaration()
    {
        Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expr initializer = null;
        if (match([TokenType.ASIGNATION]))
        {
            initializer = expression();
        }
        consume(TokenType.JUMPLINE, "Expect 'line' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

     private Stmt GoToStatement()
    {
        consume(TokenType.LEFT_SQUARE, "Expect '[' after 'GoTo'.");
        Token label = consume(TokenType.IDENTIFIER, "Expect label name.");
        consume(TokenType.RIGHT_SQUARE, "Expect ']' after label.");
        
        consume(TokenType.LEFT_PAREN, "Expect '(' after '[label]'.");
        Expr condition = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        
        consume(TokenType.JUMPLINE, "Expect 'line' after GoTo statement.");
        
        return new Stmt.GoTo(label, condition);
 }
    private Stmt SpawnStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Spawn' function'.");

        int x = (int)consume(TokenType.NUMBER, "Expect 'number' x").literal;
        consume(TokenType.COMMA, "Expect ',' after 'x'.");
        int y = (int)consume(TokenType.NUMBER, "Expect 'number' y").literal;

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'y'.");

        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.Spawn(x, y);
    }
    private Stmt ColorStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Color' function'.");

        string color = consume(TokenType.STRING, "Expect 'string' color").literal.ToString();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'color'.");

        consume(TokenType.JUMPLINE, "Expect 'line' .");

        return new Stmt.Color(color);
    }

    private Stmt SizeStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Size' function'.");

        int k = (int)consume(TokenType.NUMBER, "Expect 'number'").literal;
        

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'number'.");

        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.Size(k);
    }
    private Stmt DrawLineStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawLine' function'.");

        int dirX = (int)consume(TokenType.NUMBER, "Expect 'number' dirX").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        int dirY = (int)consume(TokenType.NUMBER, "Expect 'number' dirY").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        int distance = (int)consume(TokenType.NUMBER, "Expect 'number' distance").literal;

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'distance'.");
        
        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.DrawLine(dirX, dirY, distance);
    }

    private Stmt DrawCircleStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' function'.");

        int dirX = (int)consume(TokenType.NUMBER, "Expect 'number' dirX").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        int dirY = (int)consume(TokenType.NUMBER, "Expect 'number' dirY").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        int radius = (int)consume(TokenType.NUMBER, "Expect 'number' radius").literal;

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'radius'.");
        
        consume(TokenType.JUMPLINE, "Expect 'line' .");

        return new Stmt.DrawCircle(dirX, dirY, radius);
    }
    private Stmt DrawRectangleStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' function'.");

        int dirX = (int)consume(TokenType.NUMBER, "Expect 'number' dirX").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        int dirY = (int)consume(TokenType.NUMBER, "Expect 'number' dirY").literal;
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        int distance = (int)consume(TokenType.NUMBER, "Expect 'number' distance").literal;
        consume(TokenType.COMMA, "Expect ',' after 'distance'.");
        int width = (int)consume(TokenType.NUMBER, "Expect 'number' width").literal;
        consume(TokenType.COMMA, "Expect ',' after 'width'.");
        int high = (int)consume(TokenType.NUMBER, "Expect 'number' high").literal;

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'high'.");
        
        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.DrawRectangle(dirX, dirY, distance, width, high);
    }
    private Stmt FillStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Fill' function'.");
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");
        consume(TokenType.JUMPLINE, "Expect 'line'.");

        // Fix with walle.x and walle.y 
        return new Stmt.Fill(0, 0);
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
        if (match([TokenType.IDENTIFIER])) return new Expr.Var(previous());
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
            switch (peek().type)
            {
                case TokenType.VAR:
                    return;
            }
            advance();
        }
    }
    public List<Stmt> parse()
    {
        List<Stmt> statements = new List<Stmt>();
        while (!isAtEnd())
        {
            statements.Add(declaration());
        }
        return statements;
    }
    private Stmt statement()
    {
        if (match([TokenType.SPAWN])) return SpawnStatement();
        if (match([TokenType.COLOR])) return ColorStatement();
        if (match([TokenType.DRAWLINE])) return DrawLineStatement();
        if (match([TokenType.DRAWCIRCLE])) return DrawCircleStatement();
        if (match([TokenType.DRAWRECTANGLE])) return DrawRectangleStatement();
        if (match([TokenType.SIZE])) return SizeStatement();
        if (match([TokenType.FILL])) return FillStatement();
        else
        return expressionStatement();

    }
    private Stmt expressionStatement()
    {
        Expr expr = expression();

        consume(TokenType.JUMPLINE, "Expect 'line' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr assignment()
    {
        Expr expr = or();
        if (match([TokenType.ASIGNATION]))
        {
            Token equals = previous();
            Expr value = assignment();
            if (expr is Expr.Var)
            {
                Token name = ((Expr.Var)expr).name;
                return new Expr.Assign.Variable(name, value);
            }
            error(equals, "Invalid assignment target.");
        }
        return expr;
    }
    private Expr or()
    {
        Expr expr = and();
        while (match([TokenType.OR]))
        {
            Token operat = previous();
            Expr right = and();
            expr = new Expr.Logical(expr, operat, right);
        }
        return expr;
    }
    private Expr and()
    {
        Expr expr = equality();
        while (match([TokenType.AND])) {
        Token operat = previous();
        Expr right = equality();
        expr = new Expr.Logical(expr, operat, right);
        }
        return expr;
 }
}

// expression → equality ;
// equality   → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term       → factor ( ( "-" | "+" ) factor )* ;
// factor     → unary ( ( "/" | "*" ) unary )* ;
// unary      → ( "!" | "-" ) unary | primary ;             
// primary    → NUMBER | STRING | "null" | "(" expression ")" ;