﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using Newtonsoft.Json;
using Codist.Classifiers;
using Codist.SyntaxHighlight;
using Codist.Margins;
using AppHelpers;

namespace Codist
{
	sealed class Config
	{
		const string ThemePrefix = "res:";
		const int DefaultIconSize = 20;
		internal const string LightTheme = ThemePrefix + "Light", DarkTheme = ThemePrefix + "Dark", SimpleTheme = ThemePrefix + "Simple";

		static DateTime _LastSaved, _LastLoaded;
		static int _LoadingConfig;

		public static readonly string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Constants.NameOfMe + "\\Config.json";
		public static Config Instance = InitConfig();

		[DefaultValue(Features.All)]
		public Features Features { get; set; }

		[DefaultValue(SpecialHighlightOptions.None)]
		public SpecialHighlightOptions SpecialHighlightOptions { get; set; }

		[DefaultValue(MarkerOptions.Default)]
		public MarkerOptions MarkerOptions { get; set; } = MarkerOptions.Default;

		[DefaultValue(QuickInfoOptions.Default)]
		public QuickInfoOptions QuickInfoOptions { get; set; } = QuickInfoOptions.Default;

		[DefaultValue(SmartBarOptions.Default)]
		public SmartBarOptions SmartBarOptions { get; set; } = SmartBarOptions.Default;

		public double TopSpace { get; set; }
		public double BottomSpace { get; set; }
		public double QuickInfoMaxWidth { get; set; }
		public double QuickInfoMaxHeight { get; set; }
		public bool NoSpaceBetweenWrappedLines { get; set; }
		[DefaultValue(DefaultIconSize)]
		public int SmartBarButtonSize { get; set; } = DefaultIconSize;
		public List<CommentLabel> Labels { get; } = new List<CommentLabel>();
		public List<CommentStyle> CommentStyles { get; } = new List<CommentStyle>();
		public List<XmlCodeStyle> XmlCodeStyles { get; } = new List<XmlCodeStyle>();
		public List<CSharpStyle> CodeStyles { get; } = new List<CSharpStyle>();
		public List<CodeStyle> GeneralStyles { get; } = new List<CodeStyle>();
		public List<SymbolMarkerStyle> SymbolMarkerStyles { get; } = new List<SymbolMarkerStyle>();
		public List<MarkerStyle> MarkerSettings { get; } = new List<MarkerStyle>();

		public static event EventHandler Loaded;
		public static event EventHandler<ConfigUpdatedEventArgs> Updated;

		public static Config InitConfig() {
			//AppHelpers.LogHelper.UseLogMethod(i => Debug.WriteLine(i));
			if (File.Exists(ConfigPath) == false) {
				var config = GetDefaultConfig();
				config.SaveConfig(ConfigPath);
				return config;
			}
			try {
				return InternalLoadConfig(ConfigPath);
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				return GetDefaultConfig();
			}
		}

		public static void LoadConfig(string configPath) {
			//HACK: prevent redundant load operations issued by configuration pages
			if (_LastLoaded.AddSeconds(2) > DateTime.Now
				|| Interlocked.Exchange(ref _LoadingConfig, 1) != 0) {
				return;
			}
			try {
				Instance = InternalLoadConfig(configPath);
				Loaded?.Invoke(Instance, EventArgs.Empty);
				Updated?.Invoke(Instance, new ConfigUpdatedEventArgs(Features.All));
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				Instance = GetDefaultConfig();
			}
			finally {
				_LoadingConfig = 0;
			}
		}

