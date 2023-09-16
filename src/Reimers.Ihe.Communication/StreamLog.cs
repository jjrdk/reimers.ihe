// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamLog.cs" company="Reimers.dk">
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

using System.Buffers;

namespace Reimers.Ihe.Communication
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Abstractions;

    /// <summary>
	/// Defines a log implementation which outputs to a <see cref="Stream"/>.
	/// </summary>
	public class StreamLog : IMessageLog, IAsyncDisposable
    {
        private readonly StreamWriter _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamLog"/> class.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="encoding"></param>
        public StreamLog(Stream output, Encoding? encoding = null)
        {
            _output = new StreamWriter(output, encoding ?? Encoding.UTF8);
        }

        /// <inheritdoc />
        public Task Write(ReadOnlyMemory<char> msg)
        {
            return _output.WriteAsync(msg);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _output.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
