namespace Website.Routes;
public static class Routes
{
    public static void MapAppRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "user-profile",
            pattern: "User/{username}",
            defaults: new { controller = "User", action = "Profile" });
        app.MapControllerRoute(
            name: "ingredient",
            pattern: "Ingredients/{action=Index}/{id?}",
            defaults: new { controller = "Ingredient" });
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}