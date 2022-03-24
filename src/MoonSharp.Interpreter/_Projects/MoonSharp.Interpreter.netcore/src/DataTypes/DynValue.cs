﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// A class representing a value in a Lua/MoonSharp script.
	/// </summary>
	public struct DynValue
	{
		private double m_Number;
		private object m_Object;
		private DataType m_Type;

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public DataType Type { get { return m_Type; } }
		/// <summary>
		/// Gets the function (valid only if the <see cref="Type"/> is <see cref="DataType.Function"/>)
		/// </summary>
		public Closure Function { get { return m_Object as Closure; } }
		/// <summary>
		/// Gets the numeric value (valid only if the <see cref="Type"/> is <see cref="DataType.Number"/>)
		/// </summary>
		public double Number { get { return m_Number; } }
		/// <summary>
		/// Gets the values in the tuple (valid only if the <see cref="Type"/> is Tuple).
		/// This field is currently also used to hold arguments in values whose <see cref="Type"/> is <see cref="DataType.TailCallRequest"/>.
		/// </summary>
		public DynValue[] Tuple { get { return m_Object as DynValue[]; } }
		/// <summary>
		/// Gets the coroutine handle. (valid only if the <see cref="Type"/> is Thread).
		/// </summary>
		public Coroutine Coroutine { get { return m_Object as Coroutine; } }
		/// <summary>
		/// Gets the table (valid only if the <see cref="Type"/> is <see cref="DataType.Table"/>)
		/// </summary>
		public Table Table { get { return m_Object as Table; } }
		/// <summary>
		/// Gets the boolean value (valid only if the <see cref="Type"/> is <see cref="DataType.Boolean"/>)
		/// </summary>
		public bool Boolean { get { return Number != 0; } }
		/// <summary>
		/// Gets the string value (valid only if the <see cref="Type"/> is <see cref="DataType.String"/>)
		/// </summary>
		public string String { get { return m_Object as string; } }
		/// <summary>
		/// Gets the CLR callback (valid only if the <see cref="Type"/> is <see cref="DataType.ClrFunction"/>)
		/// </summary>
		public CallbackFunction Callback { get { return m_Object as CallbackFunction; } }
		/// <summary>
		/// Gets the tail call data.
		/// </summary>
		public TailCallData TailCallData { get { return m_Object as TailCallData; } }
		/// <summary>
		/// Gets the yield request data.
		/// </summary>
		public YieldRequest YieldRequest { get { return m_Object as YieldRequest; } }
		/// <summary>
		/// Gets the tail call data.
		/// </summary>
		public UserData UserData { get { return m_Object as UserData; } }
		public int Int { get { return (int)m_Number; } }

		/// <summary>
		/// Creates a new writable value initialized to the specified boolean.
		/// </summary>
		public static DynValue NewBoolean(bool v)
		{
			return new DynValue()
			{
				m_Number = v ? 1 : 0,
				m_Type = DataType.Boolean,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified number.
		/// </summary>
		public static DynValue NewNumber(double num)
		{
			return new DynValue()
			{
				m_Number = num,
				m_Type = DataType.Number,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified string.
		/// </summary>
		public static DynValue NewString(string str)
		{
			return new DynValue()
			{
				m_Object = str,
				m_Type = DataType.String,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified StringBuilder.
		/// </summary>
		public static DynValue NewString(StringBuilder sb)
		{
			return new DynValue()
			{
				m_Object = sb.ToString(),
				m_Type = DataType.String,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified string using String.Format like syntax
		/// </summary>
		public static DynValue NewString(string format, params object[] args)
		{
			return new DynValue()
			{
				m_Object = string.Format(format, args),
				m_Type = DataType.String,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified coroutine.
		/// Internal use only, for external use, see Script.CoroutineCreate
		/// </summary>
		/// <param name="coroutine">The coroutine object.</param>
		/// <returns></returns>
		public static DynValue NewCoroutine(Coroutine coroutine)
		{
			return new DynValue()
			{
				m_Object = coroutine,
				m_Type = DataType.Thread
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified closure (function).
		/// </summary>
		public static DynValue NewClosure(Closure function)
		{
			return new DynValue()
			{
				m_Object = function,
				m_Type = DataType.Function,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified CLR callback.
		/// </summary>
		public static DynValue NewCallback(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack, string name = null)
		{
			return new DynValue()
			{
				m_Object = new CallbackFunction(callBack, name),
				m_Type = DataType.ClrFunction,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified CLR callback.
		/// See also CallbackFunction.FromDelegate and CallbackFunction.FromMethodInfo factory methods.
		/// </summary>
		public static DynValue NewCallback(CallbackFunction function)
		{
			return new DynValue()
			{
				m_Object = function,
				m_Type = DataType.ClrFunction,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified table.
		/// </summary>
		public static DynValue NewTable(Table table)
		{
			return new DynValue()
			{
				m_Object = table,
				m_Type = DataType.Table,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to an empty prime table (a 
		/// prime table is a table made only of numbers, strings, booleans and other
		/// prime tables).
		/// </summary>
		public static DynValue NewPrimeTable()
		{
			return NewTable(new Table(null));
		}

		/// <summary>
		/// Creates a new writable value initialized to an empty table.
		/// </summary>
		public static DynValue NewTable(Script script)
		{
			return NewTable(new Table(script));
		}

		/// <summary>
		/// Creates a new writable value initialized to with array contents.
		/// </summary>
		public static DynValue NewTable(Script script, params DynValue[] arrayValues)
		{
			return NewTable(new Table(script, arrayValues));
		}

		/// <summary>
		/// Creates a new request for a tail call. This is the preferred way to execute Lua/MoonSharp code from a callback,
		/// although it's not always possible to use it. When a function (callback or script closure) returns a
		/// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
		/// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
		/// of functionality (state savings, coroutines, etc) keeps working at full power.
		/// </summary>
		/// <param name="tailFn">The function to be called.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public static DynValue NewTailCallReq(DynValue tailFn, params DynValue[] args)
		{
			return new DynValue()
			{
				m_Object = new TailCallData()
				{
					Args = args,
					Function = tailFn,
				},
				m_Type = DataType.TailCallRequest,
			};
		}

		/// <summary>
		/// Creates a new request for a tail call. This is the preferred way to execute Lua/MoonSharp code from a callback,
		/// although it's not always possible to use it. When a function (callback or script closure) returns a
		/// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
		/// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
		/// of functionality (state savings, coroutines, etc) keeps working at full power.
		/// </summary>
		/// <param name="tailCallData">The data for the tail call.</param>
		/// <returns></returns>
		public static DynValue NewTailCallReq(TailCallData tailCallData)
		{
			return new DynValue()
			{
				m_Object = tailCallData,
				m_Type = DataType.TailCallRequest,
			};
		}



		/// <summary>
		/// Creates a new request for a yield of the current coroutine.
		/// </summary>
		/// <param name="args">The yield argumenst.</param>
		/// <returns></returns>
		public static DynValue NewYieldReq(DynValue[] args)
		{
			return new DynValue()
			{
				m_Object = new YieldRequest() { ReturnValues = args },
				m_Type = DataType.YieldRequest,
			};
		}

		/// <summary>
		/// Creates a new request for a yield of the current coroutine.
		/// </summary>
		/// <param name="args">The yield argumenst.</param>
		/// <returns></returns>
		internal static DynValue NewForcedYieldReq()
		{
			return new DynValue()
			{
				m_Object = new YieldRequest() { Forced = true },
				m_Type = DataType.YieldRequest,
			};
		}

		/// <summary>
		/// Creates a new tuple initialized to the specified values.
		/// </summary>
		public static DynValue NewTuple(params DynValue[] values)
		{
			if (values.Length == 0)
				return DynValue.Nil;

			if (values.Length == 1)
				return values[0];

			return new DynValue()
			{
				m_Object = values,
				m_Type = DataType.Tuple,
			};
		}

		/// <summary>
		/// Creates a new tuple initialized to the specified values - which can be potentially other tuples
		/// </summary>
		public static DynValue NewTupleNested(params DynValue[] values)
		{
			if (!values.Any(v => v.Type == DataType.Tuple))
				return NewTuple(values);

			if (values.Length == 1)
				return values[0];

			List<DynValue> vals = new List<DynValue>();

			foreach (var v in values)
			{
				if (v.Type == DataType.Tuple)
					vals.AddRange(v.Tuple);
				else
					vals.Add(v);
			}

			return new DynValue()
			{
				m_Object = vals.ToArray(),
				m_Type = DataType.Tuple,
			};
		}


		/// <summary>
		/// Creates a new userdata value
		/// </summary>
		public static DynValue NewUserData(UserData userData)
		{
			return new DynValue()
			{
				m_Object = userData,
				m_Type = DataType.UserData,
			};
		}


		/// <summary>
		/// A preinitialized, readonly instance, equaling Void
		/// </summary>
		public static DynValue Void { get; private set; }
		/// <summary>
		/// A preinitialized, readonly instance, equaling Nil
		/// </summary>
		public static DynValue Nil { get; private set; }
		/// <summary>
		/// A preinitialized, readonly instance, equaling True
		/// </summary>
		public static DynValue True { get; private set; }
		/// <summary>
		/// A preinitialized, readonly instance, equaling False
		/// </summary>
		public static DynValue False { get; private set; }


		static DynValue()
		{
			Nil = new DynValue() { m_Type = DataType.Nil };
			Void = new DynValue() { m_Type = DataType.Void };
			True = DynValue.NewBoolean(true);
			False = DynValue.NewBoolean(false);
		}


		/// <summary>
		/// Returns a string which is what it's expected to be output by the print function applied to this value.
		/// </summary>
		public string ToPrintString()
		{
			if (this.m_Object != null && this.m_Object is RefIdObject)
			{
				RefIdObject refid = (RefIdObject)m_Object;

				string typeString = this.Type.ToLuaTypeString();

				if (m_Object is UserData)
				{
					UserData ud = (UserData)m_Object;
					string str = ud.Descriptor.AsString(ud.Object);
					if (str != null)
						return str;
				}

				return refid.FormatTypeString(typeString);
			}

			switch (Type)
			{
				case DataType.String:
					return String;
				case DataType.Tuple:
					return string.Join("\t", Tuple.Select(t => t.ToPrintString()).ToArray());
				case DataType.TailCallRequest:
					return "(TailCallRequest -- INTERNAL!)";
				case DataType.YieldRequest:
					return "(YieldRequest -- INTERNAL!)";
				default:
					return ToString();
			}
		}

		/// <summary>
		/// Returns a string which is what it's expected to be output by debuggers.
		/// </summary>
		public string ToDebugPrintString()
		{
			if (this.m_Object != null && this.m_Object is RefIdObject)
			{
				RefIdObject refid = (RefIdObject)m_Object;

				string typeString = this.Type.ToLuaTypeString();

				if (m_Object is UserData)
				{
					UserData ud = (UserData)m_Object;
					string str = ud.Descriptor.AsString(ud.Object);
					if (str != null)
						return str;
				}

				return refid.FormatTypeString(typeString);
			}

			switch (Type)
			{
				case DataType.Tuple:
					return string.Join("\t", Tuple.Select(t => t.ToPrintString()).ToArray());
				case DataType.TailCallRequest:
					return "(TailCallRequest)";
				case DataType.YieldRequest:
					return "(YieldRequest)";
				default:
					return ToString();
			}
		}


		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			switch (Type)
			{
				case DataType.Void:
					return "void";
				case DataType.Nil:
					return "nil";
				case DataType.Boolean:
					return Boolean.ToString().ToLower();
				case DataType.Number:
					return Number.ToString(CultureInfo.InvariantCulture);
				case DataType.String:
					return "\"" + String + "\"";
				case DataType.Function:
					return string.Format("(Function {0:X8})", Function.EntryPointByteCodeLocation);
				case DataType.ClrFunction:
					return string.Format("(Function CLR)", Function);
				case DataType.Table:
					return "(Table)";
				case DataType.Tuple:
					return string.Join(", ", Tuple.Select(t => t.ToString()).ToArray());
				case DataType.TailCallRequest:
					return "Tail:(" + string.Join(", ", Tuple.Select(t => t.ToString()).ToArray()) + ")";
				case DataType.UserData:
					return "(UserData)";
				case DataType.Thread:
					return string.Format("(Coroutine {0:X8})", this.Coroutine.ReferenceID);
				default:
					return "(???)";
			}
		}


		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			int baseValue = ((int)(Type)) << 27;
			switch (Type)
			{
				case DataType.Void:
				case DataType.Nil:
					return 0;
				case DataType.Boolean:
					return Boolean ? 1 : 2;
				case DataType.Number:
					return baseValue ^ Number.GetHashCode();
				default:
					if (m_Object == null) return 0;
					else return baseValue ^ m_Object.GetHashCode();
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is DynValue other)) return false;

			if ((other.Type == DataType.Nil && this.Type == DataType.Void)
				|| (other.Type == DataType.Void && this.Type == DataType.Nil))
				return true;

			if (other.Type != this.Type) return false;


			switch (Type)
			{
				case DataType.Void:
				case DataType.Nil:
					return true;
				case DataType.Boolean:
					return Boolean == other.Boolean;
				case DataType.Number:
					return Number == other.Number;
				case DataType.String:
					return String == other.String;
				case DataType.Function:
					return Function == other.Function;
				case DataType.ClrFunction:
					return Callback == other.Callback;
				case DataType.Table:
					return Table == other.Table;
				case DataType.Tuple:
				case DataType.TailCallRequest:
					if (Tuple.Length != other.Tuple.Length)
						return false;
					for (int i = 0; i < Tuple.Length; i++)
						if (!Equals(Tuple[i], other.Tuple[i]))
							return false;
					return true;
				case DataType.Thread:
					return Coroutine == other.Coroutine;
				case DataType.UserData:
					{
						UserData ud1 = this.UserData;
						UserData ud2 = other.UserData;

						if (ud1 == null || ud2 == null)
							return false;

						if (ud1.Descriptor != ud2.Descriptor)
							return false;

						if (ud1.Object == null && ud2.Object == null)
							return true;

						if (ud1.Object != null && ud2.Object != null)
							return ud1.Object.Equals(ud2.Object);

						return false;
					}
				default:
					return false;
			}
		}


		/// <summary>
		/// Casts this DynValue to string, using coercion if the type is number.
		/// </summary>
		/// <returns>The string representation, or null if not number, not string.</returns>
		public string CastToString()
		{
			ref DynValue rv = ref ScalarReference(ref this);
			if (rv.Type == DataType.Number)
			{
				return rv.Number.ToString();
			}
			else if (rv.Type == DataType.String)
			{
				return rv.String;
			}
			return null;
		}

		/// <summary>
		/// Casts this DynValue to a double, using coercion if the type is string.
		/// </summary>
		/// <returns>The string representation, or null if not number, not string or non-convertible-string.</returns>
		public double? CastToNumber()
		{
			ref DynValue rv = ref ScalarReference(ref this);
			if (rv.Type == DataType.Number)
			{
				return rv.Number;
			}
			else if (rv.Type == DataType.String)
			{
				if (ToNumber(rv.String, out var n))
				{
					return n;
				}
			}
			return null;
		}

		internal double AssertNumber(int stage)
		{
			if (!TryCastToNumber(out var num))
				throw ScriptRuntimeException.ConvertToNumberFailed(stage);
			return num;
		}

		public bool TryCastToNumber(out double d)
		{
			ref DynValue rv = ref ScalarReference(ref this);
			if (rv.Type == DataType.Number)
			{
				d = rv.Number;
				return true;
			}
			else if (rv.Type == DataType.String)
			{
				if (ToNumber(rv.String, out d))
				{
					return true;
				}
			}
			d = 0.0;
			return false;
		}

		public static bool ToNumber(string str, out double num)
		{
			//Validate characters
			num = 0.0;
			bool hex = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (char.IsWhiteSpace(str[i]) ||
					char.IsDigit(str[i]) ||
					(str[i] >= 'A' && str[i] <= 'F') ||
					(str[i] >= 'a' && str[i] <= 'f') ||
					str[i] == '-' ||
					str[i] == 'p' ||
					str[i] == 'P' ||
					str[i] == '+' ||
					str[i] == '.')
					continue;

				if (str[i] == 'x' || str[i] == 'X')
				{
					hex = true;
					continue;
				}

				return false;
			}

			//hex float
			if (hex)
			{
				if (ParseHexFloat(str, out num))
					return true;
			}
			else
			{
				if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
					return true;
			}
			return false;
		}

		static bool ParseHexFloat(string s, out double result)
		{
			bool negate = false;
			result = 0.0;
			s = s.Trim();
			if (s[0] == '+')
				s = s.Substring(1);
			if (s[0] == '-')
			{
				negate = true;
				s = s.Substring(1);
			}
			if ((s.Length < 3) || s[0] != '0' || char.ToUpperInvariant(s[1]) != 'X')
				return false;

			s = s.Substring(2);
			double value = 0.0;
			int dummy, exp = 0;

			s = LexerUtils.ReadHexProgressive(s, ref value, out dummy);

			if (s.Length > 0 && s[0] == '.')
			{
				s = s.Substring(1);
				s = LexerUtils.ReadHexProgressive(s, ref value, out exp);
			}

			exp *= -4;

			if (s.Length > 0 && char.ToUpper(s[0]) == 'P')
			{
				if (s.Length == 1)
					return false;
				s = s.Substring(s[1] == '+' ? 2 : 1);
				int exp1 = int.Parse(s, CultureInfo.InvariantCulture);
				if (exp1 < 0) return false; //can't add negative exponent
				exp += exp1;
				s = "";
			}

			if (s.Length > 0) return false;

			result = value * Math.Pow(2, exp);
			if (negate) result = -result;
			return true;
		}


		/// <summary>
		/// Casts this DynValue to a bool
		/// </summary>
		/// <returns>False if value is false or nil, true otherwise.</returns>
		public bool CastToBool()
		{
			ref DynValue rv = ref ScalarReference(ref this);
			if (rv.Type == DataType.Boolean)
				return rv.Boolean;
			else return (rv.Type != DataType.Nil && rv.Type != DataType.Void);
		}

		/// <summary>
		/// Returns this DynValue as an instance of <see cref="IScriptPrivateResource"/>, if possible,
		/// null otherwise
		/// </summary>
		/// <returns>False if value is false or nil, true otherwise.</returns>
		public IScriptPrivateResource GetAsPrivateResource()
		{
			return m_Object as IScriptPrivateResource;
		}


		/// <summary>
		/// Converts a tuple to a scalar value. If it's already a scalar value, this function returns "this".
		/// </summary>
		public DynValue ToScalar()
		{
			if (Type != DataType.Tuple)
				return this;

			if (Tuple.Length == 0)
				return DynValue.Void;

			return Tuple[0].ToScalar();
		}

		static internal ref DynValue ScalarReference(ref DynValue d)
		{
			if (d.Type != DataType.Tuple)
				return ref d;
			if (d.Tuple.Length == 0)
			{
				return ref d;
			}
			return ref ScalarReference(ref d.Tuple[0]);
		}



		/// <summary>
		/// Gets the length of a string or table value.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ScriptRuntimeException">Value is not a table or string.</exception>
		public DynValue GetLength()
		{
			if (this.Type == DataType.Table)
				return DynValue.NewNumber(this.Table.Length);
			if (this.Type == DataType.String)
				return DynValue.NewNumber(this.String.Length);

			throw new ScriptRuntimeException("Can't get length of type {0}", this.Type);
		}

		/// <summary>
		/// Determines whether this instance is nil or void
		/// </summary>
		public bool IsNil()
		{
			return this.Type == DataType.Nil || this.Type == DataType.Void;
		}

		/// <summary>
		/// Determines whether this instance is not nil or void
		/// </summary>
		public bool IsNotNil()
		{
			return this.Type != DataType.Nil && this.Type != DataType.Void;
		}

		/// <summary>
		/// Determines whether this instance is void
		/// </summary>
		public bool IsVoid()
		{
			return this.Type == DataType.Void;
		}

		/// <summary>
		/// Determines whether this instance is not void
		/// </summary>
		public bool IsNotVoid()
		{
			return this.Type != DataType.Void;
		}

		/// <summary>
		/// Determines whether is nil, void or NaN (and thus unsuitable for using as a table key).
		/// </summary>
		public bool IsNilOrNan()
		{
			return (this.Type == DataType.Nil) || (this.Type == DataType.Void) || (this.Type == DataType.Number && double.IsNaN(this.Number));
		}



		/// <summary>
		/// Creates a new DynValue from a CLR object
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static DynValue FromObject(Script script, object obj)
		{
			return MoonSharp.Interpreter.Interop.Converters.ClrToScriptConversions.ObjectToDynValue(script, obj);
		}

		/// <summary>
		/// Converts this MoonSharp DynValue to a CLR object.
		/// </summary>
		public object ToObject()
		{
			return MoonSharp.Interpreter.Interop.Converters.ScriptToClrConversions.DynValueToObject(this);
		}

		/// <summary>
		/// Converts this MoonSharp DynValue to a CLR object of the specified type.
		/// </summary>
		public object ToObject(Type desiredType)
		{
			//Contract.Requires(desiredType != null);
			return MoonSharp.Interpreter.Interop.Converters.ScriptToClrConversions.DynValueToObjectOfType(this, desiredType, null, false);
		}

		/// <summary>
		/// Converts this MoonSharp DynValue to a CLR object of the specified type.
		/// </summary>
		public T ToObject<T>()
		{
			T myObject = (T)ToObject(typeof(T));
			if (myObject == null)
			{
				return default(T);
			}

			return myObject;
		}

#if HASDYNAMIC
		/// <summary>
		/// Converts this MoonSharp DynValue to a CLR object, marked as dynamic
		/// </summary>
		public dynamic ToDynamic()
		{
			return MoonSharp.Interpreter.Interop.Converters.ScriptToClrConversions.DynValueToObject(this);
		}
#endif

		/// <summary>
		/// Checks the type of this value corresponds to the desired type. A propert ScriptRuntimeException is thrown
		/// if the value is not of the specified type or - considering the TypeValidationFlags - is not convertible
		/// to the specified type.
		/// </summary>
		/// <param name="funcName">Name of the function requesting the value, for error message purposes.</param>
		/// <param name="desiredType">The desired data type.</param>
		/// <param name="argNum">The argument number, for error message purposes.</param>
		/// <param name="flags">The TypeValidationFlags.</param>
		/// <returns></returns>
		/// <exception cref="ScriptRuntimeException">Thrown
		/// if the value is not of the specified type or - considering the TypeValidationFlags - is not convertible
		/// to the specified type.</exception>
		public DynValue CheckType(string funcName, DataType desiredType, int argNum = -1, TypeValidationFlags flags = TypeValidationFlags.Default)
		{
			if (this.Type == desiredType)
				return this;

			bool allowNil = ((int)(flags & TypeValidationFlags.AllowNil) != 0);

			if (allowNil && this.IsNil())
				return this;

			bool autoConvert = ((int)(flags & TypeValidationFlags.AutoConvert) != 0);

			if (autoConvert)
			{
				if (desiredType == DataType.Boolean)
					return DynValue.NewBoolean(this.CastToBool());

				if (desiredType == DataType.Number)
				{
					double? v = this.CastToNumber();
					if (v.HasValue)
						return DynValue.NewNumber(v.Value);
				}

				if (desiredType == DataType.String)
				{
					string v = this.CastToString();
					if (v != null)
						return DynValue.NewString(v);
				}
			}

			if (this.IsVoid())
				throw ScriptRuntimeException.BadArgumentNoValue(argNum, funcName, desiredType);

			throw ScriptRuntimeException.BadArgument(argNum, funcName, desiredType, this.Type, allowNil);
		}

		/// <summary>
		/// Checks if the type is a specific userdata type, and returns it or throws.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="funcName">Name of the function.</param>
		/// <param name="argNum">The argument number.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public T CheckUserDataType<T>(string funcName, int argNum = -1, TypeValidationFlags flags = TypeValidationFlags.Default)
		{
			DynValue v = this.CheckType(funcName, DataType.UserData, argNum, flags);
			bool allowNil = ((int)(flags & TypeValidationFlags.AllowNil) != 0);

			if (v.IsNil())
				return default(T);

			object o = v.UserData.Object;
			if (o != null && o is T)
				return (T)o;

			throw ScriptRuntimeException.BadArgumentUserData(argNum, funcName, typeof(T), o, allowNil);
		}

	}




}