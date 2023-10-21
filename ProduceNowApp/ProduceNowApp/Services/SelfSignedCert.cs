using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using SIPSorcery.Net;

namespace ProduceNowApp.Services;

public class SelfSignedCert
{
    public AsymmetricKeyParameter PrivateKey;
    public Org.BouncyCastle.X509.X509Certificate Certificate;
    
    public SelfSignedCert()
    {
        Console.WriteLine("Generating self signed certificate...");
        (Certificate, PrivateKey) = DtlsUtils.CreateSelfSignedBouncyCastleCert();
        //RtcCertificate2 = new() { Certificate = certificate, PrivateKey = privateKey };
        Console.WriteLine("... done creating certificate.");
    }
}