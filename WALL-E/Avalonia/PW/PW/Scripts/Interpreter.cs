namespace PW;

using System;
using System.Collections.Generic;

public class Interpreter : Expr.IVisitor<object> , Stmt.IVisitor
{
    private Environment environment = new Environment();
    private int line = 1;
    public object visitBinary(Expr.Binary expr)
    {
        object left = evaluate(expr.left);
        object right = evaluate(expr.right);
        switch (expr.operat.type)
        {
            case TokenType.PLUS:
                if (left is int && right is int)
                {
                    return (int)left + (int)right;
                }
                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }
                throw new RuntimeError(expr.operat, "Operands must be two numbers or two strings.");

            case TokenType.MINUS:
                checkNumberOperands(expr.operat, left, right);
                return (int)left - (int)right;

            case TokenType.SLASH:
                checkNumberOperands(expr.operat, left, right);
                if ((int)right == 0) throw new RuntimeError(expr.operat, "Division by 0 is not defined");
                return (int)left / (int)right;

            case TokenType.STAR:
                checkNumberOperands(expr.operat, left, right);
                return (int)left * (int)right;

            case TokenType.GREATER:
                checkNumberOperands(expr.operat, left, right);
                return (int)left > (int)right;


            case TokenType.GREATER_EQUAL:
                checkNumberOperands(expr.operat, left, right);
                return (int)left >= (int)right;

            case TokenType.LESS:
                checkNumberOperands(expr.operat, left, right);
                return (int)left < (int)right;

            case TokenType.LESS_EQUAL:
                checkNumberOperands(expr.operat, left, right);
                return (int)left <= (int)right;

            case TokenType.BANG_EQUAL: return !isEqual(left, right);

            case TokenType.EQUAL_EQUAL: return isEqual(left, right);

        }
        // Unreachable.
        return null;
    }
    public object visitUnary(Expr.Unary expr)
    {

        object right = evaluate(expr.right);
        switch (expr.operat.type)
        {
            case TokenType.BANG:
                return !isTruth(right);
            case TokenType.MINUS:
                return -(int)right;
        }
        return null;
        // Unreachable.

    }

    public object visitGrouping(Expr.Grouping expr)
    {
        return evaluate(expr.expression);
    }
    public object visitLiterals(Expr.Literal expr)
    {
        return expr.value;
    }
     public object visitLogical(Expr.Logical expr)
    {
        object left = evaluate(expr.left);
        if (expr.operat.type == TokenType.OR) {
        if (isTruth(left)) return left;
        } else {
        if (!isTruth(left)) return left;
        }
        return evaluate(expr.right);
    }

    public object visitVar(Expr.Var expr)
    {
        return environment.get(expr.name);
    }
  

    public void visitExprStmt(Stmt.Expression stmt)
    {
        evaluate(stmt.expression);
    }

    public void visitSpawnStmt(Stmt.Spawn spawn)
    {
        // Estoy cansado , jefe
        
    }
    public void visitColorStmt(Stmt.Color color)
    {
        Console.WriteLine("Color");
    }
    public void visitSizeStmt(Stmt.Size size)
    {
        Console.WriteLine("Size");
    }
    public void visitDrawLineStmt(Stmt.DrawLine drawLine)
    {
        Console.WriteLine("DrawLine");
    }
    public void visitDrawCircleStmt(Stmt.DrawCircle drawCircle)
    {
        Console.WriteLine("DrawCircle");
    }
    public void visitDrawRectangleStmt(Stmt.DrawRectangle drawRectangle)
    {
        Console.WriteLine("DrawRectangle");
    }
    public void visitFillStmt(Stmt.Fill fill)
    {
        Console.WriteLine("Fill");
    }
    public void visitGoToStmt(Stmt.GoTo stmt)
    {
        if (isTruth(stmt.condition))
        {
            line = environment.getLine(stmt.label);
        }
    }
    public void visitVarStmt(Stmt.Var stmt)
    {
        object value = null;
        if (stmt.initializer != null) value = evaluate(stmt.initializer);
        environment.define(stmt.name.lexeme, value);
        

    }
    public void visitLabelStmt(Stmt.Label stmt)
    {
        environment.assignLabel(stmt.label);

    }
    public object visitAssign(Expr.Assign expr)
    {
        object value = evaluate(expr.value);
        environment.define(expr.name.lexeme, value);
        return value;
    }


    private bool isTruth(object obj)
    {
        if (obj == null) return false;
        if (obj is bool) return (bool)obj;
        return true;
    }
    private static bool isEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a != b;
    }
    private  void checkNumber(Token operat, Expr operand , string where)
    {
        if (evaluate(operand) is int) return;
        throw new RuntimeError(operat, $"{where} must be a number.");
    }
    private  void checkString(Token operat, object operand , string where)
    {
        if (operand is string) return;
        throw new RuntimeError(operat, $"{where} must be a string.");
    }
    private void checkNumberOperands(Token operat, object left, object right)
    {
        if (left is int && right is int) return;

        throw new RuntimeError(operat, "Operands must be numbers.");
    }
    
      private object evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public void interpret(List<Stmt> statements)
    {
        try
        {
            while (line <= statements.Count)
            {
                execute(statements[line - 1]);    
                line++;
            }
        }
        catch (RuntimeError error)
        {
            PixelWallE.runtimeError(error);
        }
    }
    private void execute(Stmt stmt)
    {
        stmt.Accept(this);
    }
    //  private string stringify(object obj)
    // {
    //     if (obj == null) return "nil";
    //     if (obj is int)
    //     {
    //         string text = obj.ToString();
    //         if (text.EndsWith(".0"))
    //         {
    //             text = text.Substring(0, text.Length - 2);
    //         }
    //         return text;
    //     }
    //     return obj.ToString();
    // }
}