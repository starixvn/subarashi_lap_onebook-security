using System;
using System.Runtime.Serialization;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace LAP.ONEBook.Security.UnitTest
{
    public class AuthTest
    {
        private Mock<IStorageProvider> _mockStorage;

        public AuthTest()
        {
            _mockStorage = new Mock<IStorageProvider>();
        }

        [Theory]
        [InlineData("same-value", "same-value")]
        [InlineData("test-value-1", "test-value-2")]
        public void ApplyToken_Token_NotEmptyAndUnique(string value1, string value2)
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token1 = auth.ApplyToken(value1);
            Assert.NotEmpty(token1);

            var token2 = auth.ApplyToken(value2);
            Assert.NotEmpty(token2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void ApplyToken_Token_ExcludeSpecialChars()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token = auth.ApplyToken("test-special-chars");

            // Assert
            Assert.DoesNotContain('=', token);
            Assert.DoesNotContain('+', token);
        }

        [Fact]
        public void ApplyToken_Storage_ApplyTokenWithExpiry()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token = auth.ApplyToken("test");

            // Assert
            _mockStorage.Verify(r => r.ApplyToken(token, It.IsAny<string>(), 1));
        }

        [Fact]
        public void ApplyToken_StorageData_SerializeToJson()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);

            // Action
            auth.ApplyToken(new TestAuthSerializationClass { Age = 123, Name = "abc" });

            // Assert
            _mockStorage.Verify(r => r.ApplyToken(
                It.IsAny<string>(),
                It.Is<string>(p => p == "{\"Age\":123,\"Name\":\"abc\"}"),
                It.IsAny<int>()));
        }

        [Fact]
        public void ApplyToken_JsonSpecialChars_WellSerialize()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);
            string actual = null;
            _mockStorage.Setup(r => r.ApplyToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback(new InvocationAction(i => actual = (string)i.Arguments[1]));

            // Action
            auth.ApplyToken("\"\\/\b\f\n\r\t");

            // Assert
            Assert.Equal("\"\\\"\\\\/\\b\\f\\n\\r\\t\"", actual);
        }

        [Theory]
        [InlineData("same-value", "same-value")]
        [InlineData("test-value-1", "test-value-2")]
        public void ApplyTokenAsync_Token_NotEmptyAndUnique(string value1, string value2)
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token1 = auth.ApplyToken(value1);
            Assert.NotEmpty(token1);

            var token2 = auth.ApplyToken(value2);
            Assert.NotEmpty(token2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public async void ApplyTokenAsync_Token_ExcludeSpecialChars()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token = await auth.ApplyTokenAsync("test-special-chars");

            // Assert
            Assert.DoesNotContain('=', token);
            Assert.DoesNotContain('+', token);
        }

        [Fact]
        public async void ApplyTokenAsync_Storage_ApplyTokenWithExpiry()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);

            // Action
            var token = await auth.ApplyTokenAsync("test");

            // Assert
            _mockStorage.Verify(r => r.ApplyTokenAsync(token, It.IsAny<string>(), 1));
        }

        [Fact]
        public async void ApplyTokenAsync_StorageData_SerializeToJson()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);

            // Action
            await auth.ApplyTokenAsync(new TestAuthSerializationClass { Name = "def", Age = 200 });

            // Assert
            _mockStorage.Verify(r => r.ApplyTokenAsync(
                It.IsAny<string>(),
                It.Is<string>(p => p == "{\"Age\":200,\"Name\":\"def\"}"),
                It.IsAny<int>()));
        }

        [Fact]
        public async void ApplyTokenAsync_JsonSpecialChars_WellSerialize()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);
            string actual = null;
            _mockStorage.Setup(r => r.ApplyTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback(new InvocationAction(i => actual = (string)i.Arguments[1]));

            // Action
            await auth.ApplyTokenAsync("\"\\/\b\f\n\r\t");

            // Assert
            Assert.Equal("\"\\\"\\\\/\\b\\f\\n\\r\\t\"", actual);
        }

        [Fact]
        public void Authenticate_HasPermissionData_Deserialize()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);
            _mockStorage.Setup(r => r.Authenticate("my-token"))
                .Returns("{\"<ActionLink>k__BackingField\":\"/api/NotificationApi/Index\",\"<Currency>k__BackingField\":[{\"AgentId\":[29173034,29173042,29470955],\"Currency\":\"MYR\",\"CurrencyId\":2},{\"AgentId\":[28589030,28589036,28946751],\"Currency\":\"USD\",\"CurrencyId\":3},{\"AgentId\":[860379],\"Currency\":\"EUR\",\"CurrencyId\":6},{\"AgentId\":[1015422,1015423],\"Currency\":\"AUD\",\"CurrencyId\":9},{\"AgentId\":[28031433,28031438,28031443,28927811,28949625,28949628,29116702,29116708,29116716,29314284,29314288,29314291,29314294,29314297,29469096,29469098,29469100,29536582,29536585,29536588,29536592],\"Currency\":\"CNY\",\"CurrencyId\":13},{\"AgentId\":[3233923,28041170,28041171,28041179,28186750,28186751,28927829,28956769,28956774,29116738,29116742,29173019,29173022,29314233,29314241,29314244,29314249,29314254,29470741,29470745,29470747,29470749,29470751,29470753,29470756,29470758,29536613,29536616,29536618,29536620,29536622,29536624,29740145,29740147,31844017],\"Currency\":\"UUS\",\"CurrencyId\":20},{\"AgentId\":[29595890],\"Currency\":\"INH\",\"CurrencyId\":51},{\"AgentId\":[6006447],\"Currency\":\"INR\",\"CurrencyId\":61},{\"AgentId\":[29595908],\"Currency\":\"MKK\",\"CurrencyId\":70}],\"<IsAdminstrator>k__BackingField\":true,\"<Language>k__BackingField\":\"en\",\"<Permissions>k__BackingField\":[],\"<SiteName>k__BackingField\":\"12bet\",\"<UserID>k__BackingField\":\"vntest\",\"<UserName>k__BackingField\":\"vntest\",\"<UserSEQ>k__BackingField\":3761,\"<VendorID>k__BackingField\":\"12betv5\"}"


);

            // Action
            var actual = auth.Authenticate("my-token");

            // Assert
            Assert.Equal("12bet", actual.Name);
            Assert.Equal(0, actual.Age);
        }

        [Fact]
        public async void AuthenticateAsync_HasPermissionData_Deserialize()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);
            _mockStorage.Setup(r => r.AuthenticateAsync("my-token")).ReturnsAsync("{\"Name\":\"ABC\",\"Age\":111");

            // Action
            var actual = await auth.AuthenticateAsync("my-token");

            // Assert
            Assert.Equal("ABC", actual.Name);
            Assert.Equal(111, actual.Age);
        }

        [Fact]
        public void Authenticate_JsonSpecialChars_WellDeserialize()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);
            _mockStorage.Setup(r => r.Authenticate("test")).Returns("\"\\\"\\\\/\\b\\f\\n\\r\\t\"");

            // Action
            var actual = auth.Authenticate("test");

            // Assert
            Assert.Equal("\"\\/\b\f\n\r\t", actual);
        }

        [Fact]
        public async void AuthenticateAsync_JsonSpecialChars_WellDeserialize()
        {
            // Arrange
            var auth = new Auth<string>(_mockStorage.Object);
            _mockStorage.Setup(r => r.AuthenticateAsync("test")).ReturnsAsync("\"\\\"\\\\/\\b\\f\\n\\r\\t\"");

            // Action
            var actual = await auth.AuthenticateAsync("test");

            // Assert
            Assert.Equal("\"\\/\b\f\n\r\t", actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Authenticate_NoPermissionData_ReturnDefault(string permissionData)
        {
            // Arrange
            _mockStorage.Setup(r => r.Authenticate(It.IsAny<string>())).Returns(permissionData);

            // Action
            // Assert
            Assert.Equal(0, new Auth<int>(_mockStorage.Object).Authenticate("test-integer"));
            Assert.Equal(0, new Auth<decimal>(_mockStorage.Object).Authenticate("test-decimal"));
            Assert.Equal(DateTime.MinValue, new Auth<DateTime>(_mockStorage.Object).Authenticate("test-datetime"));
            Assert.Null(new Auth<string>(_mockStorage.Object).Authenticate("test-string"));
            Assert.Null(new Auth<TestAuthSerializationClass>(_mockStorage.Object).Authenticate("test-object"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void AuthenticateAsync_NoPermissionData_ReturnDefault(string permissionData)
        {
            // Arrange
            _mockStorage.Setup(r => r.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(permissionData);

            // Action
            // Assert
            Assert.Equal(0, await new Auth<int>(_mockStorage.Object).AuthenticateAsync("test-integer"));
            Assert.Equal(0, await new Auth<decimal>(_mockStorage.Object).AuthenticateAsync("test-decimal"));
            Assert.Equal(DateTime.MinValue, await new Auth<DateTime>(_mockStorage.Object).AuthenticateAsync("test-datetime"));
            Assert.Null(await new Auth<string>(_mockStorage.Object).AuthenticateAsync("test-string"));
            Assert.Null(await new Auth<TestAuthSerializationClass>(_mockStorage.Object).AuthenticateAsync("test-object"));
        }

        [Fact]
        public void Authenticate_Exception_ThrowCustomError()
        {
            // Arrange
            _mockStorage.Setup(r => r.Authenticate(It.IsAny<string>())).Returns("invalid-permission-data");

            // Action
            var actual = Assert.Throws<Exception>(() => new Auth<int>(_mockStorage.Object).Authenticate("test-token"));
            var originalException = actual.InnerException;

            // Assert
            Assert.Equal($"Authenticate error: {originalException.Message}\r\nToken=test-token\r\nData=invalid-permission-data", actual.Message);
        }

        [Fact]
        public async void AuthenticateAsync_Exception_ThrowCustomError()
        {
            // Arrange
            _mockStorage.Setup(r => r.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync("invalid-permission-data");

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => new Auth<int>(_mockStorage.Object).AuthenticateAsync("test-token"));
            var originalException = actual.InnerException;

            // Assert
            Assert.Equal($"Authenticate error: {originalException.Message}\r\nToken=test-token\r\nData=invalid-permission-data", actual.Message);
        }

        public class TestAuthSerializationClass
        {

            public string Name { get; set; }

            public int Age { get; set; }

            [JsonProperty("<SiteName>k__BackingField")]
            public string SiteName { get; set; }

            [JsonProperty("<ActionLink>k__BackingField")]
            private string LegacyActionLink { set { ActionLink = value; } }
            public string ActionLink { get; set; }

        }

    }
}