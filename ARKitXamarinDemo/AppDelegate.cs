using Foundation;
using System.Threading.Tasks;
using UIKit;
using ARKit;
using System.Linq;
using Urho;
using AVFoundation;
using MonoTouch.Dialog;
using Urho.iOS;

namespace ARKitXamarinDemo
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			var rootElement = new RootElement("UrhoSharp");
			Window.RootViewController = new DialogViewController(rootElement);
			var section = new Section("ARKit samples", "UrhoSharp");
			rootElement.Add(section);
            section.Add(new StringElement("Mutant demo", () => RunMutantSurface()));
			section.Add(new StringElement("Crowd demo", () => Run<CrowdDemo>()));
			section.Add(new StringElement("Ruler demo", () => Run<RulerDemo>()));

			Window.MakeKeyAndVisible();

			return true;
		}

        static void Run<T>() where T : ArkitApp
		{
			Urho.Application.CreateInstance<T>(new ApplicationOptions {
				ResourcePaths = new[] { "UrhoData" },
                Orientation = ApplicationOptions.OrientationType.Portrait
			}).Run();
		}

        void RunMutantSurface()
        {
            var viewController = new UIViewController();

            var surface = new UrhoSurface(UIScreen.MainScreen.Bounds);

            viewController.View.AddSubview(surface);

            Window.RootViewController.PresentViewController(viewController, false, null);

            surface.Show<MutantDemo>(new ApplicationOptions
            {
                ResourcePaths = new[] { "UrhoData" },
                Orientation = ApplicationOptions.OrientationType.Portrait
            });
        }
	}
}

