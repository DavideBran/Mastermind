using fileManager;
public class UserDataManager : IRepository<User>
{

    private FileManager _fileManager;
    private List<User> _users = new();

    private string _userListPath;
    public UserDataManager()
    {   
        _userListPath = "Users.json";
        _fileManager = new FileManager("AuthenticationSeriviceServer");
        Upload();
    }

    public User Get(string email)
    {
        User retrivedUser = _users.Find((user) => user.Email == email) ?? throw new UserNotRegistered();
        return retrivedUser;
    }

    public IEnumerable<User> GetAll()
    {
        return _users;
    }

    public void Insert(User user)
    {
        _users.Add(user);

        SaveUserList();
    }

    public void RemoveUser(User user)
    {
        if (!_users.Contains(user)) throw new UserNotRegistered();
        _users.Remove(user);

        Save();
    }

    public void Save()
    {
        SaveUserList();
    }

    private void SaveUserList()
    {
        _fileManager.SaveList(_users, _userListPath);
    }

    public void Upload()
    {
        _users = (List<User>)_fileManager.PoPList<User>(_userListPath) ?? throw new NullReferenceException();
    }
}