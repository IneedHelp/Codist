﻿using System;
using System.Collections.Generic;
using System.Linq; // unnccessary code
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Codist.Fake
{
#if DEBUG
	interface IInterface // interface declaration
	{
		int Property { get; set; }
	}
	[Flags]
	enum MyEnum : ushort // enum declaration
	{
		None = 0, OK = 1, Happy = 1 << 1, Composite = OK | Happy, Unknown = 0xFFFF
	}
	[System.ComponentModel.Description("demo")]
	struct MyStruct // struct declaration
	{
		const short Constant = ushort.MaxValue ^ 0xF0F0; // const field
		const string ConstantString = "literal string"; // const string
		public readonly static DateTime StartDate = DateTime.Now; // static field
		private static int _static = (int)DateTime.Now.Ticks; // static instance field
		static readonly int _staticReadonly = Int32.MinValue; // static readonly field
		private readonly int _readonly; // readonly field
		private int _instance; // field
	}
	abstract class AbstractClass : IInterface
	{
		protected abstract int Property { get; set; }
		protected abstract int AbstractMethod();
		public virtual void VirtualMethod() { }
		int IInterface.Property { get; set; }
	}
	static class ExtensionClass // static class
	{
		public static void Log(this string text) { } // static method
	}
	class ConcreteClass : AbstractClass
	{
		delegate void Clone<T>(T text);
		event EventHandler<EventArgs> MyEvent;

		/// <summary>
		/// Creates a new instance of <see cref="ConcreteClass"/>.
		/// </summary>
		/// <param name="fieldId">The field.</param>
		/// <example><code><![CDATA[System.Console.WriteLine(Codist.Constants.NameOfMe);]]></code></example>
		public ConcreteClass(int fieldId) {
			const int A = 1; // local constant
			var localField = fieldId; // local field
			@"Multiline
text".Log(); // multiline string (string verbatim)
			$"Test page {fieldId} is initialized"
				.Log(); // calling extension method

			switch ((MyEnum)fieldId) {
				case MyEnum.None:
					break; // normal swtich break
				case MyEnum.OK:
					return; // control flow keyword
				default:
					throw new NotImplementedException(fieldId.ToString() + " is not supported"); // control flow keyword
			}
			for (int i = 0; i < 0XFF; i++) {
				if (i == localField) {
					continue; // control flow keyword
				}
				else if (i > A) {
					break; // control flow keyword
				}
				else if (i > 2) {
					goto END; // label
				}
				else {
					throw new InvalidOperationException();
				}
			}
			END: // label
			MyEvent += TestPage_MyEvent;
		}

		private void TestPage_MyEvent(object sender, EventArgs e) {
			var anonymous = new { // anonymous type
				sender, // property of anonymous type
				@event = e,
				time = DateTime.Now
			};
		}

		protected override int Property { get; set; }

		public void Method<TGeneric>() {
			// unnecessary code
			Codist.Fake.ExtensionClass.Log(typeof(TGeneric).Name); // extension method
			NativeMethods.Print(IntPtr.Zero); // extern method
			AbstractMethod(); // abstract method
			VirtualMethod(); // overridden method
			base.VirtualMethod(); // base virtual method
			MyEvent(this, EventArgs.Empty); // invoke event
		}

		protected override int AbstractMethod() { // overridden method
			throw new NotImplementedException();
		}

		public override void VirtualMethod() { // overridden method
			base.VirtualMethod();
		}

		static class NativeMethods
		{
			[DllImport("dummy.dll", EntryPoint = "DummyFunction", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
			public static extern void Print(IntPtr ptr);
		}
	}
#else
	// Excluded code here
	class Unused
	{
	}
#endif
}