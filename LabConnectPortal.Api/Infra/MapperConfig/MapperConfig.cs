using AutoMapper;
using LabConnectPortal.Domain;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.MapperConfig
{
    public static class MapperConfig
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TestECL, TestECLViewModel>().ReverseMap();
                cfg.CreateMap<TestJoze3, TestECLViewModel>().ReverseMap();
            });

            return config.CreateMapper();
        });

        public static IMapper Mapper => Lazy.Value;
    }
}
