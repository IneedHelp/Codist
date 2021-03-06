﻿using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using AppHelpers;

namespace Codist.SyntaxHighlight
{
	/// <summary>
	/// Applies customized syntax highlight styles to editor.
	/// </summary>
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType(Constants.CodeTypes.Code)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	sealed class CodeViewCreationListener : IWpfTextViewCreationListener
	{
		public void TextViewCreated(IWpfTextView textView) {
			if (Config.Instance.Features.MatchFlags(Features.SyntaxHighlight) == false) {
				return;
			}
			textView.Properties.GetOrCreateSingletonProperty(() => {
				return new CodeViewDecorator(
					textView,
					ServicesHelper.Instance.ClassificationFormatMap.GetClassificationFormatMap(textView),
					ServicesHelper.Instance.ClassificationTypeRegistry,
					ServicesHelper.Instance.EditorFormatMap.GetEditorFormatMap(textView));
			});
			//IEditorFormatMap formatMap = _EditorFormatMapService.GetEditorFormatMap(textView);
			//ChangeEditorFormat(formatMap, EditorTextViewBackground, m => m[EditorFormatDefinition.BackgroundBrushId] = Brushes.LightYellow);
		}

		static void ChangeEditorFormat(IEditorFormatMap formatMap, string propertyId, Action<System.Windows.ResourceDictionary> changer) {
			var m = formatMap.GetProperties(propertyId);
			if (m != null) {
				changer(m);
			}
			formatMap.SetProperties(propertyId, m);
		}

	}
}
