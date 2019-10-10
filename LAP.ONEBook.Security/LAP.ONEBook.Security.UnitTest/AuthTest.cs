using System;
using Moq;
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
            _mockStorage.Setup(r => r.Authenticate("my-token")).Returns("{\"Name\":\"xyz\",\"Age\":1}");

            // Action
            var actual = auth.Authenticate("my-token");

            // Assert
            Assert.Equal("xyz", actual.Name);
            Assert.Equal(1, actual.Age);
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
        }
    }
}