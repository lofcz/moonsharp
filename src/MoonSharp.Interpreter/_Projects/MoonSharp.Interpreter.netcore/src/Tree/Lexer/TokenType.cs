
using System.Collections.Generic;

namespace MoonSharp.Interpreter.Tree
{
	public static class TokenTypeUtils
    {
		public static readonly List<TokenType> operators = new List<TokenType>() {
			TokenType.Op_Equal, 
			TokenType.Op_Assignment, 
			TokenType.Op_LessThan,
			TokenType.Op_LessThanEqual,
			TokenType.Op_GreaterThan,
			TokenType.Op_GreaterThanEqual,
			TokenType.Op_Add,
			TokenType.Op_MinusOrSub,
			TokenType.Op_Mul,
			TokenType.Op_Div,
			TokenType.Op_Mod,
			TokenType.Op_Pwr,
			TokenType.Op_Len,
		};

		public static bool IsOpToken(TokenType type)
        {
			return type == TokenType.Op_Add || type == TokenType.Op_MinusOrSub || type == TokenType.Op_Mul || type == TokenType.Op_Div || type == TokenType.Op_Mod || type == TokenType.Op_Pwr || type == TokenType.Op_Len;
        }
    }

	public enum TokenType
	{
		Eof,
		HashBang,
		Name,
		And,
		Break,
		Do,
		Else,
		ElseIf,
		End,
		False,
		For,
		Function,
		Lambda,
		Goto,
		If,
		In,
		Local,
		Nil,
		Not,
		Or,
		Repeat,
		Return,
		Then,
		True,
		Until,
		While,
		Op_Equal,
		Op_Assignment,
		Op_LessThan,
		Op_LessThanEqual,
		Op_GreaterThanEqual,
		Op_GreaterThan,
		Op_NotEqual,
		Op_Concat,
		VarArgs,
		Dot,
		Colon,
		DoubleColon,
		Comma,
		Brk_Close_Curly,
		Brk_Open_Curly,
		Brk_Close_Round,
		Brk_Open_Round,
		Brk_Close_Square,
		Brk_Open_Square,
		Op_Len,
		Op_Pwr,
		Op_Mod,
		Op_Div,
		Op_Mul,
		Op_MinusOrSub,
		Op_Add,
		Comment,

		String,
		String_Long,

		Number,
		Number_HexFloat,
		Number_Hex,
		SemiColon,
		Invalid,

		Brk_Open_Curly_Shared,
		Op_Dollar,
	}



}
