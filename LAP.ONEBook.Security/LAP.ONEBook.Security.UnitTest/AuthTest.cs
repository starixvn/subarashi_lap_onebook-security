using LAP.ONEBook.Security.StorageProviders;
using Moq;
using System;
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
            auth.ApplyToken(new TestAuthSerializationClass
            {
                Name = "Bill",
                Number = 1
            });

            // Assert
            _mockStorage.Verify(r => r.ApplyToken(
                It.IsAny<string>(),
                It.Is<string>(p => p == "{\"Name\":\"Bill\",\"Number\":1}"),
                It.IsAny<int>()));
        }

        [Fact]
        public void Authenticate_HasDataInStorage_Deserialize()
        {
            // Arrange
            var auth = new Auth<TestAuthSerializationClass>(_mockStorage.Object);
            _mockStorage.Setup(r => r.Authenticate("my-token"))
                .Returns("{\"Name\":\"Steve\",\"Number\":100}");

            // Action
            var actual = auth.Authenticate("my-token");

            // Assert
            Assert.Equal("Steve", actual.Name);
            Assert.Equal(100, actual.Number);
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

        public class TestAuthSerializationClass
        {
            public string Name { get; set; }

            public int Number { get; set; }
        }
    }
}