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
                throw new RuntimeError("Operands must be two numbers or two strings.");

            case TokenType.MINUS:
                checkNumberOperands(expr.operat, left, right);
                return (int)left - (int)right;

            case TokenType.SLASH:
                checkNumberOperands(expr.operat, left, right);
                if ((int)right == 0) throw new RuntimeError("Division by 0 is not defined");
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

    public object visitGetActualX(Expr.GetActualX expr)
    {
        return MainWindow.GetActualX();
    }
    public object visitGetActualY(Expr.GetActualY expr)
    {
        return MainWindow.GetActualY();
    }
    public object visitGetCanvasSize(Expr.GetCanvasSize expr)
    {
        return MainWindow.GetCanvasSize();
    }
    public object visitGetColorCount(Expr.GetColorCount expr)
    {
        checkString(expr.color, "GetColorCount.color");
        checkNumber(expr.x1, "GetColorCount.x1");
        checkNumber(expr.y1, "GetColorCount.y1");
        checkNumber(expr.x2, "GetColorCount.x2");
        checkNumber(expr.y2, "GetColorCount.y2");
        return MainWindow.GetColorCount(   stringify(evaluate(expr.color)),
                                        (int)evaluate(expr.x1) ,(int)evaluate( expr.y1),
                                        (int)evaluate(expr.x2) , (int)evaluate(expr.y2));
    }
    public object visitIsBrushSize(Expr.IsBrushSize expr)
    {
        checkNumber(expr.size, "IsBrushSize.size");
        return MainWindow.IsBrushSize((int)evaluate(expr.size));
    }
    public object visitIsBrushColor(Expr.IsBrushColor expr)
    {
        checkString(expr.color, "IsBrushColor.color");
        return MainWindow.IsBrushColor(stringify(evaluate(expr.color)));
    }
    public object visitIsCanvasColor(Expr.IsCanvasColor expr)
    {

        checkString(expr.color, "IsCanvasColor.color");
        checkNumber(expr.x, "IsCanvasColor.x");
        checkNumber(expr.y, "IsCanvasColor.y");

        return MainWindow.IsCanvasColor(stringify(evaluate(expr.color)),
                                        (int)evaluate(expr.x), (int)evaluate(expr.y));
    }

    public void visitExprStmt(Stmt.Expression stmt)
    {
        evaluate(stmt.expression);
    }

    public void visitSpawnStmt(Stmt.Spawn spawn)
    {
        checkNumber(spawn.x, "Spawn.x");
        checkNumber(spawn.y, "Spawn.y");
        int x = (int)evaluate(spawn.x);
        int y = (int)evaluate(spawn.y);
        MainWindow.Spawn(x, y);
        MainWindow.SetStatus("atudddd", true);
    }
    public void visitColorStmt(Stmt.Color color)
    {
        checkString(color.color , "Color");
        string newcolor = stringify(color);
        MainWindow.Color(newcolor);
    }
    public void visitSizeStmt(Stmt.Size size)
    {
        checkNumber(size.size, "Size.size");
        int newsize = (int)evaluate(size.size);
        MainWindow.Size(newsize);
    }
    public void visitDrawLineStmt(Stmt.DrawLine drawLine)
    {
        checkNumber(drawLine.dirX, "DrawLine.dirX");
        checkNumber(drawLine.dirY, "DrawLine.dirY");
        checkNumber(drawLine.distance, "DrawLine.distance");
        int x = (int)evaluate(drawLine.dirX);
        int y = (int)evaluate(drawLine.dirY);
        int distance = (int)evaluate(drawLine.distance);
        MainWindow.DrawLine(x , y , distance);
    }
    public void visitDrawCircleStmt(Stmt.DrawCircle drawCircle)
    {
        int x = (int)evaluate(drawCircle.dirX);
        int y = (int)evaluate(drawCircle.dirY);
        int radius = (int)evaluate(drawCircle.radius);
        MainWindow.DrawLine(x , y , radius);
    }
    public void visitDrawRectangleStmt(Stmt.DrawRectangle drawRectangle)
    {
        int x = (int)evaluate(drawRectangle.dirX);
        int y = (int)evaluate(drawRectangle.dirY);
        int distance = (int)evaluate(drawRectangle.distance);
        int width = (int)evaluate(drawRectangle.width);
        int height = (int)evaluate(drawRectangle.height);

        MainWindow.DrawRectangle(x , y , distance , width , height);
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
    private  void checkNumber(Expr operand , string where)
    {
        if (evaluate(operand) is int) return;
        throw new RuntimeError($"{where} must be a number.");
    }
    private  void checkString(object operand , string where)
    {
        if (operand is string) return;
        throw new RuntimeError($"{where} must be a string.");
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
     private string stringify(object obj)
    {
        if (obj == null) return "null";
        if (obj is int)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }
        return obj.ToString();
    }
}