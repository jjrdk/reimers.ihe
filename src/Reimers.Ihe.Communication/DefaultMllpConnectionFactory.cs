// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMllpConnectionFactory.cs" company="Reimers.dk">
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
	using System.Net.Security;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Defines the default MLLP connection factory.
	/// </summary>
	public class DefaultMllpConnectionFactory
	{
		private readonly string _address;
		private readonly int _port;
		private readonly IMessageLog _messageLog;
		private readonly Encoding _encoding;
		private readonly X509CertificateCollection _clientCertificateCollection;
		private readonly RemoteCertificateValidationCallback _userCertificateValidationCallback;

		/// <summary>
		/// Iniitializes a new instance of the <see cref="DefaultMllpConnectionFactory"/> class.
		/// </summary>
		/// <param name="address">The address of the remote server.</param>
		/// <param name="port">The port of the remote server.</param>
		/// <param name="messageLog">The message logger</param>
		/// <param name="encoding">The <see cref="Encoding"/> to use for data transfer. If no value is specified, then <see cref="Encoding.ASCII"/> is used.</param>
		/// <param name="clientCertificateCollection">The client certificates to use for connection security.</param>
		/// <param name="userCertificateValidationCallback">Optional call to validate user remote certificate.</param>
		public DefaultMllpConnectionFactory(string address, int port, IMessageLog messageLog = null, Encoding encoding = null,
			 X509CertificateCollection clientCertificateCollection = null,
			 RemoteCertificateValidationCallback userCertificateValidationCallback = null)
		{
			_address = address;
			_port = port;
			_messageLog = messageLog ?? NullLog.Get();
			_encoding = encoding;
			_clientCertificateCollection = clientCertificateCollection;
			_userCertificateValidationCallback = userCertificateValidationCallback;
		}

		/// <summary>
		/// Gets an instance of an MLLP connection.
		/// </summary>
		/// <returns>An MLLP connection as an async operation.</returns>
		public Task<IHostConnection> Get()
		{
			return MllpClient.Create(_address, _port, _messageLog, _encoding, _clientCertificateCollection, _userCertificateValidationCallback);
		}
	}
}