using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;

namespace LibraryManagement.AutoMappper
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<GetAllBooks,Book>().ReverseMap();
            CreateMap<UserLogin,User>().ReverseMap();
            CreateMap<UserRegistration,User>().ReverseMap();    
            
        }
    }
}
