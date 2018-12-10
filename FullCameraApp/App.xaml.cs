using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace FullCameraApp
{
	public partial class App : Application
	{
		public static bool shouldSave;
		public App ()
		{
			InitializeComponent();
			MainPage = new VideoPage();
			MessagingCenter.Subscribe<App, object>(App.Current, "Pop", (snd, arg) =>
			{
				Device.BeginInvokeOnMainThread(() => {
					MainPage.Navigation.PopModalAsync();
				});
			});
			MessagingCenter.Subscribe<App, string>(App.Current, "OpenResult", (snd, arg) =>
			{
				Device.BeginInvokeOnMainThread(() => {
					MainPage.Navigation.PushModalAsync(new Result(arg));
				});
			});


		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
