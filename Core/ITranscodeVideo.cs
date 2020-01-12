using System.Threading.Tasks;

namespace StudioFreesia.Vivideo.Core
{
    public interface ITranscodeVideo
    {
        void Transcode(TranscodeQueue queue);
    }
}
