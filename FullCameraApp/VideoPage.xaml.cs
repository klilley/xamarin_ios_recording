using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FullCameraApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VideoPage : ContentPage
	{
		public VideoPage ()
		{
			InitializeComponent ();
		}

		private void Button_Clicked(object sender, System.EventArgs e)
		{
			Navigation.PushModalAsync(new CameraPage());
		}

		protected override void OnDisappearing()
		{
			this.video.Pause();
		}

		protected override void OnAppearing()
		{
			this.video.Play();
		}
	}
}