namespace Website.Routes;
public static class Routes
{
    public static void MapAppRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}