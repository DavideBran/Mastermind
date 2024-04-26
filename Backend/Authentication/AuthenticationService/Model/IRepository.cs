public interface IRepository<T>
{
    public void Save();
    public void Upload();
    public T Get(string id);
    public IEnumerable<T> GetAll();
    public void Insert(T element);
}