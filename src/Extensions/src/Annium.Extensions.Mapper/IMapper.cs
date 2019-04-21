namespace Annium.Extensions.Mapper
{
    public interface IMapper
    {
        T Map<T>(object source);
    }
}