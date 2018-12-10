using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FullCameraApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Result : ContentPage
	{
		public Result (string result)
		{
			InitializeComponent ();

			this.result_label.Text = result;
		}

		private void Result_label_Clicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new VideoPage());
		}
	}
}