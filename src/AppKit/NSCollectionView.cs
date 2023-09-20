#if !__MACCATALYST__
using System;
using Foundation;
using ObjCRuntime;

#nullable enable

namespace AppKit {
	public partial class NSCollectionView {
		public void RegisterClassForItem (Type itemClass, string identifier)
		{
			_RegisterClassForItem (itemClass is null ? IntPtr.Zero : Class.GetHandle (itemClass), identifier);
		}

		public void RegisterClassForItem<TItem> (string identifier)
		{
			// TODO we could use virtual methods on TItem here
			// in the original method we can't really do that :-/
			RegisterClassForItem (typeof (TItem), identifier);
		}

		public void RegisterClassForSupplementaryView (Type viewClass, NSString kind, string identifier)
		{
			_RegisterClassForSupplementaryView (viewClass is null ? IntPtr.Zero : Class.GetHandle (viewClass), kind, identifier);
		}

#if !NET
		[Obsolete ("Use 'GetLayoutAttributes' instead.")]
		public virtual NSCollectionViewLayoutAttributes? GetLayoutAttributest (string kind, NSIndexPath indexPath)
		{
			return GetLayoutAttributes (kind, indexPath);
		}
#endif // !NET
	}
}
#endif // !__MACCATALYST__
