using LAP.ONEBook.Security.StorageProviders;
using Moq;
using StackExchange.Redis;
using System;
using Xunit;

namespace LAP.ONEBook.Security.UnitTest.StorageProviders
{
    public class RedisProviderTest
    {
        private Mock<IConnectionMultiplexer> _mockConnection;

        public RedisProviderTest()
        {
            _mockConnection = new Mock<IConnectionMultiplexer>();
        }

        [Fact]
        public void ApplyToken_WhenCall_SetStringWithExpiry()
        {
            // Arrange
            const string Token = "some-token";
            const string Data = "some-permission-data";
            const int Expiry = 999;

            var mockDb = new Mock<IDatabase>();
            _mockConnection.Setup(r => r.GetDatabase(-1, null)).Returns(mockDb.Object);

            var redis = new RedisProvider(_mockConnection.Object);

            // Action
            redis.ApplyToken(Token, Data, Expiry);

            // Assert
            mockDb.Verify(r => r.StringSet(Token, Data,
                TimeSpan.FromMinutes(Expiry),
                When.Always,
                CommandFlags.None));
        }

        [Fact]
        public void Authenticate_TokenInstorage_ClearTokenAndReturnTheOldOne()
        {
            // Arrange
            const string Token = "test";

            var mockDb = new Mock<IDatabase>();
            mockDb.Setup(r => r.StringGetSet(Token, string.Empty, CommandFlags.None)).Returns("hello").Verifiable();
            _mockConnection.Setup(r => r.GetDatabase(-1, null)).Returns(mockDb.Object);

            var redis = new RedisProvider(_mockConnection.Object);

            // Action
            var actual = redis.Authenticate(Token);

            // Assert
            mockDb.Verify();
            Assert.Equal("hello", actual);
        }
    }
}