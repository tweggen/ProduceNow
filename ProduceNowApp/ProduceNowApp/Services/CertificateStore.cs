using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace ProduceNowApp.Services;

public class CertificateStore
{
    public AsymmetricKeyParameter PrivateKey { get; set; }
    public X509Certificate Certificate { get; set; }

    
    string CertificateToString()
    {
        TextWriter textWriter = new StringWriter();
        PemWriter pemWriter = new PemWriter(textWriter);
        pemWriter.WriteObject(Certificate);
        pemWriter.Writer.Flush();
        string strCertificate = textWriter.ToString();
    }
    
    
    string PrivateKeyToString()
    {
        TextWriter textWriter = new StringWriter();
        PemWriter pemWriter = new PemWriter(textWriter);
        pemWriter.WriteObject(PrivateKey);
        pemWriter.Writer.Flush();
        string strPrivateKey = textWriter.ToString();
    }


    string CertificateFromString(string str)
    {
        TextReader textReader = new StringReader(str);
        PemReader pemReader = new PemReader(textReader);
        var pemObject = pemReader.ReadPemObject();
        switch (pemObject.Type)
        {
            case 
        }
    }
    
}