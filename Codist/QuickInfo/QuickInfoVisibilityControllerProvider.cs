﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using AppHelpers;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Codist.QuickInfo
{
	/// <summary>
	/// Controls whether quick info should be displayed. When activated, quick info would not be displayed unless Shift key is pressed.
	/// </summary>
	[Export(typeof(IQuickInfoSourceProvider))]
	[Name("Quick Info Visibility Controller")]
	[Order(Before = "Default Quick Info Presenter")]
	[ContentType(Constants.CodeTypes.Code)]
	sealed class QuickInfoVisibilityControllerProvider : IQuickInfoSourceProvider
	{
		public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) {
			return new QuickInfoVisibilityController();
		}

		sealed class QuickInfoVisibilityController : IQuickInfoSource
		{
			public void AugmentQuickInfoSession(IQuickInfoSession session, IList<Object> qiContent, out ITrackingSpan applicableToSpan) {
				// don't show Quick Info when CtrlQuickInfo option is on and shift is not pressed
				if (Config.Instance.QuickInfoOptions.MatchFlags(QuickInfoOptions.CtrlQuickInfo)
					&& Keyboard.Modifiers.MatchFlags(ModifierKeys.Shift) == false
				// do not show Quick Info when user is hovering on the smart bar
					|| session.TextView.Properties.ContainsProperty(nameof(SmartBars.SmartBar))
					) {
					session.Dismiss();
				}
				applicableToSpan = null;
			}

			void IDisposable.Dispose() {}

		}

		sealed class QuickInfoContainer : StackPanel
		{
			protected override void OnVisualParentChanged(DependencyObject oldParent) {
				base.OnVisualParentChanged(oldParent);
				if (VisualParent == null) {
					return;
				}
				var p = VisualParent;
				var root = FindVisualRoot(VisualParent);
				var items = System.Windows.Media.VisualTreeHelper.GetChild(root, 0);
				items = System.Windows.Media.VisualTreeHelper.GetChild(items, 0);
				var ch = LogicalTreeHelper.GetChildren(root);
				if (items != null) {
					var ic = FindAncestorOrSelf<ItemsControl>(VisualParent);
					if (ic.Parent is StackPanel) {
						return;
					}
					var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto }.LimitSize();
					(ic.Parent as UserControl).Content = scrollViewer;
					scrollViewer.Content = ic;
				}
			}
			static DependencyObject FindLogicalRoot(DependencyObject obj) {
				var r = obj;
				while ((obj = LogicalTreeHelper.GetParent(obj)) != null) {
					r = obj;
				}
				return r;
			}
			static DependencyObject FindVisualRoot(DependencyObject obj) {
				var r = obj;
				while ((obj = System.Windows.Media.VisualTreeHelper.GetParent(obj)) != null) {
					r = obj;
				}
				return r;
			}
			static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject {
				while (obj != null) {
					T t = obj as T;
					if (t != null) {
						return t;
					}
					obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
				}
				return null;
			}
		}
	}

}
