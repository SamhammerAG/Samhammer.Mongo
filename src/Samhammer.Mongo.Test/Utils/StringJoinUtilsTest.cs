using Samhammer.Mongo.Utils;
using Xunit;

namespace Samhammer.Mongo.Test.Utils
{
    public class StringJoinUtilsTest
    {
        [Theory]
        [InlineData(new string[] { }, "")]
        [InlineData(new[] { "" }, "")]
        [InlineData(new[] { null, "test" }, "test")]
        [InlineData(new[] { "", "" }, "")]
        [InlineData(new[] { " " }, " ")]
        [InlineData(new[] { " ", " " }, " X ")]
        [InlineData(new[] { "test", " " }, "testX ")]
        [InlineData(new[] { " ", "test" }, " Xtest")]
        [InlineData(new[] { "test1", "test2" }, "test1Xtest2")]
        [InlineData(new[] { " test1 ", " test2 " }, " test1 X test2 ")]

        public void JoinIgnoreEmpty(string[] inputList, string expected)
        {
            var actual = StringJoinUtils.JoinIgnoreEmpty("X", inputList);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("multiChar")]
        public void JoinIgnoreEmptyTestSeparator(string separator)
        {
            var actual = StringJoinUtils.JoinIgnoreEmpty(separator, "text1", "text2");
            Assert.Equal($"text1{separator ?? string.Empty}text2", actual);
        }
    }
}
