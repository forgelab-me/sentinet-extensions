using System.Text.RegularExpressions;
using Newtonsoft.Json;
using CustomMonitoringFilter.Obfuscator;

namespace CustomMonitoringFilter.Tests.Obfuscator
{
    public class JsonObfuscatorTests
    {
        private const string ObfuscatedValue = "***OBFUSCATED***";

        #region Simple Field Obfuscation

        [Fact]
        public void Obfuscate_Should_Replace_Simple_Field()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "username", "john.doe" },
                { "password", "secret123" }
            };
            var fieldsToReplace = new[] { "password" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["username"].Should().Be("john.doe");
            jsonDoc["password"].Should().Be(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Be_Case_Insensitive()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "PassWord", "secret123" },
                { "CLIENT_SECRET", "abc123" }
            };
            var fieldsToReplace = new[] { "password", "client_secret" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["PassWord"].Should().Be(ObfuscatedValue);
            jsonDoc["CLIENT_SECRET"].Should().Be(ObfuscatedValue);
        }

        [Fact]
        public void Obfuscate_Should_Handle_Multiple_Fields()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "username", "john" },
                { "password", "secret" },
                { "api_key", "key123" },
                { "client_secret", "secret456" }
            };
            var fieldsToReplace = new[] { "password", "api_key", "client_secret" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["username"].Should().Be("john");
            jsonDoc["password"].Should().Be(ObfuscatedValue);
            jsonDoc["api_key"].Should().Be(ObfuscatedValue);
            jsonDoc["client_secret"].Should().Be(ObfuscatedValue);
        }

        #endregion

        #region Field Removal

        [Fact]
        public void Obfuscate_Should_Remove_Field()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "username", "john" },
                { "tempToken", "xyz" }
            };
            var fieldNamesToRemove = new[] { "tempToken" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, [], [], fieldNamesToRemove);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc.Should().ContainKey("username");
            jsonDoc.Should().NotContainKey("tempToken");
        }

        [Fact]
        public void Obfuscate_Should_Remove_Multiple_Fields()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "keepThis", "value1" },
                { "removeThis", "value2" },
                { "removeThisToo", "value3" }
            };
            var fieldNamesToRemove = new[] { "removeThis", "removeThisToo" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, [], [], fieldNamesToRemove);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc.Should().ContainKey("keepThis");
            jsonDoc.Should().NotContainKey("removeThis");
            jsonDoc.Should().NotContainKey("removeThisToo");
        }

        #endregion

        #region Regex Pattern Matching

        [Fact]
        public void Obfuscate_Should_Match_Regex_Pattern()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "api_key_prod", "key1" },
                { "api_key_dev", "key2" },
                { "username", "john" }
            };
            var regexes = new[] { new Regex(@"^api_key_.*$", RegexOptions.Compiled) };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, [], regexes, []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["api_key_prod"].Should().Be(ObfuscatedValue);
            jsonDoc["api_key_dev"].Should().Be(ObfuscatedValue);
            jsonDoc["username"].Should().Be("john");
        }

        [Fact]
        public void Obfuscate_Should_Support_Multiple_Regex_Patterns()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "secret_key", "key1" },
                { "token_value", "token1" },
                { "normal_field", "value" }
            };
            var regexes = new[]
            {
                new Regex(@"^secret_.*$", RegexOptions.Compiled),
                new Regex(@".*_token$|^token_.*$", RegexOptions.Compiled)
            };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, [], regexes, []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["secret_key"].Should().Be(ObfuscatedValue);
            jsonDoc["token_value"].Should().Be(ObfuscatedValue);
            jsonDoc["normal_field"].Should().Be("value");
        }

        #endregion

        #region Nested Object Obfuscation

        [Fact]
        public void Obfuscate_Should_Handle_Nested_Objects()
        {
            // Arrange
            var nestedJson = @"{
                ""user"": {
                    ""name"": ""john"",
                    ""password"": ""secret123""
                }
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(nestedJson)!;
            var fieldsToReplace = new[] { "password" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().Contain("john");
            serialized.Should().Contain(ObfuscatedValue);
            serialized.Should().NotContain("secret123");
        }

        [Fact]
        public void Obfuscate_Should_Handle_Deeply_Nested_Objects()
        {
            // Arrange
            var nestedJson = @"{
                ""level1"": {
                    ""level2"": {
                        ""level3"": {
                            ""password"": ""deep-secret""
                        }
                    }
                }
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(nestedJson)!;
            var fieldsToReplace = new[] { "password" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().Contain(ObfuscatedValue);
            serialized.Should().NotContain("deep-secret");
        }

        [Fact]
        public void Obfuscate_Should_Handle_Array_Of_Objects()
        {
            // Arrange
            var arrayJson = @"{
                ""users"": [
                    { ""name"": ""john"", ""password"": ""pass1"" },
                    { ""name"": ""jane"", ""password"": ""pass2"" }
                ]
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(arrayJson)!;
            var fieldsToReplace = new[] { "password" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().Contain("john");
            serialized.Should().Contain("jane");
            serialized.Should().NotContain("pass1");
            serialized.Should().NotContain("pass2");
            // The obfuscated value should appear at least twice (once per user)
            var count = Regex.Matches(serialized, Regex.Escape(ObfuscatedValue)).Count;
            count.Should().BeGreaterOrEqualTo(2);
        }

        [Fact]
        public void Obfuscate_Should_Preserve_Non_Object_Array_Elements()
        {
            // Arrange
            var arrayJson = @"{
                ""tags"": [""tag1"", ""tag2"", ""tag3""],
                ""password"": ""secret""
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(arrayJson)!;
            var fieldsToReplace = new[] { "password" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().Contain("tag1");
            serialized.Should().Contain("tag2");
            serialized.Should().Contain("tag3");
            serialized.Should().Contain(ObfuscatedValue);
            serialized.Should().NotContain("secret");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Obfuscate_Should_Handle_Null_Dictionary()
        {
            // Arrange
            var obfuscator = new JsonObfuscator(ObfuscatedValue, [], [], []);

            // Act
            var act = () => obfuscator.Obfuscate(null!);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Obfuscate_Should_Handle_Empty_Dictionary()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>();
            var obfuscator = new JsonObfuscator(ObfuscatedValue, new[] { "password" }, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc.Should().BeEmpty();
        }

        [Fact]
        public void Obfuscate_Should_Handle_No_Matching_Fields()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "username", "john" },
                { "email", "john@example.com" }
            };
            var fieldsToReplace = new[] { "password", "api_key" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["username"].Should().Be("john");
            jsonDoc["email"].Should().Be("john@example.com");
        }

        [Fact]
        public void Obfuscate_Should_Not_Throw_On_Complex_Nested_Structure()
        {
            // Arrange
            var complexJson = @"{
                ""users"": [
                    {
                        ""profile"": {
                            ""credentials"": {
                                ""password"": ""secret1"",
                                ""apiKeys"": [
                                    { ""key"": ""key1"", ""client_secret"": ""cs1"" },
                                    { ""key"": ""key2"", ""client_secret"": ""cs2"" }
                                ]
                            }
                        }
                    }
                ]
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(complexJson)!;
            var fieldsToReplace = new[] { "password", "client_secret" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            var act = () => obfuscator.Obfuscate(jsonDoc);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Mixed Operations

        [Fact]
        public void Obfuscate_Should_Both_Obfuscate_And_Remove_Fields()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "username", "john" },
                { "password", "secret" },
                { "tempToken", "temp123" }
            };
            var fieldsToReplace = new[] { "password" };
            var fieldsToRemove = new[] { "tempToken" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], fieldsToRemove);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["username"].Should().Be("john");
            jsonDoc["password"].Should().Be(ObfuscatedValue);
            jsonDoc.Should().NotContainKey("tempToken");
        }

        [Fact]
        public void Obfuscate_Should_Apply_Both_Exact_Match_And_Regex()
        {
            // Arrange
            var jsonDoc = new Dictionary<string, object>
            {
                { "password", "pass1" },
                { "api_key_prod", "key1" },
                { "username", "john" }
            };
            var fieldsToReplace = new[] { "password" };
            var regexes = new[] { new Regex(@"^api_key_.*$", RegexOptions.Compiled) };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, regexes, []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            jsonDoc["password"].Should().Be(ObfuscatedValue);
            jsonDoc["api_key_prod"].Should().Be(ObfuscatedValue);
            jsonDoc["username"].Should().Be("john");
        }

        #endregion

        #region Performance & HashSet Verification

        [Fact]
        public void Obfuscate_Should_Use_HashSet_For_Fast_Lookup()
        {
            // Arrange - large field list to verify O(1) lookup
            var largeFieldList = Enumerable.Range(1, 1000)
                .Select(i => $"field{i}")
                .Append("password")
                .ToArray();

            var jsonDoc = new Dictionary<string, object>
            {
                { "password", "secret" },
                { "username", "john" }
            };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, largeFieldList, [], []);

            // Act
            var act = () => obfuscator.Obfuscate(jsonDoc);

            // Assert
            act.Should().NotThrow();
            jsonDoc["password"].Should().Be(ObfuscatedValue);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Obfuscate_Should_Handle_Real_World_OAuth_Response()
        {
            // Arrange
            var oauthJson = @"{
                ""access_token"": ""ya29.a0AfH6SMBx..."",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3599,
                ""refresh_token"": ""1//0gOauth2RefreshToken"",
                ""client_id"": ""123456789.apps.googleusercontent.com"",
                ""client_secret"": ""GOCSPX-AbCdEf123456""
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(oauthJson)!;
            var fieldsToReplace = new[] { "client_secret", "refresh_token", "access_token" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().NotContain("ya29.a0AfH6SMBx");
            serialized.Should().NotContain("1//0gOauth2RefreshToken");
            serialized.Should().NotContain("GOCSPX-AbCdEf123456");
            serialized.Should().Contain(ObfuscatedValue);
            serialized.Should().Contain("Bearer");
            serialized.Should().Contain("3599");
        }

        [Fact]
        public void Obfuscate_Should_Handle_Real_World_User_Profile()
        {
            // Arrange
            var profileJson = @"{
                ""user"": {
                    ""id"": 12345,
                    ""username"": ""john.doe"",
                    ""email"": ""john@example.com"",
                    ""password"": ""MySecretP@ss123"",
                    ""settings"": {
                        ""notifications"": true,
                        ""apiKey"": ""sk-1234567890abcdef""
                    },
                    ""image"": ""iVBORw0KGgoAAAANSUhEUgAAAAUA..."",
                    ""lastLogin"": ""2024-01-15T10:30:00Z""
                }
            }";
            var jsonDoc = JsonConvert.DeserializeObject<Dictionary<string, object>>(profileJson)!;
            var fieldsToReplace = new[] { "password", "apiKey", "image" };
            var obfuscator = new JsonObfuscator(ObfuscatedValue, fieldsToReplace, [], []);

            // Act
            obfuscator.Obfuscate(jsonDoc);

            // Assert
            var serialized = JsonConvert.SerializeObject(jsonDoc);
            serialized.Should().Contain("john.doe");
            serialized.Should().Contain("john@example.com");
            serialized.Should().NotContain("MySecretP@ss123");
            serialized.Should().NotContain("sk-1234567890abcdef");
            serialized.Should().NotContain("iVBORw0KGgoAAAANSUhEUgAAAAUA");
            serialized.Should().Contain(ObfuscatedValue);
        }

        #endregion
    }
}
