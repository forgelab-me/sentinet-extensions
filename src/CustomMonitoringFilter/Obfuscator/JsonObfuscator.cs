using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CustomMonitoringFilter.Obfuscator
{
    /// <summary>
    /// Handles obfuscation of sensitive fields within JSON documents.
    /// Supports field removal, value replacement, and recursive traversal of nested objects.
    /// </summary>
    internal sealed class JsonObfuscator
    {
        private readonly string obfuscatedValue;

        // Using HashSet for O(1) lookup performance instead of array scanning
        private readonly HashSet<string> fieldsToReplace;
        private readonly Regex[] regexesToReplace;
        private readonly HashSet<string> fieldNamesToRemove;

        /// <summary>
        /// Initializes a new instance of the JsonObfuscator class.
        /// </summary>
        /// <param name="obfuscatedValue">The replacement value for obfuscated fields (e.g., "***OBFUSCATED***")</param>
        /// <param name="fieldsToReplace">Array of exact field names to obfuscate (case-insensitive)</param>
        /// <param name="regexesToReplace">Array of regex patterns to match field names for obfuscation</param>
        /// <param name="fieldNamesToRemove">Array of field names to completely remove from the JSON</param>
        public JsonObfuscator(string obfuscatedValue, string[] fieldsToReplace, Regex[] regexesToReplace, string[] fieldNamesToRemove)
        {
            this.obfuscatedValue = obfuscatedValue;
            // Convert arrays to HashSet for O(1) lookup performance
            this.fieldsToReplace = new HashSet<string>(fieldsToReplace, StringComparer.InvariantCultureIgnoreCase);
            this.regexesToReplace = regexesToReplace;
            this.fieldNamesToRemove = new HashSet<string>(fieldNamesToRemove, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Recursively obfuscates sensitive fields in a JSON document dictionary.
        /// Removes fields marked for deletion and replaces values of fields marked for obfuscation.
        /// </summary>
        /// <param name="jsonDoc">Dictionary representation of a JSON object to obfuscate</param>
        public void Obfuscate(Dictionary<string, object> jsonDoc)
        {
            if (jsonDoc == null)
                return;

            try
            {
                // Use a list to collect keys for modification to avoid modifying collection during enumeration
                List<string> keysToRemove = [];
                List<string> keysToObfuscate = [];
                List<KeyValuePair<string, object>> nestedObjects = [];

                // Single pass through the dictionary to categorize all keys
                foreach (var kvp in jsonDoc)
                {
                    if (ShouldRemove(kvp.Key))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                    else if (ShouldObfuscate(kvp.Key))
                    {
                        keysToObfuscate.Add(kvp.Key);
                    }
                    else if (kvp.Value is JObject or JArray)
                    {
                        // Track nested structures for recursive processing
                        nestedObjects.Add(kvp);
                    }
                }

                // Remove fields marked for deletion
                foreach (var key in keysToRemove)
                {
                    jsonDoc.Remove(key);
                }

                // Obfuscate field values marked for replacement
                foreach (var key in keysToObfuscate)
                {
                    jsonDoc[key] = obfuscatedValue;
                }

                // Recursively process nested objects and arrays
                foreach (var field in nestedObjects)
                {
                    ProcessNestedStructure(jsonDoc, field.Key, field.Value);
                }
            }
            catch (Exception e)
            {
                // Swallow exception to prevent obfuscation failures from breaking the main flow
                // In production, consider logging this with a proper logger
                var message = JsonConvert.SerializeObject(e);
                // TODO: Optionally log the exception details
            }
        }

        /// <summary>
        /// Processes nested JSON structures (objects and arrays) recursively.
        /// </summary>
        /// <param name="parentDoc">The parent dictionary containing the nested structure</param>
        /// <param name="key">The key in the parent dictionary</param>
        /// <param name="value">The nested structure (JObject or JArray)</param>
        private void ProcessNestedStructure(Dictionary<string, object> parentDoc, string key, object value)
        {
            if (value is JObject jObject)
            {
                // Deserialize and recursively obfuscate nested object
                var child = JsonConvert.DeserializeObject<Dictionary<string, object>>(jObject.ToString());
                if (child != null)
                {
                    Obfuscate(child);
                    parentDoc[key] = child;
                }
            }
            else if (value is JArray jArray)
            {
                // Process each element in the array
                var arrayElements = new List<object>();
                foreach (var item in jArray)
                {
                    if (item is JObject itemObject)
                    {
                        // Recursively obfuscate objects within the array
                        var childDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(itemObject.ToString());
                        if (childDict != null)
                        {
                            Obfuscate(childDict);
                            arrayElements.Add(childDict);
                        }
                    }
                    else
                    {
                        // Keep non-object array elements as-is
                        arrayElements.Add(item);
                    }
                }
                parentDoc[key] = arrayElements;
            }
        }

        /// <summary>
        /// Determines if a field name should have its value obfuscated.
        /// Matches against exact field names (case-insensitive) and regex patterns.
        /// </summary>
        /// <param name="name">The field name to check</param>
        /// <returns>True if the field should be obfuscated, false otherwise</returns>
        private bool ShouldObfuscate(string name)
        {
            // HashSet.Contains is O(1) vs Array.Contains which is O(n)
            if (fieldsToReplace.Contains(name))
                return true;

            // Check regex patterns (unavoidably O(n) but typically small n)
            foreach (var regex in regexesToReplace)
            {
                if (regex.IsMatch(name))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a field should be completely removed from the JSON.
        /// Performs case-insensitive matching.
        /// </summary>
        /// <param name="name">The field name to check</param>
        /// <returns>True if the field should be removed, false otherwise</returns>
        private bool ShouldRemove(string name)
          => fieldNamesToRemove.Contains(name); // O(1) lookup with HashSet
    }
}
