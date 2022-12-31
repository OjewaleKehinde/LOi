using AutoMapper;
using LOi.Models;

namespace LOi{
    public class AutoMapperProfiles : Profile{
        public AutoMapperProfiles()
        {
            //Default mapping when property namesare same
            //Mapping source to destination
            CreateMap<User, UserDto>();
            CreateMap<Order, OrderCreation>();
            CreateMap<OrderCreation, Order>();

            CreateMap<User, LoginDto>();
            CreateMap<Admin, AdminDto>();
            CreateMap<Order, AdminOrderCreation>();
            CreateMap<Order, AdminUpdateOrder>();
            // CreateMap<Order, AdminUpdateOrder>();
            // CreateMap<Order, AdminUpdateOrder>();
            CreateMap<Order, UpdateOrder>();

            
        }
    }
}