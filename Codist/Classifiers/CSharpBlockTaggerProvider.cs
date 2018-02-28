using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Codist.Classifiers
{
	[Export(typeof(ITaggerProvider))]
	[ContentType("CSharp")]
	[TagType(typeof(ICodeMemberTag))]
	internal sealed class CSharpBlockTaggerProvider : ITaggerProvider
	{
		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag {
			if (typeof(T) != typeof(ICodeMemberTag)) {
				return null;
			}

			var tagger = buffer.Properties.GetOrCreateSingletonProperty(
				typeof(CSharpBlockTaggerProvider),
				() => new CSharpBlockTagger(buffer)
			);
			return new DisposableTagger(tagger) as ITagger<T>;
		}
	}


	internal sealed class DisposableTagger : ITagger<ICodeMemberTag>, IDisposable
	{
		private CSharpBlockTagger _tagger;
		public DisposableTagger(CSharpBlockTagger tagger) {
			_tagger = tagger;
			_tagger.AddRef();
			_tagger.TagsChanged += OnTagsChanged;
		}

		private void OnTagsChanged(object sender, SnapshotSpanEventArgs e) {
			TagsChanged?.Invoke(sender, e);
		}

		public IEnumerable<ITagSpan<ICodeMemberTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
			return _tagger.GetTags(spans);
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		public void Dispose() {
			if (_tagger != null) {
				_tagger.TagsChanged -= OnTagsChanged;
				_tagger.Release();
				_tagger = null;
			}
		}
	}
}