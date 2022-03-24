﻿using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using System.Collections.Generic;

using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ForLoopStatement : Statement
	{
		//for' NAME '=' exp ',' exp (',' exp)? 'do' block 'end'
		RuntimeScopeBlock m_StackFrame;
		Statement m_InnerBlock;
		SymbolRef m_VarName;
		Expression m_Start, m_End, m_Step;
		SourceRef m_RefFor, m_RefEnd;
		TokenType stepOp = TokenType.Op_Add;

		public ForLoopStatement(ScriptLoadingContext lcontext, Token nameToken, Token forToken)
			: base(lcontext)
		{
			//	for Name ‘=’ exp ‘,’ exp [‘,’ exp] do block end | 

			// lexer already at the '=' ! [due to dispatching vs for-each]
			CheckTokenType(lcontext, TokenType.Op_Assignment);

			m_Start = Expression.Expr(lcontext);
			CheckTokenType(lcontext, TokenType.Comma, TokenType.SemiColon);

			if (CheckTokenTypeAndDiscardIfMatch(lcontext, TokenType.Name, nameToken.Text))
			{
				CheckTokenTypeAndDiscardIfNot(lcontext, TokenTypeUtils.operators);
			}
			m_End = Expression.Expr(lcontext);

			if (lcontext.Lexer.Current.Type == TokenType.Comma || lcontext.Lexer.Current.Type == TokenType.SemiColon)
			{
				lcontext.Lexer.Next();

				if (CheckTokenTypeAndDiscardIfMatch(lcontext, TokenType.Name, nameToken.Text))
				{
					Token stepToken = CheckTokenTypeAndDiscard(lcontext, TokenTypeUtils.operators);
					Token stepToken2 = CheckTokenTypeAndDiscardIfNot(lcontext, TokenTypeUtils.operators);

					if (stepToken != null && TokenTypeUtils.IsOpToken(stepToken.Type))
					{
						stepOp = stepToken.Type;

					}
				}

				m_Step = Expression.Expr(lcontext);
			}
			else
			{
				m_Step = new LiteralExpression(lcontext, DynValue.NewNumber(1));
			}

			lcontext.Scope.PushBlock();
			m_VarName = lcontext.Scope.DefineLocal(nameToken.Text);

			forToken.GetSourceRef(CheckTokenTypeAndDiscard(lcontext, TokenType.Brk_Close_Round));

			m_RefFor = forToken.GetSourceRef(CheckTokenType(lcontext, TokenType.Do));
			m_InnerBlock = new CompositeStatement(lcontext);
			m_RefEnd = CheckTokenType(lcontext, TokenType.End).GetSourceRef();
			m_StackFrame = lcontext.Scope.PopBlock();

			lcontext.Source.Refs.Add(m_RefFor);
			lcontext.Source.Refs.Add(m_RefEnd);
		}


		public override void Compile(ByteCode bc)
		{
			bc.PushSourceRef(m_RefFor);

			Loop L = new Loop()
			{
				Scope = m_StackFrame
			};

			bc.LoopTracker.Loops.Push(L);

			m_End.CompilePossibleLiteral(bc);
			m_Step.Compile(bc);
			m_Start.CompilePossibleLiteral(bc);

			int start = bc.GetJumpPointForNextInstruction();
			var jumpend = bc.Emit_Jump(OpCode.JFor, -1);
			bc.Emit_Enter(m_StackFrame);

			bc.Emit_Store(m_VarName, 0, 0);

			m_InnerBlock.Compile(bc);

			bc.PopSourceRef();
			bc.PushSourceRef(m_RefEnd);

			bc.Emit_Debug("..end");
			bc.Emit_Leave(m_StackFrame);
			bc.Emit_Incr(1);
			bc.Emit_Jump(OpCode.Jump, start);

			bc.LoopTracker.Loops.Pop();

			int exitpoint = bc.GetJumpPointForNextInstruction();

			foreach (int i in L.BreakJumps)
				bc.SetNumVal(i, exitpoint);

			bc.SetNumVal(jumpend, exitpoint);
			bc.Emit_Pop(3);

			bc.PopSourceRef();
		}

	}
}