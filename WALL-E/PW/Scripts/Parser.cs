namespace PW;

using System;
using System.Collections.Generic;
using System.Drawing;

public class Parser
{
    private class ParseError : Exception { }

    private List<Token> tokens;

    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    // private  Dictionary<TokenType, Func<Expr>> functions = new Dictionary<TokenType, Expr>()
    // {
    //     {TokenType.GETACTUALX , GetActualX()},
    //     {TokenType.GETACTUALY , GetActualY()},
    //     {TokenType.GETCANVASIZE ,  GetCanvasSize()},
    //     {TokenType.GETCOLORCOUNT , GetColorCount()},
    //     {TokenType.ISBRUSHSIZE , IsBrushSize()},
    //     {TokenType.ISBRUSHCOLOR , IsBrushColor()},
    //     {TokenType.ISCANVASCOLOR , IsCanvasColor()}
    // };
    // private  Dictionary<TokenType, Stmt> instructions = new Dictionary<TokenType, Stmt>()
    // {
    //     {TokenType.SPAWN , SpawnStatement()},
    //     {TokenType.COLOR , ColorStatement()},
    //     {TokenType.SIZE ,  SizeStatement()},
    //     {TokenType.DRAWLINE , DrawLineStatement()},
    //     {TokenType.DRAWCIRCLE, DrawCircleStatement()},
    //     {TokenType.DRAWRECTANGLE , DrawRectangleStatement()},
    //     {TokenType.FILL , FillStatement()}
    // };
    private Stmt statement()
    {
        if (match([TokenType.GOTO])) { return GoToStatement(); }
        if (match([TokenType.SPAWN])) { current--; return SpawnStatement(); }
        if (match([TokenType.COLOR])) { current--; return ColorStatement(); }
        if (match([TokenType.DRAWLINE])) { current--; return DrawLineStatement(); }
        if (match([TokenType.DRAWCIRCLE])) { current--; return DrawCircleStatement(); }
        if (match([TokenType.DRAWRECTANGLE])) { current--; return DrawRectangleStatement(); }
        if (match([TokenType.SIZE])) { current--; return SizeStatement(); }
        if (match([TokenType.FILL])) { current--; return FillStatement(); }
        else
            return expressionStatement();

    }
    // private Stmt statement()
    // {

