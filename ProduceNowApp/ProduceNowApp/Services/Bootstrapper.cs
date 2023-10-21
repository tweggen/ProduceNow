using Splat;

namespace ProduceNowApp.Services;

public class Bootstrapper
{
    public Bootstrapper(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.Register<Services.Database>(() => new Services.Database());
        services.Register<Services.CertificateStore>(() => new Services.CertificateStore());
        services.Register<Services.SelfSignedCert>(() => new Services.SelfSignedCert());
    }
}