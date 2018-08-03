using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSite.DataModel;

namespace PowerSite
{
    /// <summary>
    /// Interface implemented by classes that can render templates into content.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Renders a template with the provided data.
        /// </summary>
        /// <param name="siteKey">Unique site key (e.g. the full path to the site root).</param>
        /// <param name="template">Template to render.</param>
        /// <param name="data">Data provided to template.</param>
        /// <returns>Rendered template content.</returns>
        string Render(string siteKey, NamedContentBase template, dynamic data);
    }

    public class RedererMetadata
    {
        public String Extension { get; set; }
    }
}
