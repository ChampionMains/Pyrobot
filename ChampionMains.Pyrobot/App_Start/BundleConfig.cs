﻿using System.Web.Optimization;

namespace ChampionMains.Pyrobot
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        // https://github.com/Taritsyn/BundleTransformer/blob/5310f4c3e56ba823b657b59ba7f8d4c8992c9c4a/samples/BundleTransformer.Sample.AspNet4.Mvc4/App_Start/BundleConfig.cs
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/Bundles/dialogPolyfill").Include(
                "~/node_modules/dialog-polyfill/dialog-polyfill.css"));

            bundles.Add(new ScriptBundle("~/Bundles/polyfills").Include(
                "~/node_modules/mdn-polyfills/Object.assign.js",
                "~/node_modules/mdn-polyfills/Object.values.js",
                "~/node_modules/mdn-polyfills/Object.entries.js",
                "~/node_modules/mdn-polyfills/Element.prototype.closest.js",
                "~/node_modules/native-promise-only/npo.js",
                "~/node_modules/whatwg-fetch/dist/fetch.umd.js",
                "~/node_modules/dialog-polyfill/dialog-polyfill.js",
                "~/content/js/_mixins.js"));

            bundles.Add(new ScriptBundle("~/Bundles/npmScripts").Include(
                "~/node_modules/material-design-lite/material.js",
                "~/node_modules/mdl-ext/lib/mdl-ext.js",
                "~/node_modules/getmdl-select/src/js/getmdl-select.js",
                "~/node_modules/fitty/dist/fitty.min.js"));

            bundles.Add(new ScriptBundle("~/Bundles/vue").Include(
                "~/node_modules/vue/dist/vue.js",
                "~/node_modules/numeral/numeral.js",
                "~/node_modules/vue-numeral-filter/dist/vue-numeral-filter.min.js",
                "~/node_modules/moment/moment.js",
                "~/node_modules/vue-moment/dist/vue-moment.js"));

//            bundles.UseCdn = true;
//
//            var nullBuilder = new NullBuilder();
//            var nullOrderer = new NullOrderer();
//
//            // Replace a default bundle resolver in order to the debugging HTTP handler
//            // can use transformations of the corresponding bundle
//            BundleResolver.Current = new CustomBundleResolver();
//
//            var commonStylesBundle = new CustomStyleBundle("~/Bundles/CommonStyles");
//            commonStylesBundle.Include(
//                "~/Content/Fonts.css",
//                "~/Content/Site.css",
//                "~/Content/BundleTransformer.css",
//                "~/AlternativeContent/css/TestCssComponentsPaths.css",
//                "~/Content/themes/base/jquery.ui.core.css",
//                "~/Content/themes/base/jquery.ui.theme.css",
//                "~/Content/themes/base/jquery.ui.resizable.css",
//                "~/Content/themes/base/jquery.ui.button.css",
//                "~/Content/themes/base/jquery.ui.dialog.css",
//                "~/Content/TestTranslators.css",
//                "~/Content/less/TestLess.less",
//                "~/AlternativeContent/less/LessIcons.less",
//                "~/Content/sass/TestSass.sass",
//                "~/Content/scss/TestScss.scss");
//            commonStylesBundle.Orderer = nullOrderer;
//            bundles.Add(commonStylesBundle);
//
//            var modernizrBundle = new CustomScriptBundle("~/Bundles/Modernizr");
//            modernizrBundle.Include("~/Scripts/modernizr-2.*");
//            modernizrBundle.Orderer = nullOrderer;
//            bundles.Add(modernizrBundle);
//
//            var jQueryBundle = new CustomScriptBundle("~/Bundles/Jquery",
//                "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-2.2.1.min.js");
//            jQueryBundle.Include("~/Scripts/jquery-{version}.js");
//            jQueryBundle.Orderer = nullOrderer;
//            jQueryBundle.CdnFallbackExpression = "window.jquery";
//            bundles.Add(jQueryBundle);
//
//            var commonScriptsBundle = new CustomScriptBundle("~/Bundles/CommonScripts");
//            commonScriptsBundle.Include(
//                "~/Scripts/MicrosoftAjax.js",
//                "~/Scripts/jquery-ui-{version}.js",
//                "~/Scripts/jquery.validate.js",
//                "~/Scripts/jquery.validate.unobtrusive.js",
//                "~/Scripts/jquery.unobtrusive-ajax.js",
//                "~/Scripts/knockout-3.*",
//                "~/Scripts/coffee/TestCoffeeScript.coffee",
//                "~/Scripts/coffee/TestLiterateCoffeeScript.litcoffee",
//                "~/Scripts/coffee/TestCoffeeScriptMarkdown.coffee.md",
//                "~/Scripts/ts/TranslatorBadge.ts",
//                "~/Scripts/ts/ColoredTranslatorBadge.ts",
//                "~/Scripts/ts/TestTypeScript.ts");
//            commonScriptsBundle.Orderer = nullOrderer;
//            bundles.Add(commonScriptsBundle);
//
//            var commonTemplatesBundle = new CustomScriptBundle("~/Bundles/CommonTemplates");
//            commonTemplatesBundle.Include(
//                "~/Scripts/hogan/template-{version}.js",
//                "~/Scripts/hogan/HoganTranslatorBadge.mustache",
//                "~/Scripts/hogan/TestHogan.js",
//                "~/Scripts/handlebars/handlebars.runtime.js",
//                "~/Scripts/handlebars/HandlebarsHelpers.js",
//                "~/Scripts/handlebars/HandlebarsTranslatorBadge.handlebars",
//                "~/Scripts/handlebars/TestHandlebars.js");
//            commonTemplatesBundle.Orderer = nullOrderer;
//            bundles.Add(commonTemplatesBundle);
//
//            var jqueryUiStylesDirectoryBundle = new Bundle("~/Bundles/JqueryUiStylesDirectory")
//            {
//                Builder = nullBuilder
//            };
//            jqueryUiStylesDirectoryBundle.IncludeDirectory("~/Content/themes/base/", "*.css");
//            jqueryUiStylesDirectoryBundle.Transforms.Add(new StyleTransformer(
//                new[] { "*.all.css", "jquery.ui.base.css" }));
//            bundles.Add(jqueryUiStylesDirectoryBundle);
//
//            var scriptsDirectoryBundle = new Bundle("~/Bundles/ScriptsDirectory")
//            {
//                Builder = nullBuilder
//            };
//            scriptsDirectoryBundle.IncludeDirectory("~/Scripts/", "*.js", true);
//            scriptsDirectoryBundle.Transforms.Add(new ScriptTransformer(
//                new[] { "*.all.js", "_references.js" }));
//            bundles.Add(scriptsDirectoryBundle);
        }
    }
}