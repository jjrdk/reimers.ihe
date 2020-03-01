namespace Reimers.Ihe.Communication.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public static class DefaultMessageControlIdGeneratorTests
    {
        public class GivenADefaultMessageControlIdGenerator
        {
            [Fact]
            public void WhenGeneratingIdThenLengthIsLessThan20Chars()
            {
                var generator = DefaultMessageControlIdGenerator.Instance;
                var id = generator.NextId();

                Assert.Equal(20, id.Length);
            }

            [Fact]
            public async Task
                WhenGeneratingIdsOnMultipleThreadsThenDoesNotCreateDuplicates()
            {
                var tasks = Enumerable.Repeat(false, 10000)
                    .Select(
                        _ => Task.Run(
                            () =>
                            {
                                return Enumerable.Range(0, 100)
                                    .Select(
                                        x => DefaultMessageControlIdGenerator
                                            .Instance.NextId());
                            }));
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                var ids = results.SelectMany(x => x).ToList();

                Assert.Equal(ids.Count, ids.Distinct().Count());
            }
        }
    }
}