using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomMonitoringFilter.Obfuscator
{
    /// <summary>
    /// Handles obfuscation of sensitive data in HTTP headers, particularly authorization tokens.
    /// </summary>
    internal static class HeaderObfuscator
    {
        // Compiled regex for JWT token pattern matching
        private static readonly Regex JwtTokenRegex = new(
            @"(eyJ[A-Za-z0-9-_+\/=]+\.ey[A-Za-z0-9-_+\/=]+\.)([A-Za-z0-9-_+\/=]+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Obfuscates authorization headers in transport data.
        /// Supports JWT, Bearer, Negotiate, NTLM, and Basic authentication schemes.
        /// </summary>
        /// <param name="transportData">The HTTP transport data containing headers</param>
        /// <returns>The transport data with obfuscated authorization headers</returns>
        public static string Obfuscate(string transportData)
        {
            if (string.IsNullOrEmpty(transportData))
                return transportData;

            var authorization = GetHeader("Authorization", transportData);
            if (string.IsNullOrEmpty(authorization))
                return transportData;

            // Split authorization header to get the value part
            var parts = authorization.Split(':', 2);
            if (parts.Length < 2)
                return transportData;

            var token = parts[1].Trim();
            var obfuscateToken = ObfuscateAuthToken(token);

            // Replace original token with obfuscated version
            return transportData.Replace(token, obfuscateToken);
        }

        /// <summary>
        /// Extracts a specific header value from HTTP transport data.
        /// </summary>
        /// <param name="headerName">The name of the header to find</param>
        /// <param name="transportData">The HTTP transport data to search</param>
        /// <returns>The header line if found, null otherwise</returns>
        public static string? GetHeader(string headerName, string transportData)
        {
            if (string.IsNullOrEmpty(transportData) || string.IsNullOrEmpty(headerName))
                return null;

            using var srTransportData = new StringReader(transportData);
            string? line;
            while ((line = srTransportData.ReadLine()) != null)
            {
                if (line.Contains(headerName, StringComparison.OrdinalIgnoreCase))
                {
                    return line;
                }
            }

            return null;
        }

        /// <summary>
        /// Obfuscates different types of authentication tokens.
        /// </summary>
        /// <param name="token">The authentication token to obfuscate</param>
        /// <returns>The obfuscated token</returns>
        private static string ObfuscateAuthToken(string token)
        {
            // Check if it's a JWT token and strip the signature
            if (JwtTokenRegex.IsMatch(token))
            {
                return StripSignatureFromJWTToken(token);
            }

            // Handle different authentication schemes
            if (token.Contains("Bearer", StringComparison.OrdinalIgnoreCase))
                return "Bearer *******";

            if (token.Contains("Negotiate", StringComparison.OrdinalIgnoreCase))
                return "Negotiate *******";

            if (token.Contains("NTLM", StringComparison.OrdinalIgnoreCase))
                return "NTLM *******";

            if (token.Contains("Basic", StringComparison.OrdinalIgnoreCase))
                return "Basic *******";

            // Default: return the original token if no known scheme matches
            return token;
        }

        /// <summary>
        /// Strips the signature from a JWT token and replaces it with a SHA256 hash.
        /// Preserves the header and payload while obfuscating the signature.
        /// </summary>
        /// <param name="token">The JWT token to process</param>
        /// <returns>The JWT token with obfuscated signature</returns>
        private static string StripSignatureFromJWTToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return token;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return token;

                var signature = parts[2];
                var obfuscateSignature = $"<SHA256>{GetHashSha256(signature)}</SHA256>";

                // Replace only the signature part
                return $"{parts[0]}.{parts[1]}.{obfuscateSignature}";
            }
            catch
            {
                // Return original token if processing fails
                return token;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of a string.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>The hexadecimal representation of the SHA256 hash</returns>
        private static string GetHashSha256(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(bytes);

            // Convert to hexadecimal string
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
