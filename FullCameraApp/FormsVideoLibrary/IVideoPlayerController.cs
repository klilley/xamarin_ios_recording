using System;

namespace FullCameraApp.FormsVideoLibrary
{
	public interface IVideoPlayerController
	{
		VideoStatus Status { set; get; }

		TimeSpan Duration { set; get; }
	}
}
