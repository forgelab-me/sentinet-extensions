using System.Text.RegularExpressions;
using CustomMonitoringFilter.Obfuscator;

namespace CustomMonitoringFilter.Tests.Obfuscator
{
    public class PlaintextObfuscatorTests
    {
        private const string ObfuscatedValue = "***OBFUSCATED***";

        #region Basic Obfuscation Tests

        [Fact]
        public void Obfuscate_Should_Replace_Matched_Pattern()
        {
            // Arrange
            var text = "This is a secret: MY_SECRET_DATA here";
            var regexes = new[] { new Regex(@"MY_SECRET_DATA", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("This is a secret:");
            result.Should().NotContain("MY_SECRET_DATA");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Replace_Multiple_Occurrences()
        {
            // Arrange
            var text = "SECRET appears here and SECRET appears there";
            var regexes = new[] { new Regex(@"SECRET", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().NotContain("SECRET");
            var count = Regex.Matches(result, Regex.Escape(ObfuscatedValue)).Count;
            count.Should().Be(2);
        }

        [Fact]
        public void Obfuscate_Should_Apply_Multiple_Regex_Patterns()
        {
            // Arrange
            var text = "PASSWORD: secret123, API_KEY: key456";
            var regexes = new[]
            {
                new Regex(@"(?<=PASSWORD: )\w+", RegexOptions.Compiled),
                new Regex(@"(?<=API_KEY: )\w+", RegexOptions.Compiled)
            };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("PASSWORD:");
            result.Should().Contain("API_KEY:");
            result.Should().NotContain("secret123");
            result.Should().NotContain("key456");
            var count = Regex.Matches(result, Regex.Escape(ObfuscatedValue)).Count;
            count.Should().Be(2);
        }

        #endregion

        #region Base64 Pattern Tests

        [Fact]
        public void Obfuscate_Should_Handle_BASE64_Tag_Pattern()
        {
            // Arrange
            var text = @"<data BASE64"">iVBORw0KGgoAAAANSUhEUgAAAAUA</BASE64>";
            var regexes = new[] { new Regex(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("</BASE64>");
            result.Should().NotContain("iVBORw0KGgoAAAANSUhEUgAAAAUA");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Multiple_BASE64_Tags()
        {
            // Arrange
            var text = @"
                First: <img BASE64"">data1</BASE64>
                Second: <img BASE64"">data2</BASE64>";
            var regexes = new[] { new Regex(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().NotContain("data1");
            result.Should().NotContain("data2");
            var count = Regex.Matches(result, Regex.Escape(ObfuscatedValue)).Count;
            count.Should().Be(2);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Multiline_BASE64_Content()
        {
            // Arrange
            var text = @"<data BASE64"">
iVBORw0KGgoAAAANSUhEUgAAAAUA
AAAFCAYAAACNbyblAAAAHElEQVQI12P4
//8/w38GIAXDIBKE0DHxgljNBAAO
9TXL0Y4OHwAAAABJRU5ErkJggg==
</BASE64>";
            var regexes = new[] { new Regex(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().NotContain("iVBORw0KGgoAAAANSUhEUgAAAAUA");
            result.Should().NotContain("AAAFCAYAAACNbyblAAAAHElEQVQI12P4");
            result.Should().Contain(ObfuscatedValue);
        }

        #endregion

        #region Complex Patterns

        [Fact]
        public void Obfuscate_Should_Handle_Email_Pattern()
        {
            // Arrange
            var text = "Contact: john.doe@example.com for more info";
            var regexes = new[] { new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Contact:");
            result.Should().NotContain("john.doe@example.com");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Credit_Card_Pattern()
        {
            // Arrange
            var text = "Card: 4532-1234-5678-9010";
            var regexes = new[] { new Regex(@"\d{4}-\d{4}-\d{4}-\d{4}", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Card:");
            result.Should().NotContain("4532-1234-5678-9010");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Bearer_Token_Pattern()
        {
            // Arrange
            var text = "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.payload.signature";
            var regexes = new[] { new Regex(@"(?<=Bearer\s)[\w\-\._]+", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Bearer");
            result.Should().NotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9");
            result.Should().Contain(ObfuscatedValue);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Obfuscate_Should_Return_Original_For_Null_Text()
        {
            // Arrange
            var regexes = new[] { new Regex(@"SECRET", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Obfuscate_Should_Return_Original_For_Empty_Text()
        {
            // Arrange
            var regexes = new[] { new Regex(@"SECRET", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate("");

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void Obfuscate_Should_Return_Original_When_No_Match()
        {
            // Arrange
            var text = "This text has no secrets";
            var regexes = new[] { new Regex(@"SECRET_PATTERN", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Be(text);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Empty_Regex_Array()
        {
            // Arrange
            var text = "Some text here";
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, []);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Be(text);
        }

        [Fact]
        public void Obfuscate_Should_Not_Throw_On_Invalid_Regex_Match()
        {
            // Arrange
            var text = "Normal text without special characters";
            var regexes = new[] { new Regex(@"\d{10,}", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var act = () => obfuscator.Obfuscate(text);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Special Characters & Encoding

        [Fact]
        public void Obfuscate_Should_Handle_Special_Characters()
        {
            // Arrange
            var text = "Password: P@ssw0rd!#$%";
            var regexes = new[] { new Regex(@"(?<=Password: ).*", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Password:");
            result.Should().NotContain("P@ssw0rd!#$%");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Unicode_Characters()
        {
            // Arrange
            var text = "Secret: éàü中文🔒";
            var regexes = new[] { new Regex(@"(?<=Secret: ).*", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Secret:");
            result.Should().NotContain("éàü中文🔒");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Newlines_And_Tabs()
        {
            // Arrange
            var text = "Line1:\tSecretValue\r\nLine2: NormalValue";
            var regexes = new[] { new Regex(@"SecretValue", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(text);

            // Assert
            result.Should().Contain("Line1:");
            result.Should().Contain("Line2: NormalValue");
            result.Should().NotContain("SecretValue");
            result.Should().Contain(ObfuscatedValue);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Obfuscate_Should_Handle_Large_Text_Efficiently()
        {
            // Arrange
            var largeText = string.Concat(Enumerable.Repeat("Normal text. ", 1000)) + "SECRET" + string.Concat(Enumerable.Repeat(" More text.", 1000));
            var regexes = new[] { new Regex(@"SECRET", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var act = () => obfuscator.Obfuscate(largeText);

            // Assert
            act.Should().NotThrow();
            var result = act();
            result.Should().NotContain("SECRET");
            result.Should().Contain(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Many_Regex_Patterns()
        {
            // Arrange
            var text = "password1 secret2 apikey3 token4";
            var regexes = Enumerable.Range(1, 50)
                .Select(i => new Regex($@"pattern{i}", RegexOptions.Compiled))
                .Append(new Regex(@"password\d+", RegexOptions.Compiled))
                .ToArray();
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var act = () => obfuscator.Obfuscate(text);

            // Assert
            act.Should().NotThrow();
            var result = act();
            result.Should().NotContain("password1");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Obfuscate_Should_Handle_Real_World_HTTP_Request()
        {
            // Arrange
            var httpRequest = @"POST /api/auth HTTP/1.1
Host: api.example.com
Content-Type: application/json
Authorization: Bearer sk-1234567890abcdef

{""username"": ""john"", ""password"": ""secret""}";
            var regexes = new[] { new Regex(@"(?<=Bearer\s)[\w\-]+", RegexOptions.Compiled) };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(httpRequest);

            // Assert
            result.Should().Contain("Authorization: Bearer");
            result.Should().NotContain("sk-1234567890abcdef");
            result.Should().Contain(ObfuscatedValue);
            result.Should().Contain("username");
        }

        [Fact]
        public void Obfuscate_Should_Handle_Mixed_Content_Types()
        {
            // Arrange
            var mixedContent = @"
                Email: user@example.com
                Base64 Image: BASE64"">iVBORw0KGgoAAAANSUhEUg</BASE64>
                API Key: sk-proj-abc123xyz
                Normal text continues here...
            ";
            var regexes = new[]
            {
                new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled),
                new Regex(@"(?s)(?<=BASE64"">).*?(?=<)", RegexOptions.Compiled),
                new Regex(@"sk-proj-[\w]+", RegexOptions.Compiled)
            };
            var obfuscator = new PlaintextObfuscator(ObfuscatedValue, regexes);

            // Act
            var result = obfuscator.Obfuscate(mixedContent);

            // Assert
            result.Should().NotContain("user@example.com");
            result.Should().NotContain("iVBORw0KGgoAAAANSUhEUg");
            result.Should().NotContain("sk-proj-abc123xyz");
            result.Should().Contain("Email:");
            result.Should().Contain("API Key:");
            result.Should().Contain("Normal text continues here");
        }

        #endregion
    }
}
