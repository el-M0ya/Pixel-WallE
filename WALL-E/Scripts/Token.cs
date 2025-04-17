namespace PixelWallEInterpreter;

public enum TokenType {
 // Single-character tokens.
 LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
 COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR, PERCENT,

 // One or two character tokens.
 ASIGNATION, DOUBLE_STAR,
 BANG, BANG_EQUAL,
  EQUAL_EQUAL,
 GREATER, GREATER_EQUAL,
 LESS, LESS_EQUAL, 
 AND , OR ,

 // Literals.
 IDENTIFIER, STRING, NUMBER, VAR , 
 // Keywords.
    SPAWN , COLOR , SIZE , DRAWLINE , DRAWCIRCLE , DRAWRECTANGLE , FILL , 
   GETACTUALX ,  GETACTUALY ,  GETCANVASIZE ,  GETCOLORCOUNT ,  ISBRUSHCOLOR , 
    ISBRUSHSIZE ,  ISCOLOR , 

    GOTO,
 
 EOF
}

public class Token
{
 TokenType type;
 string lexeme;
 object literal;
 int line; 
 public Token(TokenType type, string lexeme, object literal, int line) {
 this.type = type;
 this.lexeme = lexeme;
 this.literal = literal;
 this.line = line;
 }
 public string toString() {
 return type + " " + lexeme + " " + literal;
 }
}