using System;
using Xunit;

namespace GreenDonut
{
    public class ResultTests
    {
        #region Rejected Result

        [Fact(DisplayName = "Reject: Should throw an argument null exception for error")]
        public void RejectErrorMessageNull()
        {
            // arrange
            Exception error = null;

            // act
            Action verify = () => { Result<object> result = error; };

            // assert
            Assert.Throws<ArgumentNullException>("error", verify);
        }

        [Fact(DisplayName = "Reject: Should not throw any exception")]
        public void RejectErrorMessageNotNull()
        {
            // arrange
            var errorMessage = "Foo";
            var error = new Exception(errorMessage);

            // act
            Action verify = () => { Result<object> result = error; };

            // assert
            Assert.Null(Record.Exception(verify));
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
            Assert.NotNull(result);
            Assert.True(result.IsError);
            Assert.Equal("Foo", result.Error?.Message);
            Assert.Null(result.Value);
        }

        [Fact(DisplayName = "Reject (Obsolete): Should convert an error into an error result")]
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
            Assert.NotNull(result);
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
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result.Value);
        }

        [InlineData(null)]
        [InlineData("Foo")]
        [Theory(DisplayName = "Resolve (Obsolete): Should convert a value into a value result")]
        public void DeprecatedResolveStillNeedsToBeTested(string value)
        {
            // act
#pragma warning disable CS0618 // Type or member is obsolete
            var result = Result<string>.Resolve(value);
#pragma warning restore CS0618 // Type or member is obsolete

            // assert
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result.Value);
        }

        #endregion
    }
}
