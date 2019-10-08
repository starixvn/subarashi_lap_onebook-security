using System;
using LAP.ONEBook.Security.StorageProvider.Redis;
using Moq;
using StackExchange.Redis;
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
        public void ApplyTokenAsync_WhenCall_SetStringWithExpiry()
        {
            // Arrange
            const string Token = "test-async-token";
            const string Data = "async-permission-data";
            const int Expiry = 888;

            var mockDb = new Mock<IDatabase>();
            _mockConnection.Setup(r => r.GetDatabase(-1, null)).Returns(mockDb.Object);

            var redis = new RedisProvider(_mockConnection.Object);

            // Action
            redis.ApplyTokenAsync(Token, Data, Expiry);

            // Assert
            mockDb.Verify(r => r.StringSetAsync(Token, Data,
                TimeSpan.FromMinutes(Expiry),
                When.Always,
                CommandFlags.None));
        }

        [Fact]
        public void Authenticate_TokenInStorage_ClearTokenAndReturnTheOldOne()
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

        [Fact]
        public async void AuthenticateAsync_TokenInStorage_ClearTokenAndReturnTheOldOne()
        {
            // Arrange
            const string Token = "test-async";

            var mockDb = new Mock<IDatabase>();
            mockDb.Setup(r => r.StringGetSetAsync(Token, string.Empty, CommandFlags.None)).ReturnsAsync("hello-async").Verifiable();
            _mockConnection.Setup(r => r.GetDatabase(-1, null)).Returns(mockDb.Object);

            var redis = new RedisProvider(_mockConnection.Object);

            // Action
            var actual = await redis.AuthenticateAsync(Token);

            // Assert
            mockDb.Verify();
            Assert.Equal("hello-async", actual);
        }
    }
}