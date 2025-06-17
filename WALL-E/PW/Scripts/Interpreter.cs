namespace PW;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                throw new RuntimeError(expr.operat , "Operands must be two numbers");

            case TokenType.MINUS:
                checkNumberOperands(expr.operat, left, right);
                return (int)left - (int)right;

            case TokenType.SLASH:
                checkNumberOperands(expr.operat, left, right);
                if ((int)right == 0) throw new RuntimeError(expr.operat , "Division by 0 is not defined");
                return (int)left / (int)right;

            case TokenType.STAR:
                checkNumberOperands(expr.operat, left, right);
                return (int)left * (int)right;

            case TokenType.DOUBLE_STAR:
                checkNumberOperands(expr.operat, left, right);
                return (int)Math.Pow((int)left , (int)right);

            case TokenType.PERCENT:
                checkNumberOperands(expr.operat, left, right);
                return (int)left % (int)right;

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
        checkString(expr.name , expr.color, "GetColorCount.color");
        checkNumber(expr.name ,expr.x1, "GetColorCount.x1");
        checkNumber(expr.name ,expr.y1, "GetColorCount.y1");
        checkNumber(expr.name ,expr.x2, "GetColorCount.x2");
        checkNumber(expr.name ,expr.y2, "GetColorCount.y2");
        return MainWindow.GetColorCount(stringify(evaluate(expr.color)),
                                        (int)evaluate(expr.x1), (int)evaluate(expr.y1),
                                        (int)evaluate(expr.x2), (int)evaluate(expr.y2));
    }
    public object visitIsBrushSize(Expr.IsBrushSize expr)
    {
        checkNumber(expr.name ,expr.size, "IsBrushSize.size");
        return MainWindow.IsBrushSize((int)evaluate(expr.size));
    }
    public object visitIsBrushColor(Expr.IsBrushColor expr)
    {
        checkString(expr.name ,expr.color, "IsBrushColor.color");
        return MainWindow.IsBrushColor(stringify(evaluate(expr.color)));
    }
    public object visitIsCanvasColor(Expr.IsCanvasColor expr)
    {

        checkString(expr.name ,expr.color, "IsCanvasColor.color");
        checkNumber(expr.name ,expr.x, "IsCanvasColor.x");
        checkNumber(expr.name ,expr.y, "IsCanvasColor.y");

        return MainWindow.IsCanvasColor(stringify(evaluate(expr.color)),
                                        (int)evaluate(expr.x), (int)evaluate(expr.y));
    }


    public void visitExprStmt(Stmt.Expression stmt)
    {
        evaluate(stmt.expression);
    }

    public void visitSpawnStmt(Stmt.Spawn spawn)
    {
        if (Wall_E.Instance.isSpawn)
        {
            throw new RuntimeError(spawn.name , "Mas de un Spawn");
        }
        checkNumber(spawn.name ,spawn.x, "Spawn.x");
        checkNumber(spawn.name ,spawn.y, "Spawn.y");
        int x = (int)evaluate(spawn.x);
        int y = (int)evaluate(spawn.y);
        Wall_E.Instance.isSpawn = true;
        MainWindow.Spawn(x, y);
    }
    public void visitColorStmt(Stmt.Color color)
    {
        checkString(color.name , color.color , "Color.color");
        string newcolor = stringify(evaluate(color.color));
        if (MainWindow._colorNameMap.ContainsKey(newcolor)) MainWindow.Color(newcolor);
        else
            throw new RuntimeError(color.name, $"Invalid color: {newcolor}");

    }
    public void visitSizeStmt(Stmt.Size size)
    {
        checkNumber(size.name , size.size, "Size.size");
        int newsize = (int)evaluate(size.size);
        MainWindow.Size(newsize);
    }
    public async Task visitDrawLineStmt(Stmt.DrawLine drawLine)
    {
        checkNumber(drawLine.name ,drawLine.dirX, "DrawLine.dirX");
        checkNumber(drawLine.name ,drawLine.dirY, "DrawLine.dirY");
        checkNumber(drawLine.name ,drawLine.distance, "DrawLine.distance");
        int x = (int)evaluate(drawLine.dirX);
        int y = (int)evaluate(drawLine.dirY);
        if (x < -1 || x > 1 || y < -1 || y > 1) throw new RuntimeError(drawLine.name , "Parameters out of range: directions are between -1 and 1");
        int distance = (int)evaluate(drawLine.distance);
        if (distance < 0)
        {
            x = -x;
            y = -y;
            distance = -distance;
        }
        await MainWindow.DrawLine(x , y , distance);
    }
    public async Task visitDrawCircleStmt(Stmt.DrawCircle drawCircle)
    {
        checkNumber(drawCircle.name ,drawCircle.dirX, "DrawCircle.dirX");
        checkNumber(drawCircle.name ,drawCircle.dirY, "DrawCircle.dirY");
        checkNumber(drawCircle.name ,drawCircle.radius, "DrawCircle.radius");
        int x = (int)evaluate(drawCircle.dirX);
        int y = (int)evaluate(drawCircle.dirY);
         if (x < -1 || x > 1 || y < -1 || y > 1) throw new RuntimeError(drawCircle.name , "Parameters out of range: directions are between -1 and 1");
        int radius = (int)evaluate(drawCircle.radius);
        if (radius < 0) radius = -radius;
       await MainWindow.DrawCircle(x , y , radius);
    }
    public async Task visitDrawRectangleStmt(Stmt.DrawRectangle drawRectangle)
    {
        checkNumber(drawRectangle.name ,drawRectangle.dirX, "DrawRectangle.dirX");
        checkNumber(drawRectangle.name ,drawRectangle.dirY, "DrawRectangle.dirY");
        checkNumber(drawRectangle.name ,drawRectangle.distance, "DrawRectangle.distance");
        checkNumber(drawRectangle.name ,drawRectangle.width, "DrawRectangle.width");
        checkNumber(drawRectangle.name ,drawRectangle.height, "DrawRectangle.height");
        
        int x = (int)evaluate(drawRectangle.dirX);
        int y = (int)evaluate(drawRectangle.dirY);
        if (x < -1 || x > 1 || y < -1 || y > 1) throw new RuntimeError(drawRectangle.name , "Parameters out of range: directions are between -1 and 1");
        int distance = (int)evaluate(drawRectangle.distance);
        if (distance < 0)
        {
            x = -x;
            y = -y;
            distance = -distance;
        }
        int width = (int)evaluate(drawRectangle.width);
        if (width < 0) width = -width;
        int height = (int)evaluate(drawRectangle.height);
        if (height < 0) height = -height;
        
         await MainWindow.DrawRectangle(x , y , distance , width , height);
    }
    public async Task visitFillStmt(Stmt.Fill fill)
    {
        await MainWindow.Fill();
    }
    public void visitGoToStmt(Stmt.GoTo stmt)
    {
        object condition = evaluate(stmt.condition);

        if (isTruth(condition))
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

        return a == b;
    }
    private  void checkNumber(Token operat , Expr operand , string where)
    {
        if (evaluate(operand) is int) return;
        throw new RuntimeError(operat , $"{where} must be a number.");
    }
    private void checkString(Token operat , Expr operand , string where)
    {
        if (evaluate(operand) is string) return;
        throw new RuntimeError(operat , $"{where} must be a string.");
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

    public async Task interpret(List<Stmt> statements)
    {
        environment.Reset(); 
        line = 1;
        // PRIMERA PASADA: Registrar todas las etiquetas
        for (int i = 0; i < statements.Count; i++)
        {
            if (statements[i] is Stmt.Label labelStmt)
            {
                environment.assignLabel(labelStmt.label);
            }
        }

        // SEGUNDA PASADA: Ejecución
        int count = 0;
        line = 1;
        while (line <= statements.Count)
        {
            try
            {
                if (count > 1000) throw new RuntimeError(new Token(TokenType.GOTO, null, null, 0), "Stack overflow");

                Stmt stmt = statements[line - 1];
                if (stmt is Stmt.DrawLine ||
                     stmt is Stmt.DrawCircle ||
                     stmt is Stmt.DrawRectangle ||
                     stmt is Stmt.Fill)await executeAsync(stmt);
                else
                    execute(stmt);
                count++;
                line++;
            }

            catch (RuntimeError error)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainWindow.SetStatus(error.Message, true);
                if (error is RuntimeError runtimeError)
                    PixelWallE.runtimeError(runtimeError);
            });
            break;
            }
        }
    }
    private void execute(Stmt stmt)
    {
        stmt.Accept(this);
    }
    private async Task executeAsync(Stmt stmt)
    {
        if (stmt is Stmt.DrawLine drawLine)
    {
        await visitDrawLineStmt(drawLine);
    }
    else if (stmt is Stmt.DrawCircle drawCircle)
    {
        await visitDrawCircleStmt(drawCircle);
    }
    else if (stmt is Stmt.DrawRectangle drawRectangle)
    {
        await visitDrawRectangleStmt(drawRectangle);
    }
    else if (stmt is Stmt.Fill fill)
    {
        await visitFillStmt(fill);
    }
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