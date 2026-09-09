using AutoMapper;
using Contract.Identity.UserManager;
using Domain.Identity.Users;

namespace Application
{
    public class ServiceBase
    {
        protected IMapper ObjectMapper { get;}
        
        public ServiceBase()
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new AutoMapperProfile());
                });

                ObjectMapper = config.CreateMapper();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          
        }


        public void AttachIndex(dynamic inputs)
        {
            var index = 1;
            foreach (var item in inputs)
            {
                item.Index = index;
                index++;
            }
            
        }

      
    }
}