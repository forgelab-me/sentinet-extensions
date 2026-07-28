using System.Text.RegularExpressions;

namespace CustomMonitoringFilter.Obfuscator
{
    /// <summary>
    /// Handles obfuscation of sensitive data in plain text using regex patterns.
    /// </summary>
    internal sealed class PlaintextObfuscator
    {
        private readonly string obfuscatedValue;
        private readonly Regex[] regexesToReplace;

        /// <summary>
        /// Initializes a new instance of the PlaintextObfuscator class.
        /// </summary>
        /// <param name="obfuscatedValue">The replacement value for matched patterns</param>
        /// <param name="regexesToReplace">Array of regex patterns to match and replace in the text</param>
        public PlaintextObfuscator(string obfuscatedValue, Regex[] regexesToReplace)
        {
            this.obfuscatedValue = obfuscatedValue;
            this.regexesToReplace = regexesToReplace;
        }

        /// <summary>
        /// Obfuscates sensitive data in the provided text by replacing regex matches.
        /// </summary>
        /// <param name="text">The text to obfuscate</param>
        /// <returns>The obfuscated text, or original text if an error occurs</returns>
        public string Obfuscate(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                // Apply each regex pattern to replace sensitive data
                foreach (var regexToReplace in regexesToReplace)
                {
                    text = regexToReplace.Replace(text, obfuscatedValue);
                }
                return text;
            }
            catch
            {
                // Swallow exception to prevent obfuscation failures from breaking the main flow
                // Return original text if obfuscation fails
                return text;
            }
        }
    }
}
