namespace Website.Routes;
public static class Routes
{
    public static void MapAppRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "user-actions",
            pattern: "User/{action}/{username?}",
            defaults: new { controller = "User" });
        app.MapControllerRoute(
            name: "user-profile",
            pattern: "User/{username}",
            defaults: new { controller = "User", action = "Profile" });
        app.MapControllerRoute(
            name: "ingredient",
            pattern: "Ingredients/{action=Index}/{id?}",
            defaults: new { controller = "Ingredient" });
        app.MapControllerRoute(
            name: "collections",
            pattern: "Collections/{action=Index}/{id?}",
            defaults: new { controller = "Collections" });
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}