		static Config InternalLoadConfig(string configPath) {
			var configContent = configPath == LightTheme ? Properties.Resources.Light
				: configPath == DarkTheme ? Properties.Resources.Dark
				: configPath == SimpleTheme ? Properties.Resources.Simple
				: File.ReadAllText(configPath);
			var loadFromTheme = configPath.StartsWith(ThemePrefix, StringComparison.Ordinal);
			var config = JsonConvert.DeserializeObject<Config>(configContent, new JsonSerializerSettings {
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				NullValueHandling = NullValueHandling.Ignore,
				Error = (sender, args) => {
					args.ErrorContext.Handled = true; // ignore json error
				}
			});
			var l = config.Labels;
			for (var i = l.Count - 1; i >= 0; i--) {
				if (String.IsNullOrWhiteSpace(l[i].Label)) {
					l.RemoveAt(i);
				}
			}
			if (l.Count == 0) {
				InitDefaultLabels(l);
			}
			LoadStyleEntries<CodeStyle, CodeStyleTypes>(config.GeneralStyles, loadFromTheme);
			LoadStyleEntries<CommentStyle, CommentStyleTypes>(config.CommentStyles, loadFromTheme);
			LoadStyleEntries<CSharpStyle, CSharpStyleTypes>(config.CodeStyles, loadFromTheme);
			LoadStyleEntries<XmlCodeStyle, XmlStyleTypes>(config.XmlCodeStyles, loadFromTheme);
			LoadStyleEntries<SymbolMarkerStyle, SymbolMarkerStyleTypes>(config.SymbolMarkerStyles, loadFromTheme);
			if (loadFromTheme) {
				// don't override other settings if loaded from predefined themes
				ResetCodeStyle(Instance.GeneralStyles, config.GeneralStyles);
				ResetCodeStyle(Instance.CommentStyles, config.CommentStyles);
				ResetCodeStyle(Instance.CodeStyles, config.CodeStyles);
				ResetCodeStyle(Instance.XmlCodeStyles, config.XmlCodeStyles);
				ResetCodeStyle(Instance.SymbolMarkerStyles, config.SymbolMarkerStyles);
				ResetCodeStyle(Instance.MarkerSettings, config.MarkerSettings);
				_LastLoaded = DateTime.Now;
				return Instance;
			}
			_LastLoaded = DateTime.Now;
			Debug.WriteLine("Config loaded");
			return config;
		}

		public static void ResetStyles() {
			ResetCodeStyle(Instance.GeneralStyles, GetDefaultCodeStyles<CodeStyle, CodeStyleTypes>());
			ResetCodeStyle(Instance.CommentStyles, GetDefaultCommentStyles());
			ResetCodeStyle(Instance.CodeStyles, GetDefaultCodeStyles<CSharpStyle, CSharpStyleTypes>());
			ResetCodeStyle(Instance.XmlCodeStyles, GetDefaultCodeStyles<XmlCodeStyle, XmlStyleTypes>());
			ResetCodeStyle(Instance.SymbolMarkerStyles, GetDefaultCodeStyles<SymbolMarkerStyle, SymbolMarkerStyleTypes>());
			ResetCodeStyle(Instance.MarkerSettings, GetDefaultMarkerStyles());
			Updated?.Invoke(Instance, new ConfigUpdatedEventArgs(Features.SyntaxHighlight));
		}

