// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIheTransactionHandler.cs" company="Reimers.dk">
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
    /// Defines the interface for handling IHE transactions.
    /// </summary>
    public interface IIheTransactionHandler
    {
        /// <summary>
        /// Gets the name of the message structure that is handled.
        /// </summary>
        string Handles { get; }

        /// <summary>
        /// Gets the version of the message that is handled.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Handles the received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> for the async operation.</param>
        /// <returns>The response message.</returns>
        Task<IMessage> Handle(IMessage message, CancellationToken cancellationToken = default);
    }
}