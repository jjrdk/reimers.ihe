namespace Reimers.Ihe.Communication
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using NHapi.Base.Model;
    using NHapi.Base.Parser;

    /// <summary>
    /// Defines the abstract base class for IHE transactions.
    /// </summary>
    /// <typeparam name="TSend">The IHE message to send.</typeparam>
    /// <typeparam name="TReceive">The IHE message to receive.</typeparam>
    public abstract class IheTransaction<TSend, TReceive>
        where TSend : IMessage
        where TReceive : IMessage
    {
        private readonly Func<Task<IHostConnection>> _connectionFactory;
        private readonly PipeParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="IheTransaction{TSend,TReceive}"/> class.
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="parser"></param>
        protected IheTransaction(Func<Task<IHostConnection>> connectionFactory, PipeParser parser)
        {
            _connectionFactory = connectionFactory;
            _parser = parser;
        }

        /// <summary>
        /// Sends the passed message and awaits the response from the server.
        /// If the server response cannot be parsed as the expected response an exception will be thrown.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the transaction.</param>
        /// <returns>The response message as an asynchronous operation.</returns>
        public async Task<TReceive> Send(TSend message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var hl7 = _parser.Encode(message);
            using (var connection = await _connectionFactory().ConfigureAwait(false))
            {
                var response = await connection.Send(hl7, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation(response.Message);
                var receive = (TReceive) _parser.Parse(response.Message);
                return receive;
            }
        }

        /// <summary>
        /// Defines a default message header configuration for the transaction.
        /// </summary>
        /// <param name="message">The message whose header should be configured.</param>
        /// <returns>The message with configured header.</returns>
        protected virtual Task<TSend> ConfigureHeaders(TSend message)
        {
            return Task.FromResult(message);
        }
    }
}