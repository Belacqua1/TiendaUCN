namespace TiendaUCN.src.Application.Jobs.Interface
{
    public interface IUserJob
    {
        Task<int> DeleteUnconfirmedAsync();
    }
}
