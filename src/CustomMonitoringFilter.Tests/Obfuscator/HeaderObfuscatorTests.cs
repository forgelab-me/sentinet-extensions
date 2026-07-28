using System.Reflection;
using CustomMonitoringFilter.Obfuscator;

namespace CustomMonitoringFilter.Tests.Obfuscator
{
    public class HeaderObfuscatorTests
    {
        private const string SampleJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        #region GetHeader Tests

        [Fact]
        public void GetHeader_Should_Find_Authorization_Header()
        {
            // Arrange
            var transportData = @"POST /api/test HTTP/1.1
Host: example.com
Authorization: Bearer test-token-123
Content-Type: application/json";

            // Act
            var result = HeaderObfuscator.GetHeader("Authorization", transportData);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Authorization");
            result.Should().Contain("Bearer test-token-123");
        }

        [Fact]
        public void GetHeader_Should_Be_Case_Insensitive()
        {
            // Arrange
            var transportData = @"POST /api/test HTTP/1.1
authorization: Bearer test-token-123";

            // Act
            var result = HeaderObfuscator.GetHeader("Authorization", transportData);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Bearer test-token-123");
        }

        [Fact]
        public void GetHeader_Should_Return_Null_When_Header_Not_Found()
        {
            // Arrange
            var transportData = @"POST /api/test HTTP/1.1
Host: example.com
Content-Type: application/json";

            // Act
            var result = HeaderObfuscator.GetHeader("Authorization", transportData);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHeader_Should_Return_Null_For_Empty_TransportData()
        {
            // Act
            var result = HeaderObfuscator.GetHeader("Authorization", "");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHeader_Should_Return_Null_For_Null_TransportData()
        {
            // Act
            var result = HeaderObfuscator.GetHeader("Authorization", null!);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Obfuscate Tests - JWT Tokens

        [Fact]
        public void Obfuscate_Should_Obfuscate_JWT_Bearer_Token()
        {
            // Arrange
            var transportData = $@"POST /api/test HTTP/1.1
Host: example.com
Authorization: Bearer {SampleJwtToken}
Content-Type: application/json";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotContain(SampleJwtToken);
            result.Should().Contain("<SHA256>");
            result.Should().Contain("</SHA256>");
            result.Should().Contain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"); // Header should remain
            result.Should().Contain("eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ"); // Payload should remain
        }

        [Fact]
        public void Obfuscate_Should_Hash_JWT_Signature_Consistently()
        {
            // Arrange
            var transportData = $@"Authorization: Bearer {SampleJwtToken}";

            // Act
            var result1 = HeaderObfuscator.Obfuscate(transportData);
            var result2 = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result1.Should().Be(result2);
        }

        [Fact]
        public void Obfuscate_Should_Handle_JWT_Without_Bearer_Prefix()
        {
            // Arrange
            var transportData = $@"Authorization: {SampleJwtToken}";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("<SHA256>");
            result.Should().Contain("</SHA256>");
        }

        #endregion

        #region Obfuscate Tests - Other Auth Schemes

        [Fact]
        public void Obfuscate_Should_Obfuscate_Bearer_Token()
        {
            // Arrange
            var transportData = @"POST /api/test HTTP/1.1
Authorization: Bearer abc123xyz456";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Bearer *******");
            result.Should().NotContain("abc123xyz456");
        }

        [Fact]
        public void Obfuscate_Should_Obfuscate_Basic_Auth()
        {
            // Arrange
            var transportData = @"Authorization: Basic dXNlcjpwYXNzd29yZA==";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Basic *******");
            result.Should().NotContain("dXNlcjpwYXNzd29yZA==");
        }

        [Fact]
        public void Obfuscate_Should_Obfuscate_NTLM_Auth()
        {
            // Arrange
            var transportData = @"Authorization: NTLM TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("NTLM *******");
            result.Should().NotContain("TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==");
        }

        [Fact]
        public void Obfuscate_Should_Obfuscate_Negotiate_Auth()
        {
            // Arrange
            var transportData = @"Authorization: Negotiate YIIFzgYJKoZIhvcSAQICAQBuggW9MIIFuaADAgEFoQMCAQ4=";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Negotiate *******");
            result.Should().NotContain("YIIFzgYJKoZIhvcSAQICAQBuggW9MIIFuaADAgEFoQMCAQ4=");
        }

        [Fact]
        public void Obfuscate_Should_Be_Case_Insensitive_For_Auth_Schemes()
        {
            // Arrange
            var transportData = @"Authorization: bearer abc123";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Bearer *******");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Obfuscate_Should_Return_Original_If_No_Authorization_Header()
        {
            // Arrange
            var transportData = @"POST /api/test HTTP/1.1
Host: example.com
Content-Type: application/json";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Be(transportData);
        }

        [Fact]
        public void Obfuscate_Should_Return_Original_For_Empty_String()
        {
            // Act
            var result = HeaderObfuscator.Obfuscate("");

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void Obfuscate_Should_Return_Original_For_Null()
        {
            // Act
            var result = HeaderObfuscator.Obfuscate(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Obfuscate_Should_Handle_Malformed_Authorization_Header()
        {
            // Arrange - no colon in header
            var transportData = @"Authorization Bearer test";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Be(transportData);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Unknown_Auth_Scheme()
        {
            // Arrange
            var transportData = @"Authorization: CustomScheme abc123xyz";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Be(transportData); // Should return original for unknown schemes
        }

        #endregion

        #region Private Method Tests (via Reflection)

        [Fact]
        public void StripSignatureFromJWTToken_Should_Preserve_Header_And_Payload()
        {
            // Arrange
            var method = typeof(HeaderObfuscator).GetMethod("StripSignatureFromJWTToken",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Should().NotBeNull("StripSignatureFromJWTToken method should exist");

            // Act
            var result = method!.Invoke(null, new[] { SampleJwtToken }) as string;

            // Assert
            result.Should().NotBeNull();
            result.Should().StartWith("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.");
            result.Should().Contain("<SHA256>");
            result.Should().Contain("</SHA256>");
        }

        [Fact]
        public void StripSignatureFromJWTToken_Should_Return_Original_For_Invalid_JWT()
        {
            // Arrange
            var method = typeof(HeaderObfuscator).GetMethod("StripSignatureFromJWTToken",
                BindingFlags.NonPublic | BindingFlags.Static);
            var invalidToken = "not.a.valid.jwt.with.too.many.parts";

            // Act
            var result = method!.Invoke(null, new[] { invalidToken }) as string;

            // Assert
            result.Should().Be(invalidToken);
        }

        [Fact]
        public void GetHashSha256_Should_Return_Consistent_Hash()
        {
            // Arrange
            var method = typeof(HeaderObfuscator).GetMethod("GetHashSha256",
                BindingFlags.NonPublic | BindingFlags.Static);
            var input = "test-string";

            // Act
            var result1 = method!.Invoke(null, new[] { input }) as string;
            var result2 = method!.Invoke(null, new[] { input }) as string;

            // Assert
            result1.Should().Be(result2);
            result1.Should().HaveLength(64); // SHA256 produces 64 hex characters
            result1.Should().MatchRegex("^[0-9a-f]{64}$"); // Should be lowercase hex
        }

        [Fact]
        public void GetHashSha256_Should_Return_Different_Hashes_For_Different_Inputs()
        {
            // Arrange
            var method = typeof(HeaderObfuscator).GetMethod("GetHashSha256",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result1 = method!.Invoke(null, new[] { "input1" }) as string;
            var result2 = method!.Invoke(null, new[] { "input2" }) as string;

            // Assert
            result1.Should().NotBe(result2);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Obfuscate_Should_Handle_Multiple_Lines_With_Authorization()
        {
            // Arrange
            var transportData = $@"POST /api/test HTTP/1.1
Host: example.com
Content-Type: application/json
Authorization: Bearer {SampleJwtToken}
Accept: application/json
User-Agent: TestAgent/1.0";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Host: example.com");
            result.Should().Contain("Content-Type: application/json");
            result.Should().Contain("Accept: application/json");
            result.Should().Contain("<SHA256>");
            result.Should().NotContain(SampleJwtToken);
        }

        [Fact]
        public void Obfuscate_Should_Only_Replace_Token_Not_Other_Occurrences()
        {
            // Arrange - token appears multiple times
            var transportData = $@"Authorization: Bearer token123
Body: token123 should remain";

            // Act
            var result = HeaderObfuscator.Obfuscate(transportData);

            // Assert
            result.Should().Contain("Bearer *******");
            result.Should().Contain("Body: token123 should remain");
        }

        #endregion
    }
}
