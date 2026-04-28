// App_Start/BundleConfig.cs
using System.Web.Optimization;

namespace ModuloSST_HSLV
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery principal
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            // Validación del lado cliente
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Bootstrap 5 — DEBE ser bootstrap.bundle.js (incluye Popper.js)
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.js"));

            // Estilos
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));

            // Forzar que los bundles funcionen en modo Debug
            BundleTable.EnableOptimizations = false;
        }
    }
}