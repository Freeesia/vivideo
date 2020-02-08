using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace StudioFreesia.Vivideo.Server.ComponentModel
{
    public class HangfireDashbordAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
            // => context.GetHttpContext().User.Identity.IsAuthenticated;
            => true;
    }
}
