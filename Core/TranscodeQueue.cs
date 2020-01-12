namespace StudioFreesia.Vivideo.Core
{
    public class TranscodeQueue
    {
        public string Input { get; }
        public string Output { get; }

        public TranscodeQueue(string input, string output)
            => (this.Input, this.Output) = (input, output);
    }
}
