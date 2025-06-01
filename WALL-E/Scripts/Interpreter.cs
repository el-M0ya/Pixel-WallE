namespace PixelWallEInterpreter;

public class Interpreter : Expr.IVisitor<object> , Stmt.IVisitor
{
    private Environment environment = new Environment();
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

    public object visitVar(Expr.Var expr)
    {
        return environment.get(expr.name);
    }
    private object evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public void visitExprStmt(Stmt.Expression stmt)
    {
        evaluate(stmt.expression);
    }
    public void visitPrintStmt(Stmt.Print stmt)
    {
        object value = evaluate(stmt.expression);
        Console.WriteLine(stringify(value));
    }
    public void visitVarStmt(Stmt.Var stmt)
    {
        object value = null;
        if (stmt.initializer != null)  value = evaluate(stmt.initializer);
        environment.define(stmt.name.lexeme, value);
        
    }
    public object visitAssign(Expr.Assign expr)
    {
        object value = evaluate(expr.value);
        environment.assign(expr.name, value);
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
    // private static void checkNumberOperand(Token operat, object operand)
    // {
    //     if (operand is int) return;
    //     throw new RuntimeError(operat, "Operand must be a number.");
    // }
    private void checkNumberOperands(Token operat, object left, object right)
    {
        if (left is int && right is int) return;

        throw new RuntimeError(operat, "Operands must be numbers.");
    }

    public void interpret(List<Stmt> statements)
    {
        try
        {
         foreach (Stmt statement in statements)
            {
                execute(statement);
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
        if (obj == null) return "nil";
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