namespace Reimers.Ihe.Communication.Tests
{
	using System.Threading.Tasks;
	using NHapi.Base.Parser;
	using NHapi.Model.V251.Message;
	using Xunit;

	public static class MiddlewareTests
	{
		public class GivenAMiddlewareWithATransactionHandler
		{
			private readonly DefaultHl7MessageMiddleware _defaultHl7MessageMiddleware;

			public GivenAMiddlewareWithATransactionHandler()
			{
				var handler = new TestTransactionHandler();
				_defaultHl7MessageMiddleware = new DefaultHl7MessageMiddleware(handlers: handler);
			}

			[Fact]
			public async Task WhenHandlingMessageThenReturnsResponse()
			{
				var generator = DefaultMessageControlIdGenerator.Instance;
				var parser = new PipeParser();
				var adt = new ADT_A01();
				adt.MSH.MessageControlID.Value = generator.NextId();

				var msg = new Hl7Message(parser.Encode(adt), "");
				var response = await _defaultHl7MessageMiddleware.Handle(msg).ConfigureAwait(false);

				Assert.NotNull(response);
			}
		}
	}

	public class TestTransactionHandler : IheTransactionHandler<ADT_A01, ACK>
	{
		public override string Handles => "ADT_A01";

		public override string Version => "2.5.1";

		protected override Task<ACK> HandleInternal(ADT_A01 message)
		{
			var result = new ACK();
			result.MSH.MessageControlID.Value = DefaultMessageControlIdGenerator.Instance.NextId();
			result.MSA.MessageControlID.Value = message.MSH.MessageControlID.Value;

			return Task.FromResult(result);
		}
	}
}