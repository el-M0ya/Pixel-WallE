// using System.Text;

// namespace PixelWallEInterpreter;

// public class AstPrinter : Expr.IVisitor<string>
// {
//     public string print(Expr expr)
//     {
//         return expr.Accept(this);
//     }
//     public string visitBinary(Expr.Binary expr)
//     {
//         return parenthesize(expr.operat.lexeme, [expr.left, expr.right]);
//     }
//     public string visitUnary(Expr.Unary expr)
//     {
//         return parenthesize(expr.operat.lexeme, [expr.right]);
//     }
//     public string visitGrouping(Expr.Grouping expr)
//     {
//         return parenthesize("group", [expr.expression]);
//     }
//     public string visitLiterals(Expr.Literal expr)
//     {
//         if (expr.value == null) return "null";
//         return expr.value.ToString();
//     }

//     private string parenthesize(string name, Expr[] exprs)
//     {
//         StringBuilder builder = new StringBuilder();
//         builder.Append("(").Append(name);
//         foreach (Expr expr in exprs)
//         {
//             builder.Append(" ");
//             builder.Append(expr.Accept(this));
//         }
//         builder.Append(")");
//         return builder.ToString();
//     }
   
     
// }