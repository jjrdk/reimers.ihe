// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMessageControlIdGenerator.cs" company="Reimers.dk">
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

	/// <summary>
	/// Defines the default message control id generator.
	/// </summary>
	/// <remarks>The generated id is thread safe and unique within a single application.</remarks>
	public class DefaultMessageControlIdGenerator : IMessageControlIdGenerator
	{
		private int _seed;
		private static readonly object SyncRoot = new object();

		private DefaultMessageControlIdGenerator()
		{
		}

		/// <summary>
		/// Gets the singleton instanceof the id generator.
		/// </summary>
		public static DefaultMessageControlIdGenerator Instance { get; } = new DefaultMessageControlIdGenerator();

		/// <inheritdoc />
		public string NextId()
		{
			lock (SyncRoot)
			{
				var counter = ++_seed;
				if (counter >= 999999)
				{
					_seed = 0;
				}
				return DateTime.UtcNow.ToString("yyyyMMddHHmmss") + counter.ToString("D6");
			}
		}
	}
}