using CustomMonitoringFilter.Obfuscator;
using Nevatech.Sentinet.Entities;
using Nevatech.Sentinet.Monitoring.Filters;
using System.Text.RegularExpressions;

namespace CustomMonitoringFilter
{
    /// <summary>
    /// Custom monitoring filter for Sentinet that obfuscates sensitive data in monitored messages.
    /// Implements IMonitoringFilter to intercept and sanitize monitoring records before they are stored.
    /// </summary>
    public class Obfuscation : IMonitoringFilter
    {
        /// <summary>
        /// List of field names to obfuscate in JSON payloads (case-insensitive matching).
        /// </summary>
        private static readonly string[] FieldsNameList =
        [
            "Client_secret",
            "Password",
            "Image",
            "File",
            "FileBody"
        ];

        /// <summary>
        /// List of regex patterns to match and obfuscate field values in payloads.
        /// </summary>
        private static readonly Regex[] FieldsNameRegexList =
        [
            new(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled)
        ];

        /// <summary>
        /// List of field names to completely remove from JSON payloads.
        /// Currently empty but can be configured to remove specific fields.
        /// </summary>
        private static readonly string[] FieldNameToRemoveList = [];

        /// <summary>
        /// Imports configuration for the monitoring filter.
        /// Currently not implemented as configuration is handled via static fields.
        /// </summary>
        /// <param name="configuration">Configuration string (not used)</param>
        public void ImportConfiguration(string configuration)
        {
            // Configuration is handled via static field declarations
            // This method can be extended to support dynamic configuration
        }

        /// <summary>
        /// Processes monitoring transaction properties.
        /// Currently passes through without modification.
        /// </summary>
        /// <param name="property">The monitoring property to process</param>
        /// <param name="context">The monitoring filter context</param>
        /// <param name="nextFilter">The next filter in the chain</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task WritePropertyAsync(MonitoringTransactionProperty property, MonitoringFilterContext context, MonitoringFilterBase nextFilter)
         => nextFilter.WritePropertyAsync(property, context);

        /// <summary>
        /// Processes and obfuscates sensitive data in monitoring records.
        /// Obfuscates message content and transport headers before passing to the next filter.
        /// </summary>
        /// <param name="record">The monitoring record to process</param>
        /// <param name="context">The monitoring filter context</param>
        /// <param name="nextFilter">The next filter in the chain</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task WriteRecordAsync(MonitoringRecord record, MonitoringFilterContext context, MonitoringFilterBase nextFilter)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(record);

                // Obfuscate message payload if present
                string? mimeType = null;
                if (!string.IsNullOrEmpty(record.TransportData))
                {
                    mimeType = HeaderObfuscator.GetHeader("Content-Type", record.TransportData);
                }
                if (!string.IsNullOrEmpty(record.RecordedMessage))
                {
                    record.RecordedMessage = PayloadObfuscator.Obfuscate(
                        record.RecordedMessage,
                        mimeType,
                        FieldsNameList,
                        FieldsNameRegexList,
                        FieldNameToRemoveList
                    );
                }

                // Obfuscate transport headers (particularly Authorization header)
                if (!string.IsNullOrEmpty(record.TransportData))
                {
                    record.TransportData = HeaderObfuscator.Obfuscate(record.TransportData);
                }
            }
            catch
            {
                // Swallow exceptions to prevent obfuscation failures from breaking monitoring
                // The record will be passed through without obfuscation
                // Consider logging this exception in production environments
            }

            return nextFilter.WriteRecordAsync(record, context);
        }

        /// <summary>
        /// Processes monitoring transactions.
        /// Currently passes through without modification.
        /// </summary>
        /// <param name="transaction">The monitoring transaction to process</param>
        /// <param name="context">The monitoring filter context</param>
        /// <param name="nextFilter">The next filter in the chain</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task WriteTransactionAsync(MonitoringTransaction transaction, MonitoringFilterContext context, MonitoringFilterBase nextFilter)
            => nextFilter.WriteTransactionAsync(transaction, context);
    }
}
