using System;
using Xunit;

namespace GreenDonut
{
    public class ResultTests
    {
        #region Reject

        [Fact(DisplayName = "Reject: Should throw an argument null exception for error")]
        public void RejectErrorMessageNull()
        {
            // arrange
            Exception error = null;

            // act
            Action verify = () => Result<object>.Reject(error);

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
            Action verify = () => Result<object>.Reject(error);

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
            Result<string> result = Result<string>.Reject(error);

            // assert
            Assert.NotNull(result);
            Assert.True(result.IsError);
            Assert.Equal("Foo", result.Error?.Message);
            Assert.Null(result.Value);
        }

        [Fact(DisplayName = "Error: Should convert an error into an error result")]
        public void ConvertError()
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

        #endregion

        #region Resolve

        [InlineData(null)]
        [InlineData("Foo")]
        [Theory(DisplayName = "Resolve: Should return a resolved Result")]
        public void Resolve(string value)
        {
            // act
            Result<string> result = Result<string>.Resolve(value);

            // assert
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result.Value);
        }

        [InlineData(null)]
        [InlineData("Foo")]
        [Theory(DisplayName = "Resolve: Should convert a value into a value result")]
        public void ConvertValue(string value)
        {
            // act
            Result<string> result = value;

            // assert
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal(value, result.Value);
        }

        #endregion
    }
}
