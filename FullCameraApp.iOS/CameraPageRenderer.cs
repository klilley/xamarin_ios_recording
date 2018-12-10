using System;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using FullCameraApp.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using FullCameraApp;
using System.IO;
using AssetsLibrary;

[assembly: ExportRenderer(typeof(CameraPage), typeof(CameraPageRenderer))]
namespace FullCameraApp.iOS
{
	public class CameraPageRenderer : PageRenderer
	{
		AVCaptureSession captureSession;
		AVCaptureDeviceInput captureDeviceInput;
		AVCaptureMovieFileOutput movieOutput;

		UIPaintCodeButton takePhotoButton;
		UIPaintCodeButton cancelPhotoButton;
		UIView liveCameraStream;

		string filePath;
		NSUrl fileUrl;

		UIButton yes;
		UIButton no;
		UIButton review;


		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();
			SetupUserInterface();
			if (await AuthorizeCameraUse()) {
				SetupLiveCameraStream();
				movieOutput.StartRecordingToOutputFile(fileUrl, new MyRecordingDelegate());
				review.TouchUpInside += (s, e) =>
				{
					App.shouldSave = false;
					movieOutput.StopRecording();
					MessagingCenter.Send<App, object>(App.Current as App, "Pop", null);
				};

				Device.StartTimer(TimeSpan.FromMilliseconds(3000), () =>
				{					
					yes.TouchUpInside += (s, e) =>
					{
						App.shouldSave = true;
						movieOutput.StopRecording();
						MessagingCenter.Send<App, string>(App.Current as App, "OpenResult", "Yes");
					};
					yes.SetTitleColor(UIColor.Black, UIControlState.Normal);

					no.TouchUpInside += (s, e) =>
					{
						App.shouldSave = true;
						movieOutput.StopRecording();
						MessagingCenter.Send<App, string>(App.Current as App, "OpenResult", "No");
					};
					no.SetTitleColor(UIColor.Black, UIControlState.Normal);
					return false;
				});
			}
			
		}

		private void SetupUserInterface()
		{
			liveCameraStream = new UIView()
			{
				Frame = new CGRect(0f, 0f, View.Bounds.Width, View.Bounds.Height)
			};
			yes = new UIButton(UIButtonType.Plain);
			yes.SetTitle("Yes", UIControlState.Normal);
			yes.SetTitleColor(UIColor.White, UIControlState.Normal);
			yes.Frame = new CGRect(15, View.Bounds.Height - 75, 100, 100);

			no = new UIButton(UIButtonType.Plain);
			no.SetTitle("No", UIControlState.Normal);
			no.SetTitleColor(UIColor.White, UIControlState.Normal);
			no.Frame = new CGRect(View.Bounds.Width - 100, View.Bounds.Height - 75, 100, 100); 

			review = new UIButton(UIButtonType.Plain);
			review.SetTitle("Review", UIControlState.Normal);
			review.SetTitleColor(UIColor.Black, UIControlState.Normal);
			review.Frame = new CGRect(View.Bounds.Width / 2 - 50, View.Bounds.Height - 75, 100, 100);

			View.Add(liveCameraStream);
			View.Add(yes);
			View.Add(no);
			View.Add(review);

			//#region Button
			//var centerButtonX = View.Bounds.GetMidX() - 35f;
			//var bottomButtonY = View.Bounds.Bottom - 85;
			//var topRightX = View.Bounds.Right - 65;
			//var topLeftX = View.Bounds.X + 25;
			//var topButtonY = View.Bounds.Top + 25;
			//var buttonWidth = 70;
			//var buttonHeight = 70;

			//takePhotoButton = new UIPaintCodeButton(DrawTakePhotoButton)
			//{
			//	Frame = new CGRect(centerButtonX, bottomButtonY, buttonWidth, buttonHeight)
			//};

			//cancelPhotoButton = new UIPaintCodeButton(DrawCancelPictureButton)
			//{
			//	Frame = new CGRect(topLeftX, topButtonY, 37, 37)
			//};

			//View.Add(takePhotoButton);
			//View.Add(cancelPhotoButton);
			//#endregion
		}


		public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
		{
			var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

			foreach (var device in devices)
			{
				if (device.Position == orientation)
				{
					return device;
				}
			}
			return null;
		}

