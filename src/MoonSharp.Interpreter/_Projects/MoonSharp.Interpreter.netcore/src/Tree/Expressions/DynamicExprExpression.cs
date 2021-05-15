﻿using System;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class DynamicExprExpression : Expression
	{
		Expression m_Exp;

		public DynamicExprExpression(Expression exp, ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			lcontext.Anonymous = true;
			m_Exp = exp;
		}

		public override bool EvalLiteral(out DynValue dv)
		{
			throw new InvalidOperationException();
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			return m_Exp.Eval(context);
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			throw new InvalidOperationException();
		}

		public override SymbolRef FindDynamic(ScriptExecutionContext context)
		{
			return m_Exp.FindDynamic(context);
		}
	}
}
