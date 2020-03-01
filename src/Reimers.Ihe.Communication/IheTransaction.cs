// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IheTransaction.cs" company="Reimers.dk">
//   Copyright © Reimers.dk 2017
//   This source is subject to the MIT License.
//   Please see https://opensource.org/licenses/MIT for details.
//   All other rights reserved.
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        public async Task<TReceive> Send(TSend message, CancellationToken cancellationToken = default)
        {
            message = await ConfigureHeaders(message).ConfigureAwait(false);
            var hl7 = _parser.Encode(message);

            cancellationToken.ThrowIfCancellationRequested();
            using var connection = await _connectionFactory().ConfigureAwait(false);
            var response = await connection.Send(hl7, cancellationToken).ConfigureAwait(false);
            Trace.TraceInformation(response.Message);
            return (TReceive)_parser.Parse(response.Message);
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