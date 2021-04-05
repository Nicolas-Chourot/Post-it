using System.Web;
using System.Web.Optimization;

namespace EFA_DEMO
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                       "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/autoComplete").Include(
                      "~/Scripts/autoComplete.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/imageUploader").Include(
                        "~/Scripts/imageUploader.js"));

            bundles.Add(new ScriptBundle("~/bundles/partialRefresh").Include(
                        "~/Scripts/partialRefresh.js"));

            // Utilisez la version de développement de Modernizr pour le développement et l'apprentissage. Puis, une fois
            // prêt pour la production, utilisez l'outil de génération à l'adresse https://modernizr.com pour sélectionner uniquement les tests dont vous avez besoin.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/bootstrap.css",
                     "~/Content/site.css",
                     "~/Content/themes/base/jquery-ui.css",
                      "~/Content/themes/ui-darkness/jquery-ui.all.css",
                     "~/Content/themes/ui-darkness/jquery-ui.base.css",
                     "~/Content/themes/ui-darkness/jquery-ui.dialog.css",
                     "~/Content/themes/ui-darkness/jquery-ui.theme.css",
                     "~/Content/themes/ui-darkness/datepicker.css"
                     ));
        }
    }
}
