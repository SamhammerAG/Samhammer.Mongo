using System;
using FluentAssertions;
using Samhammer.Mongo.Abstractions;
using Samhammer.Mongo.Utils;
using Xunit;

namespace Samhammer.Mongo.Test
{
    public class MongoDbUtilsTest
    {
        [Theory]
        [InlineData(typeof(TestDataModel), "testData")]
        [InlineData(typeof(TestDataModelWithCustomName), "TestDataModel")]
        private void GetCollectionName(Type modelType, string collectionName)
        {
            var name = MongoDbUtils.GetCollectionName(modelType);
            name.Should().Be(collectionName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("1234", false)]
        [InlineData("abcd", false)]
        [InlineData("1a3d", false)]
        [InlineData("5c9e1ae71d07dc6a14617513", true)]
        private void IsValidObjectId(string id, bool expected)
        {
            var result = MongoDbUtils.IsValidObjectId(id);
            result.Should().Be(expected);
        }

        [MongoCollection]
        private class TestDataModel
        {
        }

        [MongoCollection("TestDataModel")]
        private class TestDataModelWithCustomName
        {
        }
    }
}
