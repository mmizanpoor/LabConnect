using AutoMapper;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.MapperConfig;

public static class ApplicationMapper
{
    private static readonly Lazy<IMapper> Lazy = new(() =>
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
