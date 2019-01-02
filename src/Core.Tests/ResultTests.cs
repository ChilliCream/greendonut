using System;
using Xunit;

namespace GreenDonut
{
    public class ResultTests
    {
        #region Rejected Result

        [Fact(DisplayName = "Reject: Should return a resolved Result if error is null")]
        public void RejectErrorIsNull()
        {
            // arrange
            Exception error = null;

            // act
            Result<object> result = error;

            // assert
            Assert.False(result.IsError);
            Assert.Null(result.Error);
            Assert.Null(result.Value);
        }

        [Fact(DisplayName = "Reject: Should return a rejected Result")]
        public void Reject()
        {
            // arrange
            var errorMessage = "Foo";
            var error = new Exception(errorMessage);

            // act
            Result<string> result = error;

            // assert
            Assert.True(result.IsError);
            Assert.Equal(error, result.Error);
            Assert.Null(result.Value);
        }

        [Fact(DisplayName = "Reject (Obsolete): Should return a rejected Result")]
        public void DeprecatedRejectStillNeedsToBeTested()
        {
            // arrange
            var errorMessage = "Foo";
            var error = new Exception(errorMessage);

            // act
#pragma warning disable CS0618 // Type or member is obsolete
            var result = Result<string>.Reject(error);
#pragma warning restore CS0618 // Type or member is obsolete

            // assert
            Assert.True(result.IsError);
            Assert.Equal("Foo", result.Error?.Message);
            Assert.Null(result.Value);
        }

        #endregion

        #region Resolved Result

        [InlineData(null)]
        [InlineData("Foo")]
        [Theory(DisplayName = "Resolve: Should return a resolved Result")]
        public void Resolve(string value)
        {
            // act
            Result<string> result = value;

            // assert
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result);
        }

        private object List<T>()
        {
            throw new NotImplementedException();
        }

        [InlineData(null)]
        [InlineData("Foo")]
        [Theory(DisplayName = "Resolve (Obsolete): Should return a resolved Result")]
        public void DeprecatedResolveStillNeedsToBeTested(string value)
        {
            // act
#pragma warning disable CS0618 // Type or member is obsolete
            var result = Result<string>.Resolve(value);
#pragma warning restore CS0618 // Type or member is obsolete

            // assert
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result.Value);
        }

        #endregion
    }
}
