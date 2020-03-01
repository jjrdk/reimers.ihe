// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHl7MessageMiddleware.cs" company="Reimers.dk">
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

    /// <summary>
    /// Defines the HL7 handling interface.
    /// </summary>
    public interface IHl7MessageMiddleware
    {
        /// <summary>
        /// Handles the passed <see cref="Hl7Message"/> message.
        /// </summary>
        /// <param name="message">The <see cref="Hl7Message"/> to handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>An HL7 response as a <see cref="string"/>.</returns>
        Task<string> Handle(Hl7Message message, CancellationToken cancellationToken = default);
    }
}