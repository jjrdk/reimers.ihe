// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hl7Message.cs" company="Reimers.dk">
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
    /// <summary>
    /// Defines the container for received HL7 content.
    /// </summary>
    public class Hl7Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hl7Message"/> class.
        /// </summary>
        /// <param name="message">The raw HL7 message.</param>
        /// <param name="sourceAddress">The address the message was received from.</param>
        public Hl7Message(string message, string sourceAddress)
        {
            Message = message;
            SourceAddress = sourceAddress;
        }

        /// <summary>
        /// Gets the address the message was received from.
        /// </summary>
        public string SourceAddress { get; }

        /// <summary>
        /// Gets the raw received HL7 message.
        /// </summary>
        public string Message { get; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (SourceAddress + Message).GetHashCode();
        }
    }
}