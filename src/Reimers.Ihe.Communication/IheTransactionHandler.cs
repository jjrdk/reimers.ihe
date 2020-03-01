// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IheTransactionHandler.cs" company="Reimers.dk">
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
    using System.Threading;
    using System.Threading.Tasks;
    using NHapi.Base.Model;

    /// <summary>
    /// Defines the abstract IHE transaction handler.
    /// </summary>
    /// <typeparam name="TMessage">The message type to handle.</typeparam>
    /// <typeparam name="TResponse">The response message.</typeparam>
    public abstract class IheTransactionHandler<TMessage, TResponse> : IIheTransactionHandler
         where TMessage : class, IMessage
         where TResponse : class, IMessage
    {
        /// <inheritdoc />
        public abstract string Handles { get; }

        /// <inheritdoc />
        public abstract string Version { get; }

        /// <inheritdoc />
        public async Task<IMessage> Handle(IMessage message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var msg = (TMessage)message;
            var (verified, response1) = await VerifyIncomingMessage(msg, cancellationToken).ConfigureAwait(false);
            var response = response1;
            if (verified)
            {
                response = await HandleInternal(msg).ConfigureAwait(false);
            }
            return await ConfigureHeaders(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Defines a default message header configuration for the transaction.
        /// </summary>
        /// <param name="message">The message whose header should be configured.</param>
        /// <returns>The message with configured header.</returns>
        protected virtual Task<TResponse> ConfigureHeaders(TResponse message)
        {
            return Task.FromResult(message);
        }

        /// <summary>
        /// Verifies the contents of the incoming messages.
        /// </summary>
        /// <param name="message">The incoming message to verify.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>
        /// <para>If the incoming message cannot be verified, the method returns <c>false</c> as status and an error response message.</para>
        /// <para>If the verification is successful, the method returns <c>true</c> and them response message is null.</para>
        /// </returns>
        protected virtual Task<(bool verified, TResponse response)> VerifyIncomingMessage(TMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult<(bool, TResponse)>((true, null));
        }

        /// <summary>
        /// The concrete method for handling specific messages.
        /// </summary>
        /// <param name="message">The concrete message to handle.</param>
        /// <returns>The response message as an asynchronous operation.</returns>
        protected abstract Task<TResponse> HandleInternal(TMessage message);
    }
}