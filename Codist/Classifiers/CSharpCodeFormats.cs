﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows;

namespace Codist.Classifiers
{
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpLocalFieldName)]
	[Name(Constants.CSharpLocalFieldName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class LocalFieldFormat : ClassificationFormatDefinition
	{
		public LocalFieldFormat() {
			DisplayName = Constants.NameOfMe + ": local field";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpConstFieldName)]
	[Name(Constants.CSharpConstFieldName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ConstFieldFormat : ClassificationFormatDefinition
	{
		public ConstFieldFormat() {
			DisplayName = Constants.NameOfMe + ": const field";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpReadOnlyFieldName)]
	[Name(Constants.CSharpReadOnlyFieldName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ReadOnlyFieldFormat : ClassificationFormatDefinition
	{
		public ReadOnlyFieldFormat() {
			DisplayName = Constants.NameOfMe + ": readonly field";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpParameterName)]
	[Name(Constants.CSharpParameterName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ParameterFormat : ClassificationFormatDefinition
	{
		public ParameterFormat() {
			DisplayName = Constants.NameOfMe + ": parameter";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpTypeParameterName)]
	[Name(Constants.CSharpTypeParameterName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class TypeParameterFormat : ClassificationFormatDefinition
	{
		public TypeParameterFormat() {
			DisplayName = Constants.NameOfMe + ": type parameter";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpNamespaceName)]
	[Name(Constants.CSharpNamespaceName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class NamespaceFormat : ClassificationFormatDefinition
	{
		public NamespaceFormat() {
			DisplayName = Constants.NameOfMe + ": namespace";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpExtensionMethodName)]
	[Name(Constants.CSharpExtensionMethodName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ExtensionMethodFormat : ClassificationFormatDefinition
	{
		public ExtensionMethodFormat() {
			DisplayName = Constants.NameOfMe + ": extension method";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpExternMethodName)]
	[Name(Constants.CSharpExternMethodName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ExternMethodFormat : ClassificationFormatDefinition
	{
		public ExternMethodFormat() {
			DisplayName = Constants.NameOfMe + ": extern method";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpMethodName)]
	[Name(Constants.CSharpMethodName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class MethodFormat : ClassificationFormatDefinition
	{
		public MethodFormat() {
			DisplayName = Constants.NameOfMe + ": method";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpEventName)]
	[Name(Constants.CSharpEventName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class EventFormat : ClassificationFormatDefinition
	{
		public EventFormat() {
			DisplayName = Constants.NameOfMe + ": event";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpPropertyName)]
	[Name(Constants.CSharpPropertyName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class PropertyFormat : ClassificationFormatDefinition
	{
		public PropertyFormat() {
			DisplayName = Constants.NameOfMe + ": property";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpFieldName)]
	[Name(Constants.CSharpFieldName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class FieldFormat : ClassificationFormatDefinition
	{
		public FieldFormat() {
			DisplayName = Constants.NameOfMe + ": field";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpAliasNamespaceName)]
	[Name(Constants.CSharpAliasNamespaceName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class AliasNamespaceFormat : ClassificationFormatDefinition
	{
		public AliasNamespaceFormat() {
			DisplayName = Constants.NameOfMe + ": alias namespace";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpConstructorMethodName)]
	[Name(Constants.CSharpConstructorMethodName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class ConstructorMethodFormat : ClassificationFormatDefinition
	{
		public ConstructorMethodFormat() {
			DisplayName = Constants.NameOfMe + ": constructor method";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpDeclarationName)]
	[Name(Constants.CSharpDeclarationName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class DeclarationFormat : ClassificationFormatDefinition
	{
		public DeclarationFormat() {
			DisplayName = Constants.NameOfMe + ": declaration";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpNestedDeclarationName)]
	[Name(Constants.CSharpNestedDeclarationName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class NestedDeclarationFormat : ClassificationFormatDefinition
	{
		public NestedDeclarationFormat() {
			DisplayName = Constants.NameOfMe + ": nested declaration";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpStaticMemberName)]
	[Name(Constants.CSharpStaticMemberName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class StaticMemberFormat : ClassificationFormatDefinition
	{
		public StaticMemberFormat() {
			DisplayName = Constants.NameOfMe + ": static member";
			TextDecorations = new TextDecorationCollection(System.Windows.TextDecorations.Underline);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpOverrideMemberName)]
	[Name(Constants.CSharpOverrideMemberName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class OverrideMemberFormat : ClassificationFormatDefinition
	{
		public OverrideMemberFormat() {
			DisplayName = Constants.NameOfMe + ": override member";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpAbstractMemberName)]
	[Name(Constants.CSharpAbstractMemberName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class AbstractMemberFormat : ClassificationFormatDefinition
	{
		public AbstractMemberFormat() {
			DisplayName = Constants.NameOfMe + ": abstract member";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpVirtualMemberName)]
	[Name(Constants.CSharpVirtualMemberName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class VirtualMemberFormat : ClassificationFormatDefinition
	{
		public VirtualMemberFormat() {
			DisplayName = Constants.NameOfMe + ": virtual member";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpSealedClassName)]
	[Name(Constants.CSharpSealedClassName)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class SealedClassFormat : ClassificationFormatDefinition
	{
		public SealedClassFormat() {
			DisplayName = Constants.NameOfMe + ": sealed class";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpLabel)]
	[Name(Constants.CSharpLabel)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class LabelFormat : ClassificationFormatDefinition
	{
		public LabelFormat() {
			DisplayName = Constants.NameOfMe + ": label";
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.CSharpAttributeNotation)]
	[Name(Constants.CSharpAttributeNotation)]
	[UserVisible(false)]
	[Order(After = Priority.High)]
	internal sealed class AttributeNotationFormat : ClassificationFormatDefinition
	{
		public AttributeNotationFormat() {
			DisplayName = Constants.NameOfMe + ": attribute notation";
		}
	}
}