    //     TokenType type = peek().type;
    //     if (!instructions.ContainsKey(type)) return expressionStatement();
    //     return instructions[type];
    // }
    private Stmt expressionStatement()
    {
        Expr expr = expression();

        consume(TokenType.JUMPLINE, "Expect 'line' after expression.");
        return new Stmt.Expression(expr);
    }
    private Expr expression()
    {
        
        return assignment();
    }
    private Stmt declaration()
    {
        try
        {
            // Ignore empty lines
            while (match([TokenType.JUMPLINE])) { }

            if (match([TokenType.IDENTIFIER]))
            {
                if (match([TokenType.ASIGNATION]))
                {
                    current -= 2;
                    return varDeclaration();
                }
                else
                {
                    current--;
                    return labelDeclaration();
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

    // Instructions
    private Stmt SpawnStatement()
    {
        Token name = consume(TokenType.SPAWN, "No Spawn statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Spawn' Instruction'.");

        Expr x = expression();
        consume(TokenType.COMMA, "Expect ',' after 'x'.");
        Expr y = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'y'.");


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.Spawn(name, x, y);
    }
    private Stmt ColorStatement()
    {
        Token name = consume(TokenType.COLOR, "No Color statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Color' Instruction'.");

        Expr color = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'color'.");


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.Color(name, color);
    }

    private Stmt SizeStatement()
    {
        Token name = consume(TokenType.SIZE, "No Size statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Size' Instruction'.");

        Expr k = expression();


        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'number'.");


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.Size(name, k);
    }
    private Stmt DrawLineStatement()
    {
        Token name = consume(TokenType.DRAWLINE, "No DrawLine statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawLine' Instruction'.");

        Expr dirX = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr dirY = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr distance = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'distance'.");


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.DrawLine(name, dirX, dirY, distance);
    }

    private Stmt DrawCircleStatement()
    {
        Token name = consume(TokenType.DRAWCIRCLE, "No DRAWCIRCLE statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' Instruction'.");

        Expr dirX = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr dirY = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr radius = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after 'radius'.");


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.DrawCircle(name, dirX, dirY, radius);
    }
    private Stmt DrawRectangleStatement()
    {
        Token name = consume(TokenType.DRAWRECTANGLE, "No DrawRectangle statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'DrawCircle' Instruction'.");

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


        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.DrawRectangle(name, dirX, dirY, distance, width, high);
    }
    private Stmt FillStatement()
    {
        Token name = consume(TokenType.FILL, "No Fill statement found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'Fill' Instruction'.");
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");

        if (!match([TokenType.JUMPLINE, TokenType.EOF])) error(peek(), "Expexted line");

        return new Stmt.Fill(name);
    }

    // Functions

    private Expr GetActualX()
    {
        Token name = consume(TokenType.GETACTUALX, "No GetActualX expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualX' function'.");
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");


        return new Expr.GetActualX();
    }
    private Expr GetActualY()
    {
        Token name = consume(TokenType.GETACTUALY, "No GetActualY expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualY' function'.");
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");


        return new Expr.GetActualY();
    }
    private Expr GetCanvasSize()
    {
        Token name = consume(TokenType.GETCANVASIZE, "No GetCanvasSize expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetCanvasSize' function'.");
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");

        return new Expr.GetCanvasSize();
    }
    private Expr GetColorCount()
    {
        
        Token name = consume(TokenType.GETCOLORCOUNT, "No GetColorCount expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualX' function'.");

        Expr color = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr x1 = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr y1 = expression();
        consume(TokenType.COMMA, "Expect ',' after 'distance'.");
        Expr x2 = expression();
        consume(TokenType.COMMA, "Expect ',' after 'width'.");
        Expr y2 = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");


        return new Expr.GetColorCount(name, color, x1, y1, x2, y2);
    }
    private Expr IsBrushSize()
    {
        Token name = consume(TokenType.ISBRUSHSIZE, "No IsBrushSize expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualX' function'.");
        Expr size = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");


        return new Expr.IsBrushSize(name, size);
    }
    private Expr IsBrushColor()
    {
        Token name = consume(TokenType.ISBRUSHCOLOR, "No IsBrushColor expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualX' function'.");
        Expr color = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");



        return new Expr.IsBrushColor(name, color);
    }
    private Expr IsCanvasColor()
    {
        Token name = consume(TokenType.ISCANVASCOLOR, "No IsCanvasColor expression found");
        consume(TokenType.LEFT_PAREN, "Expect '(' after 'GetActualX' function'.");

        Expr color = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirX'.");
        Expr x = expression();
        consume(TokenType.COMMA, "Expect ',' after 'dirY'.");
        Expr y = expression();

        consume(TokenType.RIGHT_PAREN, "Expect ')' after '('.");

        return new Expr.IsCanvasColor(name, color, x, y);
    }

    private Expr assignment()
    {
        Expr expr = or();
        if (match([TokenType.ASIGNATION]))
        {
            Token equals = previous();
            Expr value = assignment();
            if (expr is Expr.Var var)
            {
                Token name = var.name;
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
        while (match([TokenType.AND]))
        {
            Token operat = previous();
            Expr right = equality();
            expr = new Expr.Logical(expr, operat, right);
        }
        return expr;
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
        while (match([TokenType.SLASH, TokenType.STAR, TokenType.DOUBLE_STAR, TokenType.PERCENT]))
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

        if (match([TokenType.GETACTUALX])) { current -= 1; return GetActualX(); }
        if (match([TokenType.GETACTUALY])) {current -= 1 ; return GetActualY();}
        if (match([TokenType.GETCANVASIZE])) {current -= 1 ; return GetCanvasSize();}
        if (match([TokenType.GETCOLORCOUNT])){current -= 1 ;  return GetColorCount();}
        if (match([TokenType.ISBRUSHSIZE])) {current -= 1 ; return IsBrushSize();}
        if (match([TokenType.ISBRUSHCOLOR])) {current -= 1 ; return IsBrushColor();}
        if (match([TokenType.ISCANVASCOLOR])) {current -= 1 ; return IsCanvasColor();}
        if (match([TokenType.NULL]))  return new Expr.Literal(null);
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
            if (previous().type == TokenType.JUMPLINE) return;
            switch (peek().type)
            {
                case TokenType.IDENTIFIER:
                case TokenType.GOTO:
                case TokenType.SPAWN:
                case TokenType.COLOR:
                case TokenType.SIZE:
                case TokenType.DRAWLINE:
                case TokenType.DRAWCIRCLE:
                case TokenType.DRAWRECTANGLE:
                case TokenType.FILL:
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
}

// expression → assignment ;
// assignment → Identifier ( ( "<-" ) and) ;
// and        → or ( ( "&&" ) or) ;
// or         → equality ( ( "||" ) equality) ;
// equality   → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term       → factor ( ( "-" | "+" ) factor )* ;
// factor     → unary ( ( "/" | "*" | "**" | "%") unary )* ;
// unary      → ( "!" | "-" ) unary | primary ;             
// primary    → NUMBER | STRING | "null" | "(" expression ")" ;