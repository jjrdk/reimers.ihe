// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMiddleware.cs" company="Reimers.dk">
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
    using System.Threading;
    using System.Threading.Tasks;
    using NHapi.Base.Model;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;

    public class TestMiddleware : IHl7MessageMiddleware
    {
        private readonly PipeParser _parser = new PipeParser();

        public Task<IMessage> Handle(
            Hl7Message message,
            CancellationToken cancellation = default)
        {
            var controlId = message.Message.GetMessageControlId();
            var ack = new ACK();
            ack.MSH.MessageControlID.Value = controlId;
            return Task.FromResult<IMessage>(ack);
        }
    }
}
