using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> SerializeJsonTestData
        {
            get
            {
                yield return new object[]
                {
                    new TestAuthSerializationClass { Name = "Local", Time = new DateTime(1, 2, 3, 4, 5, 6, 7, DateTimeKind.Local) },
                    "{\"Name\":\"Local\",\"Time\":\"0001-02-03T04:05:06.007\"}"
                };
                yield return new object[]
                {
                    new TestAuthSerializationClass { Name = "Utc", Time = new DateTime(2, 3, 4, 5, 6, 7, 8, DateTimeKind.Utc) },
                    "{\"Name\":\"Utc\",\"Time\":\"0002-03-04T05:06:07.008\"}"
                };
                yield return new object[]
                {
                    new TestAuthSerializationClass { Name = "Unspecified", Time = new DateTime(3, 4, 5, 6, 7, 8, 9, DateTimeKind.Unspecified) },
                    "{\"Name\":\"Unspecified\",\"Time\":\"0003-04-05T06:07:08.009\"}"
                };
            }
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
            Assert.DoesNotContain('/', token);
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

        [Theory]
        [MemberData(nameof(SerializeJsonTestData))]
        public void ApplyToken_StorageData_SerializeToJson(TestAuthSerializationClass data, string json)
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);

            // Action
            auth.ApplyToken(data);

            // Assert
            _mockStorage.Verify(r => r.ApplyToken(
                It.IsAny<string>(),
                It.Is<string>(p => p == json),
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

        [Fact]
        public void Authenticate_HasDataInStorage_Deserialize()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);
            _mockStorage.Setup(r => r.Authenticate("my-token")).Returns("{\"Name\":\"ABC\",\"Time\":\"1111-11-11T11:11:11.111\"}");

            // Action
            var actual = auth.Authenticate("my-token");

            // Assert
            Assert.Equal("ABC", actual.Name);
            Assert.Equal(new DateTime(1111, 11, 11, 11, 11, 11, 111), actual.Time);
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

        [Theory]
        [InlineData("{}", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Authenticate_WhenCalled_StatusOfDataInStorage(string permissionData, bool expected)
        {
            // Arrange
            TestAuthSerializationClass result = null;
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);
            _mockStorage.Setup(r => r.Authenticate("test-token")).Returns(permissionData);

            // Action
            var hasDataInStorage = auth.Authenticate("test-token", ref result);

            // Assert
            Assert.Equal(expected, hasDataInStorage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Authenticate_NoDataInStorage_ReturnDefault(string permissionData)
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

        [Fact]
        public void Authenticate_InvalidJsonData_Throws()
        {
            // Arrange
            _mockStorage.Setup(r => r.Authenticate(It.IsAny<string>())).Returns("invalid-permission-data");

            // Action
            var actual = Assert.Throws<Exception>(() => new Auth<int>(_mockStorage.Object).Authenticate("token"));
            var originalException = actual.InnerException;

            // Assert
            Assert.Equal($"Error: {originalException.Message}\r\nPermissionData: invalid-permission-data", actual.Message);
        }

        public class TestAuthSerializationClass
        {
            public string Name { get; set; }

            public DateTime Time { get; set; }
        }
    }
}