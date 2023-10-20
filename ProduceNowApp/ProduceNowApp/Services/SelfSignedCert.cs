using System;
using Org.BouncyCastle.Crypto;
using SIPSorcery.Net;

namespace ProduceNowApp.Services;

public class SelfSignedCert
{
    public RTCCertificate2 RtcCertificate2;
    
    private SelfSignedCert()
    {
        Console.WriteLine("Generating self signed certificate...");
        Org.BouncyCastle.X509.X509Certificate certificate;
        AsymmetricKeyParameter privateKey;
        (certificate, privateKey) = DtlsUtils.CreateSelfSignedBouncyCastleCert();
        RtcCertificate2 = new() { Certificate = certificate, PrivateKey = privateKey };
        Console.WriteLine("... done creating certificate.");
    }
    static public SelfSignedCert Instance = new ();
}