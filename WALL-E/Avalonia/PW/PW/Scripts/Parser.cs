namespace PW;

using System;
using System.Collections.Generic;

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
                else if (match([TokenType.LEFT_SQUARE]))
                {
                    current--;
                    GoToStatement();
                }
                else
                {
                    current--;
                    labelDeclaration();
                }
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
    private Stmt labelDeclaration()
    {
        Token label = consume(TokenType.IDENTIFIER, "Expect variable name.");
        consume(TokenType.JUMPLINE, "Expect 'line' after label declaration.");
        return new Stmt.Label(label);

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

        Expr x = expression();
        consume(TokenType.COMMA, "Expect ',' after 'x'.");
        Expr y = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'y'.");

        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.Spawn(x, y);
    }
    private Stmt ColorStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Color' function'.");

        Expr color = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'color'.");

        consume(TokenType.JUMPLINE, "Expect 'line' .");

        return new Stmt.Color(color);
    }

    private Stmt SizeStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Size' function'.");

        Expr k = expression();
        

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'number'.");

        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.Size(k);
    }
    private Stmt DrawLineStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawLine' function'.");

        Expr dirX = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr dirY = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr distance = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'distance'.");
        
        consume(TokenType.JUMPLINE, "Expect 'line'.");

        return new Stmt.DrawLine(dirX, dirY, distance);
    }

    private Stmt DrawCircleStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' function'.");

        Expr dirX = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr dirY = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr radius = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'radius'.");
        
        consume(TokenType.JUMPLINE, "Expect 'line' .");

        return new Stmt.DrawCircle(dirX, dirY, radius);
    }
    private Stmt DrawRectangleStatement()
    {
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' function'.");

        Expr dirX = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
       Expr dirY = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr distance = expression();
        consume(TokenType.COMMA, "Expect ',' after 'distance'.");
        Expr width = expression();
        consume(TokenType.COMMA, "Expect ',' after 'width'.");
        Expr high = expression();

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
                return new Expr.Assign(name, value);
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