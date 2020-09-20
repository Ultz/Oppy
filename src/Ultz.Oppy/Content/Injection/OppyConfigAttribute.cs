using System;

namespace Ultz.Oppy.Content.Injection
{
    /// <summary>
    /// Instructs the needle to inject a JSON configuration value.
    /// </summary>
    public class OppyConfigAttribute : Attribute
    {
        public OppyConfigAttribute(string jPath) => JPath = jPath;
        public string JPath { get; }
    }
}