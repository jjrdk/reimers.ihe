namespace Reimers.Ihe
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IHl7MessageHandler
    {
        IEnumerable<string> Handles { get; }

        Task<string> Handle(Hl7Message message);
    }
}