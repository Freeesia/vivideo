using System.Linq;

namespace StudioFreesia.Vivideo.Server.Extensions
{
    public static class SystemExtentions
    {
        public static bool Or(this string mine, params string[] targets)
            => targets.Contains(mine);
    }
}
