namespace SqlServ4r.Repository.Positions
{
    public interface IPositionRepository
    {
        (bool ExistCode, bool ExistName) CheckDuplicateInformation(string code,string name);

    }
}