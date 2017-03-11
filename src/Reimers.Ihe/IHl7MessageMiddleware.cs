namespace Reimers.Ihe
{
    using System.Threading.Tasks;

    public interface IHl7MessageMiddleware
    {
        Task<string> Handle(Hl7Message message);
    }
}