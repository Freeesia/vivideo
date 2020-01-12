using System.Threading.Tasks;

namespace StudioFreesia.Vivideo.Core
{
    public interface ITranscodeVideo
    {
        Task<string> Transcode(string path);
    }
}
