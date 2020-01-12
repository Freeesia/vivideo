using System.Threading.Tasks;

namespace StudioFreesia.Vivideo.Core
{
    interface ITranscodeVideo
    {
        Task<string> Transcode(string path);
    }
}
