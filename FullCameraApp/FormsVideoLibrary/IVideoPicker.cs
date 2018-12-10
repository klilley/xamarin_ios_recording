using System;
using System.Threading.Tasks;

namespace FullCameraApp.FormsVideoLibrary
{
	public interface IVideoPicker
	{
		Task<string> GetVideoFileAsync();
	}
}
