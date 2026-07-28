using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CustomMonitoringFilter.Obfuscator
{
    /// <summary>
    /// Provides functionality to obfuscate sensitive data in various payload formats.
    /// Supports JSON, XML, and plain text obfuscation.
    /// </summary>
    internal static class PayloadObfuscator
    {
        private const string ObfuscatedValue = "***OBFUSCATED***";

        /// <summary>
        /// Obfuscates sensitive fields in a payload based on its MIME type.
        /// </summary>
        /// <param name="clearText">The payload content to obfuscate</param>
        /// <param name="mimeType">The MIME type of the payload (can be null for auto-detection)</param>
        /// <param name="fieldNamesToObfuscate">Array of field names to obfuscate</param>
        /// <param name="fieldNameRegexsToObfuscate">Array of regex patterns to match field names</param>
        /// <param name="fieldNamesToRemove">Array of field names to completely remove</param>
        /// <returns>The obfuscated payload string</returns>
        public static string Obfuscate(
            string clearText,
            string? mimeType,
            string[] fieldNamesToObfuscate,
            Regex[] fieldNameRegexsToObfuscate,
            string[] fieldNamesToRemove)
        {
            if (string.IsNullOrEmpty(clearText))
                return clearText;

            // First pass: obfuscate using regex patterns for plain text
            clearText = ObfuscatePlaintext(clearText, fieldNameRegexsToObfuscate);

            // Determine MIME type if not provided
            mimeType ??= MimeTypes.Determine(clearText);

            // Skip JSON obfuscation for XML and plain text
            if (MimeTypes.IsXml(mimeType) || MimeTypes.IsTxt(mimeType))
                return clearText;

            // Apply JSON-specific obfuscation
            if (MimeTypes.IsJson(mimeType))
                return ObfuscateJson(clearText, fieldNamesToObfuscate, fieldNameRegexsToObfuscate, fieldNamesToRemove);

            // Recursive call with determined MIME type
            return Obfuscate(clearText, MimeTypes.Determine(clearText), fieldNamesToObfuscate, fieldNameRegexsToObfuscate, fieldNamesToRemove);
        }

        /// <summary>
        /// Obfuscates sensitive data in plain text using regex pattern matching.
        /// </summary>
        /// <param name="messagePlaintext">The plain text message to obfuscate</param>
        /// <param name="regexsToObfuscate">Array of regex patterns to match and replace</param>
        /// <returns>The obfuscated plain text, or original text if an error occurs</returns>
        private static string ObfuscatePlaintext(string messagePlaintext, Regex[] regexsToObfuscate)
        {
            try
            {
                var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexsToObfuscate);
                return obfuscator.Obfuscate(messagePlaintext);
            }
            catch
            {
                // Return original text if obfuscation fails
                return messagePlaintext;
            }
        }

        /// <summary>
        /// Obfuscates sensitive fields in JSON payloads.
        /// Handles both JSON objects and JSON arrays.
        /// </summary>
        /// <param name="messageJSON">The JSON string to obfuscate</param>
        /// <param name="fieldNamesToObfuscate">Array of field names to obfuscate</param>
        /// <param name="fieldNameRegexsToObfuscate">Array of regex patterns to match field names</param>
        /// <param name="fieldNamesToRemove">Array of field names to completely remove</param>
        /// <returns>The obfuscated JSON string, or original JSON if an error occurs</returns>
        private static string ObfuscateJson(
            string messageJSON,
            string[] fieldNamesToObfuscate,
            Regex[] fieldNameRegexsToObfuscate,
            string[] fieldNamesToRemove)
        {
            try
            {
                var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldNamesToObfuscate, fieldNameRegexsToObfuscate, fieldNamesToRemove);
                var token = JToken.Parse(messageJSON);

                // Handle JSON objects
                if (token is JObject)
                {
                    var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageJSON)!;
                    obfuscator.Obfuscate(jsonDoc);
                    return JsonConvert.SerializeObject(jsonDoc);
                }

                // Handle JSON arrays
                if (token is JArray)
                {
                    var jsonDocs = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(messageJSON)!;
                    foreach (var doc in jsonDocs)
                    {
                        obfuscator.Obfuscate(doc);
                    }
                    return JsonConvert.SerializeObject(jsonDocs);
                }

                throw new InvalidOperationException("Unsupported JSON structure.");
            }
            catch
            {
                // Return original JSON if obfuscation fails
                return messageJSON;
            }
        }
    }
}
