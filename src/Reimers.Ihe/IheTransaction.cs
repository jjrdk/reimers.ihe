namespace Reimers.Ihe
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using NHapi.Base.Model;
    using NHapi.Base.Parser;

    public abstract class IheTransaction<TSend, TReceive>
        where TSend : IMessage
        where TReceive : IMessage
    {
        private readonly Func<Task<IMllpConnection>> _connectionFactory;
        private readonly PipeParser _parser;

        protected IheTransaction(Func<Task<IMllpConnection>> connectionFactory, PipeParser parser)
        {
            _connectionFactory = connectionFactory;
            _parser = parser;
        }

        public async Task<TReceive> Send(TSend message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var hl7 = _parser.Encode(message);
            using (var connection = await _connectionFactory())
            {
                var response = await connection.Send(hl7, cancellationToken);
                Trace.TraceInformation(response.Message);
                var receive = (TReceive)_parser.Parse(response.Message);

                return receive;
            }
        }

        protected virtual Task<TSend> ConfigureHeaders(TSend message)
        {
            return Task.FromResult(message);
        }
    }
}