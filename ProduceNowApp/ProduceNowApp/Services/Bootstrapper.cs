using Splat;

namespace ProduceNowApp.Services;

public class Bootstrapper
{
    public Bootstrapper(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<Services.Database>(() => new Services.Database());
        services.RegisterLazySingleton<Services.CertificateStore>(() => new Services.CertificateStore());
        services.RegisterLazySingleton<Services.SelfSignedCert>(() => new Services.SelfSignedCert());
    }
}