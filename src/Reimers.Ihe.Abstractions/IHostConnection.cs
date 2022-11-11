// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHostConnection.cs" company="Reimers.dk">
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

namespace Reimers.Ihe.Abstractions
{
    using NHapi.Base.Model;

    /// <summary>
    /// Defines the interface for a connection to an IHE host.
    /// </summary>
    public interface IHostConnection : IAsyncDisposable
    {
        /// <summary>
        /// Sends the passed message to the server and awaits the response.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>An <see cref="Reimers.Ihe.Abstractions.Hl7Message"/> containing the response and source address.</returns>
        Task<Hl7Message> Send<TMessage>(
            TMessage message,
            CancellationToken cancellationToken = default)
            where TMessage : IMessage;
    }
}
