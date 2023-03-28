using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Samhammer.Mongo
{
    public static class AggregateFluentExtensions
    {
        public static IAggregateFluent<TResult> SortIfNotNull<TResult>(this IAggregateFluent<TResult> aggregate, SortDefinition<TResult> sort)
        {
            return aggregate.If(() => sort != null, x => x.Sort(sort));
        }

        public static IAggregateFluent<TResult> If<TResult>(this IAggregateFluent<TResult> aggregate, Func<bool> condition, Func<IAggregateFluent<TResult>, IAggregateFluent<TResult>> setup)
        {
            return condition() ? setup(aggregate) : aggregate;
        }

        public static IAggregateFluent<BsonDocument> AppendStageDynamic<TResult>(this IAggregateFluent<TResult> aggregate, Func<IAggregateFluent<BsonDocument>, IAggregateFluent<BsonDocument>> func)
        {
            return func(aggregate.As<BsonDocument>());
        }
    }
}
