using System;
using System.ComponentModel.Composition;
using System.IO;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PowerSite.Builtin.Renderers
{
    [Export(typeof(IRenderer))]
    [ExportMetadata("Extension", "cshtml")]
    public class RazorRenderer : IRenderer
    {
        public string Render(string template, object data)
        {
            string result = null;

            var config = new TemplateServiceConfiguration() { Resolver = new TemplateResolver() };

            config.Namespaces.Add("System.IO");
            config.Namespaces.Add("RazorEngine.Text");

            using (var service = new TemplateService(config))
            {
                Razor.SetTemplateService(service);

                var cacheName = template.GetHashCode().ToString();

                try
                {
                    result = Razor.Parse(template, data, cacheName);
                }
                catch (TemplateCompilationException e)
                {
                    foreach (var error in e.Errors)
                    {
                        Console.Error.WriteLine(error);
                    }
                }
            }

            return result;
        }

        private class TemplateResolver : ITemplateResolver
        {
            public string Resolve(string name)
            {
                var id = Path.GetFileNameWithoutExtension(name);

                var extension = Path.GetExtension(name).TrimStart('.');

                //var layout = RenderingState.Current.Layouts[id];

                //if (!layout.Extension.Equals(String.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
                //{
                //    // TODO: throw new exception.
                //}

                //return layout.Content;
                return "You";
            }
        }
    }
}