		public void SetupLiveCameraStream()
		{
			captureSession = new AVCaptureSession();

			var videoPreviewLayer = new AVCaptureVideoPreviewLayer(captureSession)
			{
				Frame = liveCameraStream.Bounds,
				
			};
			videoPreviewLayer.Orientation = AVCaptureVideoOrientation.LandscapeLeft;
			liveCameraStream.Layer.AddSublayer(videoPreviewLayer);

			var captureDevice = GetCameraForOrientation(AVCaptureDevicePosition.Front); //AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);

			ConfigureCameraForDevice(captureDevice);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);

			filePath = Path.Combine(Path.GetTempPath(), "temporary.mov");
			fileUrl = NSUrl.FromFilename(filePath);

			movieOutput = new AVCaptureMovieFileOutput();

			captureSession.AddOutput(movieOutput);
			captureSession.AddInput(captureDeviceInput);
			captureSession.StartRunning();
		}


		public void ConfigureCameraForDevice(AVCaptureDevice device)
		{
			var error = new NSError();
			if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
			{
				device.LockForConfiguration(out error);
				device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
				device.UnlockForConfiguration();
			}
			else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
			{
				device.LockForConfiguration(out error);
				device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
				device.UnlockForConfiguration();
			}
			else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
			{
				device.LockForConfiguration(out error);
				device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
				device.UnlockForConfiguration();
			}
		}

		public async Task<bool> AuthorizeCameraUse()
		{
			var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
			if (authorizationStatus != AVAuthorizationStatus.Authorized)
			{
				return await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
			}
			return await Task.FromResult(true);
		}

		#region Drawings

		private void DrawTakePhotoButton(CGRect frame)
		{
			var color = UIColor.White;

			var bezierPath = new UIBezierPath();
			bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.27302f * frame.Width, frame.GetMinY() + 0.15053f * frame.Height), new CGPoint(frame.GetMinX() + 0.41628f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height), new CGPoint(frame.GetMinX() + 0.33832f * frame.Width, frame.GetMinY() + 0.10803f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.15883f * frame.Width, frame.GetMinY() + 0.22484f * frame.Height), new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.35360f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height), new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.73012f * frame.Height), new CGPoint(frame.GetMinX() + 0.26988f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.73012f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height), new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.73012f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height), new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.26988f * frame.Height), new CGPoint(frame.GetMinX() + 0.73012f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height));
			bezierPath.ClosePath();
			bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height), new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.77614f * frame.Height), new CGPoint(frame.GetMinX() + 0.77614f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.22386f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height), new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.77614f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.19894f * frame.Width, frame.GetMinY() + 0.10076f * frame.Height), new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.33689f * frame.Height), new CGPoint(frame.GetMinX() + 0.07810f * frame.Width, frame.GetMinY() + 0.19203f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height), new CGPoint(frame.GetMinX() + 0.28269f * frame.Width, frame.GetMinY() + 0.03751f * frame.Height), new CGPoint(frame.GetMinX() + 0.38696f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.77614f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height), new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.22386f * frame.Height));
			bezierPath.ClosePath();
			color.SetFill();
			bezierPath.Fill();
			UIColor.Black.SetStroke();
			bezierPath.LineWidth = 1.0f;
			bezierPath.Stroke();

			var ovalPath = UIBezierPath.FromOval(new CGRect(frame.GetMinX() + NMath.Floor(frame.Width * 0.12500f + 0.5f), frame.GetMinY() + NMath.Floor(frame.Height * 0.12500f + 0.5f), NMath.Floor(frame.Width * 0.87500f + 0.5f) - NMath.Floor(frame.Width * 0.12500f + 0.5f), NMath.Floor(frame.Height * 0.87500f + 0.5f) - NMath.Floor(frame.Height * 0.12500f + 0.5f)));
			color.SetFill();
			ovalPath.Fill();
			UIColor.Black.SetStroke();
			ovalPath.LineWidth = 1.0f;
			ovalPath.Stroke();
		}

		private void DrawCancelPictureButton(CGRect frame)
		{
			var color2 = UIColor.White;

			var bezierPath = new UIBezierPath();
			bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 0.73928f * frame.Width, frame.GetMinY() + 0.14291f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height), new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height), new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.61785f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height), new CGPoint(frame.GetMinX() + 0.74457f * frame.Width, frame.GetMinY() + 0.37328f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.85355f * frame.Width, frame.GetMinY() + 0.73570f * frame.Height), new CGPoint(frame.GetMinX() + 0.74311f * frame.Width, frame.GetMinY() + 0.62526f * frame.Height), new CGPoint(frame.GetMinX() + 0.85355f * frame.Width, frame.GetMinY() + 0.73570f * frame.Height));
			bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.73570f * frame.Width, frame.GetMinY() + 0.85355f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.61785f * frame.Height), new CGPoint(frame.GetMinX() + 0.73570f * frame.Width, frame.GetMinY() + 0.85355f * frame.Height), new CGPoint(frame.GetMinX() + 0.62526f * frame.Width, frame.GetMinY() + 0.74311f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.26785f * frame.Width, frame.GetMinY() + 0.85000f * frame.Height), new CGPoint(frame.GetMinX() + 0.37621f * frame.Width, frame.GetMinY() + 0.74164f * frame.Height), new CGPoint(frame.GetMinX() + 0.26785f * frame.Width, frame.GetMinY() + 0.85000f * frame.Height));
			bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.15000f * frame.Width, frame.GetMinY() + 0.73215f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.38215f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.15000f * frame.Width, frame.GetMinY() + 0.73215f * frame.Height), new CGPoint(frame.GetMinX() + 0.25836f * frame.Width, frame.GetMinY() + 0.62379f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height), new CGPoint(frame.GetMinX() + 0.25689f * frame.Width, frame.GetMinY() + 0.37474f * frame.Height), new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.22060f * frame.Width, frame.GetMinY() + 0.19014f * frame.Height), new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height), new CGPoint(frame.GetMinX() + 0.18706f * frame.Width, frame.GetMinY() + 0.22369f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height), new CGPoint(frame.GetMinX() + 0.24420f * frame.Width, frame.GetMinY() + 0.16655f * frame.Height), new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.38215f * frame.Height), new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height), new CGPoint(frame.GetMinX() + 0.37474f * frame.Width, frame.GetMinY() + 0.25689f * frame.Height));
			bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height), new CGPoint(frame.GetMinX() + 0.62672f * frame.Width, frame.GetMinY() + 0.25543f * frame.Height), new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height));
			bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.73928f * frame.Width, frame.GetMinY() + 0.14291f * frame.Height));
			bezierPath.ClosePath();
			color2.SetFill();
			bezierPath.Fill();
			UIColor.Black.SetStroke();
			bezierPath.LineWidth = 1.0f;
			bezierPath.Stroke();
		}

		public UIBezierPath SquarePathWithCenter(CGPoint center, float side) {
			var squarePath = new UIBezierPath();
			var startX = center.X - side / 2;
			var startY = center.Y - side / 2;
			squarePath.MoveTo(new CGPoint(x: startX, y: startY));
			squarePath.AddLineTo(squarePath.CurrentPoint);
			squarePath.AddLineTo(new CGPoint(x: startX + side, y: startY));
			squarePath.AddLineTo(squarePath.CurrentPoint);
			squarePath.AddLineTo(new CGPoint(x: startX + side, y: startY + side));
			squarePath.AddLineTo(squarePath.CurrentPoint);
			squarePath.AddLineTo(new CGPoint(x: startX, y: startY + side));
			squarePath.AddLineTo(squarePath.CurrentPoint);
			squarePath.ClosePath();
			return squarePath;
		}

	#endregion

}

	internal class UIPaintCodeButton : UIButton
	{
		Action<CGRect> _drawing;
		public UIPaintCodeButton(Action<CGRect> drawing)
		{
			_drawing = drawing;
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			_drawing(rect);
		}

	}

	public class MyRecordingDelegate : AVCaptureFileOutputRecordingDelegate
	{
		public override void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
		{
			if (App.shouldSave && UIVideo.IsCompatibleWithSavedPhotosAlbum(outputFileUrl.Path))
			{
				var library = new ALAssetsLibrary();
				library.WriteVideoToSavedPhotosAlbum(outputFileUrl, (path, e2) =>
				{
					if (e2 != null)
					{
						//new UIAlertView("Error", e2.ToString(), null, "OK", null).Show();
					}
					else
					{
						//new UIAlertView("Saved", "Saved to Photos", null, "OK", null).Show();
						File.Delete(outputFileUrl.Path);
					}
				});
			}
			else
			{
				//new UIAlertView("Incompatible", "Incompatible", null, "OK", null).Show();
			}

		}
	}
}