		public void SaveConfig(string path) {
			//HACK: prevent redundant save operations issued by configuration pages
			if (_LastSaved.AddSeconds(2) > DateTime.Now) {
				return;
			}
			path = path ?? ConfigPath;
			try {
				var d = Path.GetDirectoryName(path);
				if (Directory.Exists(d) == false) {
					Directory.CreateDirectory(d);
				}
				File.WriteAllText(path, JsonConvert.SerializeObject(
					this,
					Formatting.Indented,
					new JsonSerializerSettings {
						DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
						NullValueHandling = NullValueHandling.Ignore,
						Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
					}));
				if (path == ConfigPath) {
					_LastSaved = _LastLoaded = DateTime.Now;
					Debug.WriteLine("Config saved");
					//Updated?.Invoke(this, EventArgs.Empty);
				}
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		internal void FireConfigChangedEvent(Features updatedFeature) {
			Updated?.Invoke(this, new ConfigUpdatedEventArgs(updatedFeature));
		}

		static void LoadStyleEntries<TStyle, TStyleType> (List<TStyle> styles, bool removeFontNames)
			where TStyle : StyleBase<TStyleType>, new()
			where TStyleType : struct, Enum {
			styles.RemoveAll(i => i.Id < 1);
			styles.Sort((x, y) => x.Id - y.Id);
			if (removeFontNames) {
				styles.ForEach(i => i.Font = null);
			}
			for (var i = styles.Count - 1; i >= 0; i--) {
				if (styles[i] == null || EnumHelper.IsDefined(styles[i].StyleID) == false) {
					styles.RemoveAt(i);
				}
			}
			MergeDefaultCodeStyles<TStyle, TStyleType>(styles);
		}

		static Config GetDefaultConfig() {
			_LastLoaded = DateTime.Now;
			var c = new Config();
			InitDefaultLabels(c.Labels);
			c.GeneralStyles.AddRange(GetDefaultCodeStyles<CodeStyle, CodeStyleTypes>());
			c.CommentStyles.AddRange(GetDefaultCodeStyles<CommentStyle, CommentStyleTypes>());
			c.CodeStyles.AddRange(GetDefaultCodeStyles<CSharpStyle, CSharpStyleTypes>());
			c.XmlCodeStyles.AddRange(GetDefaultCodeStyles<XmlCodeStyle, XmlStyleTypes>());
			c.SymbolMarkerStyles.AddRange(GetDefaultCodeStyles<SymbolMarkerStyle, SymbolMarkerStyleTypes>());
			c.MarkerSettings.AddRange(GetDefaultMarkerStyles());
			return c;
		}

		static void InitDefaultLabels(List<CommentLabel> labels) {
			labels.AddRange (new CommentLabel[] {
				new CommentLabel("!", CommentStyleTypes.Emphasis),
				new CommentLabel("#", CommentStyleTypes.Emphasis),
				new CommentLabel("?", CommentStyleTypes.Question),
				new CommentLabel("!?", CommentStyleTypes.Exclaimation),
				new CommentLabel("x", CommentStyleTypes.Deletion, true),
				new CommentLabel("+++", CommentStyleTypes.Heading1),
				new CommentLabel("!!", CommentStyleTypes.Heading1),
				new CommentLabel("++", CommentStyleTypes.Heading2),
				new CommentLabel("+", CommentStyleTypes.Heading3),
				new CommentLabel("-", CommentStyleTypes.Heading4),
				new CommentLabel("--", CommentStyleTypes.Heading5),
				new CommentLabel("---", CommentStyleTypes.Heading6),
				new CommentLabel("TODO", CommentStyleTypes.ToDo, true) { AllowPunctuationDelimiter = true },
				new CommentLabel("TO-DO", CommentStyleTypes.ToDo, true) { AllowPunctuationDelimiter = true },
				new CommentLabel("undone", CommentStyleTypes.ToDo, true) { AllowPunctuationDelimiter = true },
				new CommentLabel("NOTE", CommentStyleTypes.Note, true) { AllowPunctuationDelimiter = true },
				new CommentLabel("HACK", CommentStyleTypes.Hack, true) { AllowPunctuationDelimiter = true },
			});
		}
		static void MergeDefaultCodeStyles<TStyle, TStyleType> (List<TStyle> styles)
			where TStyle : StyleBase<TStyleType>, new()
			where TStyleType : struct, Enum {
			foreach (var s in GetDefaultCodeStyles<TStyle, TStyleType>()) {
				if (s.Id > 0 && styles.FindIndex(i => ClrHacker.DirectCompare(i.StyleID, s.StyleID)) == -1) {
					styles.Add(s);
				}
			}
		}
		static void ResetCodeStyle<TStyle>(List<TStyle> source, IEnumerable<TStyle> target) {
			source.Clear();
			source.AddRange(target);
		}
		internal static CommentStyle[] GetDefaultCommentStyles() {
			return new CommentStyle[] {
				new CommentStyle(CommentStyleTypes.Emphasis, Constants.CommentColor) { Bold = true, FontSize = 10 },
				new CommentStyle(CommentStyleTypes.Exclaimation, Constants.ExclaimationColor),
				new CommentStyle(CommentStyleTypes.Question, Constants.QuestionColor),
				new CommentStyle(CommentStyleTypes.Deletion, Constants.DeletionColor) { Strikethrough = true },
				new CommentStyle(CommentStyleTypes.ToDo, Colors.White) { BackgroundColor = Constants.ToDoColor.ToHexString(), ScrollBarMarkerStyle = ScrollbarMarkerStyle.Square },
				new CommentStyle(CommentStyleTypes.Note, Colors.White) { BackgroundColor = Constants.NoteColor.ToHexString(), ScrollBarMarkerStyle = ScrollbarMarkerStyle.Square },
				new CommentStyle(CommentStyleTypes.Hack, Colors.White) { BackgroundColor = Constants.HackColor.ToHexString(), ScrollBarMarkerStyle = ScrollbarMarkerStyle.Square },
				new CommentStyle(CommentStyleTypes.Undone, Color.FromRgb(164, 175, 209)) { BackgroundColor = Constants.UndoneColor.ToHexString(), ScrollBarMarkerStyle = ScrollbarMarkerStyle.Square },
				new CommentStyle(CommentStyleTypes.Heading1) { FontSize = 12 },
				new CommentStyle(CommentStyleTypes.Heading2) { FontSize = 8 },
				new CommentStyle(CommentStyleTypes.Heading3) { FontSize = 4 },
				new CommentStyle(CommentStyleTypes.Heading4) { FontSize = -1 },
				new CommentStyle(CommentStyleTypes.Heading5) { FontSize = -2 },
				new CommentStyle(CommentStyleTypes.Heading6) { FontSize = -3 },
				new CommentStyle(CommentStyleTypes.Task1) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number1 },
				new CommentStyle(CommentStyleTypes.Task2) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number2 },
				new CommentStyle(CommentStyleTypes.Task3) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number3 },
				new CommentStyle(CommentStyleTypes.Task4) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number4 },
				new CommentStyle(CommentStyleTypes.Task5) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number5 },
				new CommentStyle(CommentStyleTypes.Task6) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number6 },
				new CommentStyle(CommentStyleTypes.Task7) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number7 },
				new CommentStyle(CommentStyleTypes.Task8) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number8 },
				new CommentStyle(CommentStyleTypes.Task9) { ScrollBarMarkerStyle = ScrollbarMarkerStyle.Number9 },
			};
		}
		internal static TStyle[] GetDefaultCodeStyles<TStyle, TStyleType>()
			where TStyle : StyleBase<TStyleType>, new()
			where TStyleType : struct, Enum {
			var r = new TStyle[Enum.GetValues(typeof(TStyleType)).Length];
			for (var i = 0; i < r.Length; i++) {
				r[i] = new TStyle { StyleID = ClrHacker.DirectCast<int, TStyleType>(i) };
			}
			return r;
		}
		internal static MarkerStyle[] GetDefaultMarkerStyles() {
			return new MarkerStyle[] {
				new MarkerStyle(MarkerStyleTypes.SymbolReference, Colors.Cyan)
			};
		}
		internal void Set(Features options, bool set) {
			Features = EnumHelper.SetFlags(Features, options, set);
		}
		internal void Set(QuickInfoOptions options, bool set) {
			QuickInfoOptions = EnumHelper.SetFlags(QuickInfoOptions, options, set);
		}
		internal void Set(MarkerOptions options, bool set) {
			MarkerOptions = EnumHelper.SetFlags(MarkerOptions, options, set);
			FireConfigChangedEvent(Features.ScrollbarMarkers);
		}
		internal void Set(SpecialHighlightOptions options, bool set) {
			SpecialHighlightOptions = EnumHelper.SetFlags(SpecialHighlightOptions, options, set);
			FireConfigChangedEvent(Features.SyntaxHighlight);
		}
	}

	sealed class ConfigUpdatedEventArgs : EventArgs
	{
		public ConfigUpdatedEventArgs(Features updatedFeature) {
			UpdatedFeature = updatedFeature;
		}

		public Features UpdatedFeature { get; }
	}

	public enum BrushEffect
	{
		Solid,
		ToBottom,
		ToTop,
		ToRight,
		ToLeft
	}

	[Flags]
	public enum Features
	{
		None,
		SyntaxHighlight = 1,
		ScrollbarMarkers = 1 << 1,
		SuperQuickInfo = 1 << 2,
		SmartBar = 1 << 3,
		All = SyntaxHighlight | ScrollbarMarkers | SuperQuickInfo | SmartBar
	}

	[Flags]
	public enum QuickInfoOptions
	{
		None,
		Attributes = 1,
		BaseType = 1 << 1,
		BaseTypeInheritence = 1 << 2,
		Declaration = 1 << 3,
		SymbolLocation = 1 << 4,
		Interfaces = 1 << 5,
		InterfacesInheritence = 1 << 6,
		NumericValues = 1 << 7,
		String = 1 << 8,
		Parameter = 1 << 9,
		InterfaceImplementations = 1 << 10,
		TypeParameters = 1 << 11,
		NamespaceTypes = 1 << 12,
		OverrideDefaultDocumentation = 1 << 20,
		DocumentationFromBaseType = 1 << 21,
		TextOnlyDoc = 1 << 22,
		ReturnsDoc = 1 << 23,
		RemarksDoc = 1 << 24,
		Color = 1 << 26,
		Selection = 1 << 27,
		ClickAndGo = 1 << 28,
		CtrlQuickInfo = 1 << 29,
		HideOriginalQuickInfo = 1 << 30,
		QuickInfoOverride = OverrideDefaultDocumentation | DocumentationFromBaseType | ClickAndGo,
		Default = Attributes | BaseType | Interfaces | NumericValues | InterfaceImplementations | ClickAndGo,
	}

	[Flags]
	public enum SmartBarOptions
	{
		None,
		ExpansionIncludeTrivia = 1 << 1,
		ShiftSuppression = 1 << 2,
		Default = ExpansionIncludeTrivia
	}

	[Flags]
	public enum SpecialHighlightOptions
	{
		None,
		SpecialComment = 1,
		DeclarationBrace = 1 << 1,
		ParameterBrace = 1 << 2,
		XmlDocCode = 1 << 3,
		BranchBrace = 1 << 4,
		LoopBrace = 1 << 5,
		ResourceBrace = 1 << 6,
		SpecialPunctuation = 1 << 7,
		AllBraces = DeclarationBrace | ParameterBrace | BranchBrace | LoopBrace | ResourceBrace | SpecialPunctuation
	}

	[Flags]
	public enum MarkerOptions
	{
		None,
		SpecialComment = 1,
		MemberDeclaration = 1 << 1,
		LongMemberDeclaration = 1 << 2,
		CompilerDirective = 1 << 3,
		LineNumber = 1 << 4,
		TypeDeclaration = 1 << 5,
		MethodDeclaration = 1 << 6,
		SymbolReference = 1 << 7,
		CodeMarginMask = SpecialComment | CompilerDirective,
		MemberMarginMask = MemberDeclaration | SymbolReference,
		Default = SpecialComment | MemberDeclaration | LineNumber | LongMemberDeclaration | SymbolReference
	}

	public enum ScrollbarMarkerStyle
	{
		None,
		Square,
		Circle,
		Number1,
		Number2,
		Number3,
		Number4,
		Number5,
		Number6,
		Number7,
		Number8,
		Number9,
	}
}
