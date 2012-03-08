using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;

// http://stackoverflow.com/questions/5110028/add-css-or-js-files-to-layout-head-from-views-or-partial-views

namespace MvcApplication2.Infrastructure
    {
        public static class HtmlHelperExtensions
            {
            public static AssetsHelper Assets(this HtmlHelper htmlHelper)
                {
                return AssetsHelper.GetInstance(htmlHelper);
                }
            }

        public class AssetsHelper
            {
            public static AssetsHelper GetInstance(HtmlHelper htmlHelper)
                {
                const string instanceKey = "AssetsHelperInstance";

                var context = htmlHelper.ViewContext.HttpContext;
                if (context == null) return null;

                var assetsHelper = (AssetsHelper)context.Items[instanceKey];

                if (assetsHelper == null)
                    context.Items.Add(instanceKey, assetsHelper = new AssetsHelper());

                return assetsHelper;
                }

            public ItemRegistrar Styles { get; private set; }
            public ItemRegistrar Scripts { get; private set; }

            public AssetsHelper()
                {
                Styles = new ItemRegistrar(ItemRegistrarFormatters.StyleFormat);
                Scripts = new ItemRegistrar(ItemRegistrarFormatters.ScriptFormat);
                }
            }

        public class ItemRegistrar
        {
            private readonly string _format;
            private readonly IList<string> _items;

            public ItemRegistrar(string format)
                {
                _format = format;
                _items = new List<string>();
                }

            public ItemRegistrar Add(string url)
            {
                if (!_items.Contains(url))
                    _items.Insert(0, url);

                return this;
            }

            public IHtmlString Render()
            {
                var sb = new StringBuilder();

                foreach (var item in _items)
                {
                    var fmt = string.Format(_format, item);
                    sb.AppendLine(fmt);
                }

                return new HtmlString(sb.ToString());
            }
        }

        public class ItemRegistrarFormatters
            {
            public const string StyleFormat = "<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />";
            public const string ScriptFormat = "<script src=\"{0}\" type=\"text/javascript\"></script>";
            }     
    }
