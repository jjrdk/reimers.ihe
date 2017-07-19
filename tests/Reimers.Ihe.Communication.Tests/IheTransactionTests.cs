// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IheTransactionTests.cs" company="Reimers.dk">
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

namespace Reimers.Ihe.Communication.Tests
{
	using System;
	using System.Net;
	using System.Threading.Tasks;
	using NHapi.Base.Parser;
	using NHapi.Model.V251.Message;
	using Xunit;

	public class IheTransactionTests : IDisposable
	{
		private readonly MllpServer _server;

		private readonly int _port = 2575;

		public IheTransactionTests()
		{
			_server = new MllpServer(
				new IPEndPoint(IPAddress.Loopback, _port),
				NullLog.Get(),
				new TestMiddleware());
			_server.Start();
		}

		[Fact]
		public async Task WhenSendingMessageThenGetsAck()
		{
			IMessageControlIdGenerator generator = DefaultMessageControlIdGenerator.Instance;
			var connectionFactory =
				new DefaultMllpConnectionFactory(IPAddress.Loopback.ToString(), _port);
			var client = new TestTransaction(connectionFactory.Get, new PipeParser());
			var request = new QBP_Q11();
			request.MSH.MessageControlID.Value = generator.NextId();
			var response = await client.Send(request).ConfigureAwait(false);
			Assert.NotNull(response);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_server?.Dispose();
		}
	}
}
