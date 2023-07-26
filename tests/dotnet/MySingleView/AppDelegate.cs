using System;

using Foundation;
using UIKit;

namespace MySingleView {
	public partial class AppDelegate : UIApplicationDelegate {
		UIWindow window;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			var dvc = new UIViewController ();
			var button = new UIButton (window.Bounds);
			button.SetTitle ("\"Source-generated\" Managed Static Registrar", UIControlState.Normal);
			var clicked = 0;
			button.TouchUpInside += (sender, e) => {
				var pluralSuffix = clicked > 1 ? "s" : "";
				button.SetTitle ($"Clicked {++clicked} time{pluralSuffix}.", UIControlState.Normal);
			};
			dvc.Add (button);

			window.RootViewController = dvc;
			window.MakeKeyAndVisible ();

			return true;
		}
	}
}
