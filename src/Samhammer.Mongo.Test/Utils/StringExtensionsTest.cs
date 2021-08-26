using FluentAssertions;
using Samhammer.Mongo.Utils;
using Xunit;

namespace Samhammer.Mongo.Test.Utils
{
    public class StringExtensionsTest
    {
        [Theory]
        [InlineData("UserModel", "Model", "User")]
        [InlineData("abc", "b", "ac")]
        public void RemoveString(string input, string remove, string expected)
        {
            var result = input.RemoveString(remove);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("UserModel", "userModel")]
        [InlineData("U", "u")]
        public void ToLowerFirstChar(string input, string expected)
        {
            var result = input.ToLowerFirstChar();
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("ABCD-8765-feature-branch-name-too-long", 29, "ABCD-8765-feature-branch-name")]
        [InlineData("ABCD-8765-feature-branch-name-", 29, "ABCD-8765-feature-branch-name")]
        [InlineData("ABCD-8765-feature-branch-name", 29, "ABCD-8765-feature-branch-name")]
        [InlineData("ABCD-8765-feature-branch-nam", 29, "ABCD-8765-feature-branch-nam")]
        [InlineData("ABCD-8765-feature-branch-name", 0, "")]
        [InlineData(null, 29, null)]

        public void Truncate(string input, int truncateValue, string expected)
        {
            var result = input.Truncate(truncateValue);
            result.Should().Be(expected);
        }
    }
}
