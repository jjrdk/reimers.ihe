namespace Reimers.Ihe.Communication.Tests
{
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
		}
	}
}