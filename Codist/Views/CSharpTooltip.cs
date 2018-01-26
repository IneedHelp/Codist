﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Codist.Views
{
	sealed class CSharpQuickInfoSource : IQuickInfoSource
	{
		static Brush _InterfaceBrush, _ClassBrush, _StructBrush, _TextBrush, _NumberBrush, _EnumBrush, _KeywordBrush;
		readonly IEditorFormatMapService _FormatMapService;
		IEditorFormatMap _FormatMap;

		private bool _IsDisposed;
		CSharpQuickInfoSourceProvider _QuickInfoSourceProvider;
		SemanticModel _SemanticModel;
		ITextBuffer _TextBuffer;
		public CSharpQuickInfoSource(CSharpQuickInfoSourceProvider provider, ITextBuffer subjectBuffer, IEditorFormatMapService formatMapService) {
			_QuickInfoSourceProvider = provider;
			_TextBuffer = subjectBuffer;
			_FormatMapService = formatMapService;
			_TextBuffer.Changing += _TextBuffer_Changing;
		}

		public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan) {
			//if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None) {
			//	goto EXIT;
			//}

			// Map the trigger point down to our buffer.
			var subjectTriggerPoint = session.GetTriggerPoint(_TextBuffer.CurrentSnapshot);
			if (!subjectTriggerPoint.HasValue) {
				goto EXIT;
			}

			var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
			var workspace = currentSnapshot.TextBuffer.GetWorkspace();
			if (workspace == null) {
				goto EXIT;
			}

			var querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
			var semanticModel = _SemanticModel;
			if (semanticModel == null) {
				_SemanticModel = semanticModel = workspace.GetDocument(querySpan).GetSemanticModelAsync().Result;
			}
			var unitCompilation = semanticModel.SyntaxTree.GetCompilationUnitRoot();

			//look for occurrences of our QuickInfo words in the span
			var navigator = _QuickInfoSourceProvider.NavigatorService.GetTextStructureNavigator(_TextBuffer);
			var extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
			var node = unitCompilation.FindNode(new Microsoft.CodeAnalysis.Text.TextSpan(extent.Span.Start, extent.Span.Length), true);
			if (node == null) {
				goto EXIT;
			}
			node = node.Kind() == SyntaxKind.Argument ? (node as ArgumentSyntax).Expression : node;
			var symbol = GetSymbol(node, semanticModel);
			if (symbol == null) {
				UIElement infoBox = null;
				if (Config.Instance.ShowNumericQuickInfo && node.Kind() == SyntaxKind.NumericLiteralExpression) {
					infoBox = ShowNumericForm(node);
				}
				else if (Config.Instance.ShowStringQuickInfo) {
					var token = node.GetFirstToken();
					if (token.Kind() == SyntaxKind.StringLiteralToken) {
						infoBox = ShowStringInfo(token.ValueText);
					}
				}
				if (infoBox != null) {
					qiContent.Add(infoBox);
					applicableToSpan = currentSnapshot.CreateTrackingSpan(extent.Span.Start, extent.Span.Length, SpanTrackingMode.EdgeInclusive);
					return;
				}
				goto EXIT;
			}

			var formatMap = _FormatMapService.GetEditorFormatMap(session.TextView);
			if (_FormatMap != formatMap) {
				_FormatMap = formatMap;
				UpdateSyntaxHighlights(formatMap);
			}

			if (Config.Instance.ShowAttributesQuickInfo) {
				var attrs = symbol.GetAttributes();
				if (attrs.Length > 0) {
					qiContent.Add(ShowAttributes(attrs, node.SpanStart));
				}
			}
			var typeSymbol = symbol as INamedTypeSymbol;
			if (typeSymbol != null) {
				if (Config.Instance.ShowBaseTypeQuickInfo) {
					ShowBaseType(qiContent, typeSymbol, node.SpanStart);
				}
				if (Config.Instance.ShowInterfacesQuickInfo) {
					ShowInterfaces(qiContent, typeSymbol, node.SpanStart);
				}
				goto RETURN;
			}
			var method = symbol as IMethodSymbol;
			if (method != null && Config.Instance.ShowExtensionMethodQuickInfo && method.IsExtensionMethod) {
				ShowExtensionMethod(qiContent, method, node.SpanStart);
				goto RETURN;
			}
			var field = symbol as IFieldSymbol;
			if (field != null && field.HasConstantValue) {
				var sv = field.ConstantValue as string;
				if (sv != null) {
					if (Config.Instance.ShowStringQuickInfo) {
						qiContent.Add(ShowStringInfo(sv));
					}
				}
				else if (Config.Instance.ShowNumericQuickInfo) {
					var s = ShowNumericForms(field.ConstantValue, NumericForm.None);
					if (s != null) {
						qiContent.Add(s);
						ShowEnumValueUnderlyingType(qiContent, node, symbol);
					}
				}
			}
			RETURN:
			applicableToSpan = currentSnapshot.CreateTrackingSpan(extent.Span.Start, extent.Span.Length, SpanTrackingMode.EdgeInclusive);
			return;
			EXIT:
			applicableToSpan = null;
		}

		private static StackPanel ShowStringInfo(string sv) {
			return new StackPanel()
				.Add(new StackPanel().MakeHorizontal().AddReadOnlyTextBox(sv.Length.ToString()).AddText("chars", true))
				//.Add(new StackPanel().MakeHorizontal().AddReadOnlyNumericTextBox(System.Text.Encoding.UTF8.GetByteCount(sv).ToString()).AddText("UTF-8 bytes", true))
				//.Add(new StackPanel().MakeHorizontal().AddReadOnlyNumericTextBox(System.Text.Encoding.Default.GetByteCount(sv).ToString()).AddText("System bytes", true))
				.Add(new StackPanel().MakeHorizontal().AddReadOnlyTextBox(sv.GetHashCode().ToString()).AddText("Hash code", true));
		}

		void IDisposable.Dispose() {
			if (!_IsDisposed) {
				_TextBuffer.Changing -= _TextBuffer_Changing;
				GC.SuppressFinalize(this);
				_IsDisposed = true;
			}
		}

		static Brush GetFormatBrush(string name, IEditorFormatMap formatMap) {
			return formatMap.GetProperties(name)[EditorFormatDefinition.ForegroundBrushId] as Brush;
		}

		static ISymbol GetSymbol(SyntaxNode node, SemanticModel semanticModel) {
			return semanticModel.GetSymbolInfo(node).Symbol
					?? semanticModel.GetDeclaredSymbol(node)
					?? (node is AttributeArgumentSyntax
						? semanticModel.GetSymbolInfo((node as AttributeArgumentSyntax).Expression).Symbol
						: null)
					?? (node is SimpleBaseTypeSyntax || node is TypeConstraintSyntax
						? semanticModel.GetSymbolInfo(node.FindNode(node.Span, false, true)).Symbol
						: null);
		}

		static bool IsCommonClassName(string name) {
			return name == "Object" || name == "ValueType" || name == "Enum" || name == "MulticastDelegate";
		}

		static void ShowExtensionMethod(IList<object> qiContent, IMethodSymbol method, int position) {
			var info = ToUIText(method.ContainingType.ToDisplayParts(), "Defined in: ", true);
			string asmName = method.ContainingAssembly?.Modules?.FirstOrDefault()?.Name
				?? method.ContainingAssembly?.Name;
			if (asmName != null) {
				info.AddText(" (" + asmName + ")");
			}
			qiContent.Add(info);
		}

		static StackPanel ShowNumericForm(SyntaxNode node) {
			return ShowNumericForms(node.GetFirstToken().Value, node.Parent.Kind() == SyntaxKind.UnaryMinusExpression ? NumericForm.Negative : NumericForm.None);
		}

		static StackPanel ShowNumericForms(object value, NumericForm form) {
			if (value is int) {
				var v = (int)value;
				if (form == NumericForm.Negative) {
					v = -v;
				}
				var bytes = new byte[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };
				return ToUIText(form == NumericForm.Unsigned ? ((uint)v).ToString() : v.ToString(), bytes);
			}
			else if (value is long) {
				var v = (long)value;
				if (form == NumericForm.Negative) {
					v = -v;
				}
				var bytes = new byte[] { (byte)(v >> 56), (byte)(v >> 48), (byte)(v >> 40), (byte)(v >> 32), (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };
				return ToUIText(form == NumericForm.Unsigned ? ((ulong)v).ToString() : v.ToString(), bytes);
			}
			else if (value is byte) {
				return ToUIText(((byte)value).ToString(), new byte[] { (byte)value });
			}
			else if (value is short) {
				var v = (short)value;
				if (form == NumericForm.Negative) {
					v = (short)-v;
				}
				var bytes = new byte[] { (byte)(v >> 8), (byte)v };
				return ToUIText(form == NumericForm.Unsigned ? ((ushort)v).ToString() : v.ToString(), bytes);
			}
			else if (value is uint) {
				return ShowNumericForms((int)(uint)value, NumericForm.Unsigned);
			}
			else if (value is ulong) {
				return ShowNumericForms((long)(ulong)value, NumericForm.Unsigned);
			}
			else if (value is ushort) {
				return ShowNumericForms((short)(ushort)value, NumericForm.Unsigned);
			}
			else if (value is sbyte) {
				return ToUIText(((sbyte)value).ToString(), new byte[] { (byte)(sbyte)value });
			}
			return null;
		}

		static string ToBinString(byte[] bytes) {
			using (var sbr = ReusableStringBuilder.AcquireDefault((bytes.Length << 3) + bytes.Length)) {
				var sb = sbr.Resource;
				for (int i = 0; i < bytes.Length; i++) {
					ref var b = ref bytes[i];
					if (b == 0 && sb.Length == 0) {
						continue;
					}
					if (sb.Length > 0) {
						sb.Append(' ');
					}
					sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
				}
				return sb.Length == 0 ? "00000000" : sb.ToString();
			}
		}

		static string ToHexString(byte[] bytes) {
			switch (bytes.Length) {
				case 1: return bytes[0].ToString("X2");
				case 2: return bytes[0].ToString("X2") + bytes[1].ToString("X2");
				case 4:
					return bytes[0].ToString("X2") + bytes[1].ToString("X2") + " " + bytes[2].ToString("X2") + bytes[3].ToString("X2");
				case 8:
					return bytes[0].ToString("X2") + bytes[1].ToString("X2") + " " + bytes[2].ToString("X2") + bytes[3].ToString("X2") + " "
						+ bytes[4].ToString("X2") + bytes[5].ToString("X2") + " " + bytes[6].ToString("X2") + bytes[7].ToString("X2");
				default:
					return string.Empty;
			}
		}

		static StackPanel ToUIText(string dec, byte[] bytes) {
			var s = new StackPanel()
				.Add(new StackPanel().MakeHorizontal().AddReadOnlyTextBox(dec).AddText(" DEC", true))
				.Add(new StackPanel().MakeHorizontal().AddReadOnlyTextBox(ToHexString(bytes)).AddText(" HEX", true))
				.Add(new StackPanel().MakeHorizontal().AddReadOnlyTextBox(ToBinString(bytes)).AddText(" BIN", true));
			return s;
		}

		static StackPanel ToUIText(ImmutableArray<SymbolDisplayPart> parts) {
			return ToUIText(parts, null, false);
		}

		static StackPanel ToUIText(ImmutableArray<SymbolDisplayPart> parts, string title, bool bold) {
			var stack = new StackPanel { Orientation = Orientation.Horizontal };
			if (title != null) {
				stack.AddText(title, bold);
			}
			foreach (var part in parts) {
				switch (part.Kind) {
					case SymbolDisplayPartKind.ClassName:
						stack.AddText(part.Symbol.Name, true, false, _ClassBrush);
						break;
					case SymbolDisplayPartKind.InterfaceName:
						stack.AddText(part.Symbol.Name, true, false, _InterfaceBrush);
						break;
					case SymbolDisplayPartKind.StringLiteral:
						stack.AddText(part.ToString(), false, false, _TextBrush);
						break;
					case SymbolDisplayPartKind.Keyword:
						stack.AddText(part.ToString(), false, false, _KeywordBrush);
						break;
					default:
						stack.AddText(part.ToString());
						break;
				}
			}
			return stack;
		}

		static void UpdateSyntaxHighlights(IEditorFormatMap formatMap) {
			_InterfaceBrush = GetFormatBrush(Constants.CodeInterfaceName, formatMap);
			_ClassBrush = GetFormatBrush(Constants.CodeClassName, formatMap);
			_TextBrush = GetFormatBrush(Constants.CodeString, formatMap);
			_EnumBrush = GetFormatBrush(Constants.CodeEnumName, formatMap);
			_NumberBrush = GetFormatBrush(Constants.CodeNumber, formatMap);
			_StructBrush = GetFormatBrush(Constants.CodeStructName, formatMap);
			_KeywordBrush = GetFormatBrush(Constants.CodeKeyword, formatMap);
		}

		void _TextBuffer_Changing(object sender, TextContentChangingEventArgs e) {
			_SemanticModel = null;
		}

		StackPanel ShowAttributes(ImmutableArray<AttributeData> attrs, int position) {
			var stack = new StackPanel();
			stack.AddText(attrs.Length > 1 ? "Attributes:" : "Attribute:", true);
			foreach (var item in attrs) {
				var a = item.AttributeClass.Name;
				var attrStack = new StackPanel()
					.MakeHorizontal()
					.AddText("[")
					.AddText(a.EndsWith("Attribute", StringComparison.Ordinal) ? a.Substring(0, a.Length - 9) : a, _ClassBrush)
					.AddText("(");
				int i = 0;
				foreach (var arg in item.ConstructorArguments) {
					if (++i > 1) {
						attrStack.AddText(", ");
					}
					ToUIText(attrStack, arg, position);
				}
				var namedArguments = item.NamedArguments;
				foreach (var arg in namedArguments) {
					if (++i > 1) {
						attrStack.AddText(", ");
					}
					attrStack.AddText(arg.Key + "=", false, true);
					ToUIText(attrStack, arg.Value, position);
				}
				attrStack.AddText(")]");
				stack.Children.Add(attrStack);
			}
			return stack;
		}

		void ShowBaseType(IList<object> qiContent, INamedTypeSymbol typeSymbol, int position) {
			var baseType = typeSymbol.EnumUnderlyingType;
			if (baseType != null) {
				qiContent.Add(ToUIText(baseType.ToMinimalDisplayParts(_SemanticModel, position), "Underlying type: ", true));
				return;
			}
			baseType = typeSymbol.BaseType;
			if (baseType != null) {
				var name = baseType.Name;
				if (IsCommonClassName(name) == false) {
					var info = ToUIText(baseType.ToMinimalDisplayParts(_SemanticModel, position), "Base type: ", true);
					while (Config.Instance.ShowBaseTypeInheritenceQuickInfo && (baseType = baseType.BaseType) != null) {
						name = baseType.Name;
						if (IsCommonClassName(name) == false) {
							info.AddText(" - ").AddText(name, _ClassBrush);
						}
					}
					qiContent.Add(info);
				}
			}
		}

		void ShowEnumValueUnderlyingType(IList<object> qiContent, SyntaxNode node, ISymbol symbol) {
			if (Config.Instance.ShowBaseTypeQuickInfo) {
				var enumType = symbol.ContainingType.EnumUnderlyingType;
				if (enumType != null) {
					ShowBaseType(qiContent, symbol.ContainingType, node.SpanStart);
				}
			}
		}
		void ShowInterfaces(IList<object> output, ITypeSymbol type, int position) {
			const string SystemDisposable = "IDisposable";
			var showAll = Config.Instance.ShowInterfacesInheritenceQuickInfo;
			var interfaces = showAll ? type.AllInterfaces : type.Interfaces;
			if (interfaces.Length == 0) {
				return;
			}
			var stack = new StackPanel();
			stack.AddText(interfaces.Length > 1 ? "Interfaces:" : "Interface:", true);
			INamedTypeSymbol disposable = null;
			foreach (var item in interfaces) {
				if (item.Name == SystemDisposable) {
					disposable = item;
					continue;
				}
				stack.Children.Add(ToUIText(item.ToMinimalDisplayParts(_SemanticModel, position)));
			}
			if (disposable == null && showAll == false) {
				foreach (var item in type.AllInterfaces) {
					if (item.Name == SystemDisposable) {
						disposable = item;
						break;
					}
				}
			}
			if (disposable != null) {
				stack.Children.Insert(1, ToUIText(disposable.ToMinimalDisplayParts(_SemanticModel, position)));
			}
			output.Add(stack);
		}
		void ToUIText(StackPanel attrStack, TypedConstant v, int position) {
			switch (v.Kind) {
				case TypedConstantKind.Primitive:
					attrStack.AddText(v.ToCSharpString(), _NumberBrush);
					break;
				case TypedConstantKind.Enum:
					var en = v.ToCSharpString();
					attrStack.AddText(v.Type.Name + en.Substring(en.LastIndexOf('.')), _EnumBrush);
					break;
				case TypedConstantKind.Type:
					attrStack.AddText("typeof(");
					attrStack.Children.Add(ToUIText((v.Value as ITypeSymbol).ToMinimalDisplayParts(_SemanticModel, position)));
					attrStack.AddText(")");
					break;
				case TypedConstantKind.Array:
					attrStack.AddText("{");
					bool c = false;
					foreach (var item in v.Values) {
						if (c == false) {
							c = true;
						}
						else {
							attrStack.AddText(", ");
						}
						ToUIText(attrStack, item, position);
					}
					attrStack.AddText("}");
					break;
				default:
					attrStack.AddText(v.ToCSharpString());
					break;
			}
		}
		enum NumericForm
		{
			None,
			Negative,
			Unsigned
		}

		internal sealed class CSharpQuickInfoController : IIntellisenseController
		{
			private CSharpQuickInfoControllerProvider m_provider;
			private IQuickInfoSession m_session;
			private IList<ITextBuffer> m_subjectBuffers;
			private ITextView m_textView;
			internal CSharpQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, CSharpQuickInfoControllerProvider provider) {
				m_textView = textView;
				m_subjectBuffers = subjectBuffers;
				m_provider = provider;

				m_textView.MouseHover += this.OnTextViewMouseHover;
			}

			public void ConnectSubjectBuffer(ITextBuffer subjectBuffer) {
			}

			public void Detach(ITextView textView) {
				if (m_textView == textView) {
					m_textView.MouseHover -= this.OnTextViewMouseHover;
					m_textView = null;
				}
			}

			public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) {
			}

			private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e) {
				if (Config.Instance.ShowQuickInfo == false) {
					return;
				}
				//find the mouse position by mapping down to the subject buffer
				SnapshotPoint? point = m_textView.BufferGraph.MapDownToFirstMatch
					 (new SnapshotPoint(m_textView.TextSnapshot, e.Position),
					PointTrackingMode.Positive,
					snapshot => m_subjectBuffers.Contains(snapshot.TextBuffer),
					PositionAffinity.Predecessor);

				if (point != null && !m_provider.QuickInfoBroker.IsQuickInfoActive(m_textView)) {
					var triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);
					m_session = m_provider.QuickInfoBroker.TriggerQuickInfo(m_textView, triggerPoint, true);
				}
			}
		}

		[Export(typeof(IIntellisenseControllerProvider))]
		[Name("C# QuickInfo Controller")]
		[ContentType("CSharp")]
		internal sealed class CSharpQuickInfoControllerProvider : IIntellisenseControllerProvider
		{
			[Import]
			internal IQuickInfoBroker QuickInfoBroker { get; set; }

			public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers) {
				return new CSharpQuickInfoController(textView, subjectBuffers, this);
			}
		}

		[Export(typeof(IQuickInfoSourceProvider))]
		[Name("C# QuickInfo Source")]
		[Order(After = "Default Quick Info Presenter")]
		[ContentType("CSharp")]
		internal sealed class CSharpQuickInfoSourceProvider : IQuickInfoSourceProvider
		{
			[Import]
			IEditorFormatMapService _EditorFormatMapService = null;

			[Import]
			internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }
			[Import]
			internal ITextBufferFactoryService TextBufferFactoryService { get; set; }
			public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) {
				return new CSharpQuickInfoSource(this, textBuffer, _EditorFormatMapService);
			}
		}

	}
}