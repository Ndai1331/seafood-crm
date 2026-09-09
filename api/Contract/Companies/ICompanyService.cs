using Contract.Companies;

namespace Contract.CompanyServices
{
    public interface ICompanyService
    {
        Task<CompanyDto> CreateAsync(CreateUpdateCompanyDto input);
        Task<CompanyDto> UpdateAsync(CreateUpdateCompanyDto input, int id);
        Task DeleteAsync(int id);
        Task<List<CompanyDto>> GetListAsync();
        

    }
}