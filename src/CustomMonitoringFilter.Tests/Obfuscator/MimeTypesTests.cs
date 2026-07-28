using CustomMonitoringFilter.Obfuscator;

namespace CustomMonitoringFilter.Tests.Obfuscator
{
    public class MimeTypesTests
    {
        #region IsJson Tests

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/json")]
        [InlineData("application/json; charset=utf-8")]
        [InlineData("APPLICATION/JSON")]
        public void IsJson_Should_Return_True_For_JSON_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsJson(mimeType);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/plain")]
        [InlineData("text/html")]
        [InlineData("application/octet-stream")]
        public void IsJson_Should_Return_False_For_Non_JSON_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsJson(mimeType);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region IsXml Tests

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        [InlineData("application/soap+xml")]
        [InlineData("application/atom+xml")]
        [InlineData("application/xml; charset=utf-8")]
        [InlineData("TEXT/XML")]
        public void IsXml_Should_Return_True_For_XML_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsXml(mimeType);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        [InlineData("text/html")]
        public void IsXml_Should_Return_False_For_Non_XML_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsXml(mimeType);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region IsTxt Tests

        [Theory]
        [InlineData("text/plain")]
        [InlineData("text/html")]
        [InlineData("text/plain; charset=utf-8")]
        [InlineData("TEXT/PLAIN")]
        public void IsTxt_Should_Return_True_For_Text_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsTxt(mimeType);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/xml")]
        [InlineData("application/octet-stream")]
        public void IsTxt_Should_Return_False_For_Non_Text_MimeTypes(string mimeType)
        {
            // Act
            var result = MimeTypes.IsTxt(mimeType);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Determine Tests

        [Fact]
        public void Determine_Should_Detect_JSON()
        {
            // Arrange
            var jsonMessage = @"{""key"": ""value""}";

            // Act
            var result = MimeTypes.Determine(jsonMessage);

            // Assert
            result.Should().Be("text/json");
        }

        [Fact]
        public void Determine_Should_Detect_JSON_Array()
        {
            // Arrange
            var jsonArray = @"[{""key"": ""value""}, {""key2"": ""value2""}]";

            // Act
            var result = MimeTypes.Determine(jsonArray);

            // Assert
            result.Should().Be("text/json");
        }

        [Fact]
        public void Determine_Should_Detect_XML()
        {
            // Arrange
            var xmlMessage = @"<?xml version=""1.0""?><root><item>value</item></root>";

            // Act
            var result = MimeTypes.Determine(xmlMessage);

            // Assert
            result.Should().Be("text/xml");
        }

        [Fact]
        public void Determine_Should_Detect_XML_Without_Declaration()
        {
            // Arrange
            var xmlMessage = @"<root><item>value</item></root>";

            // Act
            var result = MimeTypes.Determine(xmlMessage);

            // Assert
            result.Should().Be("text/xml");
        }

        [Fact]
        public void Determine_Should_Return_PlainText_For_Invalid_JSON_And_XML()
        {
            // Arrange
            var plainText = "This is just plain text";

            // Act
            var result = MimeTypes.Determine(plainText);

            // Assert
            result.Should().Be("text/plain");
        }

        [Fact]
        public void Determine_Should_Return_PlainText_For_Empty_String()
        {
            // Act
            var result = MimeTypes.Determine("");

            // Assert
            result.Should().Be("text/plain");
        }

        [Fact]
        public void Determine_Should_Return_PlainText_For_Whitespace()
        {
            // Act
            var result = MimeTypes.Determine("   \r\n\t   ");

            // Assert
            result.Should().Be("text/plain");
        }

        [Fact]
        public void Determine_Should_Prioritize_XML_Over_JSON()
        {
            // Arrange - Valid XML
            var xmlMessage = @"<root><item>test</item></root>";

            // Act
            var result = MimeTypes.Determine(xmlMessage);

            // Assert
            result.Should().Be("text/xml");
        }

        [Fact]
        public void Determine_Should_Handle_Complex_JSON()
        {
            // Arrange
            var complexJson = @"{
                ""user"": {
                    ""id"": 123,
                    ""name"": ""John"",
                    ""roles"": [""admin"", ""user""]
                }
            }";

            // Act
            var result = MimeTypes.Determine(complexJson);

            // Assert
            result.Should().Be("text/json");
        }

        [Fact]
        public void Determine_Should_Handle_SOAP_XML()
        {
            // Arrange
            var soapXml = @"<?xml version=""1.0""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <m:GetUser xmlns:m=""http://example.com"">
            <m:UserId>123</m:UserId>
        </m:GetUser>
    </soap:Body>
</soap:Envelope>";

            // Act
            var result = MimeTypes.Determine(soapXml);

            // Assert
            result.Should().Be("text/xml");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void IsJson_Should_Handle_Partial_Match()
        {
            // Arrange - MimeType contains json substring
            var mimeType = "application/vnd.api+json";

            // Act
            var result = MimeTypes.IsJson(mimeType);

            // Assert
            // Current implementation checks exact matches in array, not partial substring
            result.Should().BeFalse();
        }

        [Fact]
        public void IsXml_Should_Handle_Custom_XML_MimeType()
        {
            // Arrange
            var mimeType = "application/vnd.custom+xml";

            // Act
            var result = MimeTypes.IsXml(mimeType);

            // Assert
            // Current implementation checks exact matches in array, not partial substring
            result.Should().BeFalse();
        }

        [Fact]
        public void Determine_Should_Handle_Malformed_JSON()
        {
            // Arrange
            var malformedJson = @"{invalid json without quotes}";

            // Act
            var result = MimeTypes.Determine(malformedJson);

            // Assert
            result.Should().Be("text/plain");
        }

        [Fact]
        public void Determine_Should_Handle_Malformed_XML()
        {
            // Arrange
            var malformedXml = @"<root><unclosed tag>";

            // Act
            var result = MimeTypes.Determine(malformedXml);

            // Assert
            result.Should().Be("text/plain"); // Should fallback to plain text
        }

        #endregion

        #region Case Sensitivity Tests

        [Fact]
        public void All_Methods_Should_Be_Case_Insensitive()
        {
            // Assert
            MimeTypes.IsJson("APPLICATION/JSON").Should().BeTrue();
            MimeTypes.IsJson("application/json").Should().BeTrue();
            MimeTypes.IsJson("Application/Json").Should().BeTrue();

            MimeTypes.IsXml("TEXT/XML").Should().BeTrue();
            MimeTypes.IsXml("text/xml").Should().BeTrue();
            MimeTypes.IsXml("Text/Xml").Should().BeTrue();

            MimeTypes.IsTxt("TEXT/PLAIN").Should().BeTrue();
            MimeTypes.IsTxt("text/plain").Should().BeTrue();
            MimeTypes.IsTxt("Text/Plain").Should().BeTrue();
        }

        #endregion
    }
}
