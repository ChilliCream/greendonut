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

        #endregion

        #region Resolve

        [Fact(DisplayName = "Resolve: Should throw an argument null exception for value")]
        public void ResolveValueNull()
        {
            // arrange
            string value = null;

            // act
            Action verify = () => Result<string>.Resolve(value);

            // assert
            Assert.Throws<ArgumentNullException>("value", verify);
        }

        [Fact(DisplayName = "Resolve: Should not throw any exception")]
        public void ResolveValueNotNull()
        {
            // arrange
            string value = "Foo";

            // act
            Action verify = () => Result<string>.Resolve(value);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Resolve: Should return a resolved Result")]
        public void Resolve()
        {
            // arrange
            string value = "Foo";

            // act
            Result<string> result = Result<string>.Resolve(value);

            // assert
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.False(result.IsError);
            Assert.Equal("Foo", result.Value);
        }

        #endregion
    }
}
