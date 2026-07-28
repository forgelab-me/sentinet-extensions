using Newtonsoft.Json;
using System.Xml;

namespace CustomMonitoringFilter.Obfuscator
{
    /// <summary>
    /// Provides MIME type detection and validation utilities.
    /// </summary>
    internal static class MimeTypes
    {
        private static readonly string[] JsonTypes = ["application/json", "text/json"];
        private static readonly string[] XmlTypes = ["application/xml", "application/soap+xml", "application/atom+xml", "text/xml"];
        private static readonly string[] TxtTypes = ["text/plain", "text/html"];

        /// <summary>
        /// Determines if the given MIME type represents JSON content.
        /// </summary>
        /// <param name="mimeType">The MIME type to check</param>
        /// <returns>True if the MIME type is JSON, false otherwise</returns>
        public static bool IsJson(string mimeType)
            => JsonTypes.Any(jsonType => mimeType.Contains(jsonType, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Determines if the given MIME type represents XML content.
        /// </summary>
        /// <param name="mimeType">The MIME type to check</param>
        /// <returns>True if the MIME type is XML, false otherwise</returns>
        public static bool IsXml(string mimeType)
            => XmlTypes.Any(xmlType => mimeType.Contains(xmlType, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Determines if the given MIME type represents plain text or HTML content.
        /// </summary>
        /// <param name="mimeType">The MIME type to check</param>
        /// <returns>True if the MIME type is text or HTML, false otherwise</returns>
        public static bool IsTxt(string mimeType)
            => TxtTypes.Any(txtType => mimeType.Contains(txtType, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Attempts to automatically determine the MIME type of a message by parsing it.
        /// Tries XML first, then JSON, and defaults to plain text.
        /// </summary>
        /// <param name="message">The message content to analyze</param>
        /// <returns>The determined MIME type: "text/xml", "text/json", or "text/plain"</returns>
        public static string Determine(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "text/plain";

            // Try to parse as XML
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(message);
                return "text/xml";
            }
            catch
            {
                // Not valid XML
            }

            // Try to parse as JSON
            try
            {
                JsonConvert.DeserializeObject(message);
                return "text/json";
            }
            catch
            {
                // Not valid JSON
            }

            // Default to plain text
            return "text/plain";
        }
    }
}
