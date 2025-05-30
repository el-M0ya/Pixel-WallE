namespace PixelWallEInterpreter;

public class Interpreter : IVisitor<object>
{
    public object visitBinary(Expr.Binary expr)
    {
        object left = evaluate(expr.left);
        object right = evaluate(expr.right);
        switch (expr.operat.type)
        {
            case TokenType.PLUS:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }
                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }
                throw new RuntimeError(expr.operat, "Operands must be two numbers or two strings.");

            case TokenType.MINUS:
                checkNumberOperands(expr.operat, left, right);
                return (double)left - (double)right;

            case TokenType.SLASH:
                checkNumberOperands(expr.operat, left, right);
                return (double)left / (double)right;

            case TokenType.STAR:
                checkNumberOperands(expr.operat, left, right);
                return (double)left * (double)right;

            case TokenType.GREATER:
                checkNumberOperands(expr.operat, left, right);
                return (double)left > (double)right;


            case TokenType.GREATER_EQUAL:
                checkNumberOperands(expr.operat, left, right);
                return (double)left >= (double)right;

            case TokenType.LESS:
                checkNumberOperands(expr.operat, left, right);
                return (double)left < (double)right;

            case TokenType.LESS_EQUAL:
                checkNumberOperands(expr.operat, left, right);
                return (double)left <= (double)right;

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
                return -(double)right;
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
    private object evaluate(Expr expr)
    {
        return expr.Accept(this);
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

    private static void checkNumberOperand(Token operat, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(operat, "Operand must be a number.");
    }
    private void checkNumberOperands(Token operat, object left, object right)
    {
        if (left is double && right is double) return;

        throw new RuntimeError(operat, "Operands must be numbers.");
    }

    public void interpret(Expr expression)
    {
        try
        {
            object value = evaluate(expression);
            Console.WriteLine(stringify(value));
        }
        catch (RuntimeError error)
        {
            PixelWallE.runtimeError(error);
        }
    }
     private string stringify(object obj)
    {
        if (obj == null) return "nil";
        if (obj is double) {
        string text = obj.ToString();
        if (text.EndsWith(".0")) {
        text = text.Substring(0, text.Length - 2);
        }
        return text;
        }
        return obj.ToString();
    }
}