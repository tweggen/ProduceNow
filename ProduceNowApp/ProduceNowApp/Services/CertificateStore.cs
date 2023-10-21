using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using SIPSorcery.Net;
using Splat;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ProduceNowApp.Services;

public class CertificateStore
{
    private Database _database = Locator.Current.GetService<Database>();
    
    public AsymmetricKeyParameter PrivateKey { get; set; }
    public X509Certificate Certificate { get; set; }


    public RTCCertificate2 CreateRtcCertificate2()
    {
        return new()
        {
            Certificate = Certificate,
            PrivateKey = PrivateKey
        };
    }
    
    
    string CertificateToString()
    {
        TextWriter textWriter = new StringWriter();
        PemWriter pemWriter = new PemWriter(textWriter);
        pemWriter.WriteObject(Certificate);
        pemWriter.Writer.Flush();
        string strCertificate = textWriter.ToString();
        return strCertificate;
    }
    
    
    string PrivateKeyToString()
    {
        TextWriter textWriter = new StringWriter();
        PemWriter pemWriter = new PemWriter(textWriter);
        pemWriter.WriteObject(PrivateKey);
        pemWriter.Writer.Flush();
        string strPrivateKey = textWriter.ToString();
        return strPrivateKey;
    }


    X509Certificate CertificateFromString(string str)
    {
        TextReader textReader = new StringReader(str);
        PemReader pemReader = new PemReader(textReader);
        return new X509Certificate(pemReader.ReadPemObject().Content);
    }

    
    AsymmetricKeyParameter PrivateKeyFromString(string str)
    {
        TextReader textReader = new StringReader(str);
        PemReader pemReaer = new PemReader(textReader);
        return (Org.BouncyCastle.Crypto.AsymmetricKeyParameter)pemReaer.ReadObject();
    }


    private void _storeCurrentKeys()
    {
        var clientConfig = _database.ClientConfig;
        clientConfig.CertificateString = CertificateToString();
        clientConfig.PrivateKeyString = PrivateKeyToString();
        _database.SaveClientConfig();
        Console.WriteLine("Updated certificate.");
    }
    
    
    private void _fillDefault()
    {
        var clientConfig = _database.ClientConfig;

        AsymmetricKeyParameter? privateKey = null;
        X509Certificate? certificate = null;
        bool storeKeys = false;
        
        /*
         * First try a previously serialized 
         */

        if (null==privateKey || null==certificate) {
            try
            {
                string privateKeyString = clientConfig.PrivateKeyString;
                string certificateString = clientConfig.CertificateString;
                if (!string.IsNullOrEmpty(privateKeyString) || !string.IsNullOrEmpty(certificateString))
                {
                    privateKey = PrivateKeyFromString(privateKeyString);
                    certificate = CertificateFromString(certificateString);
                    Console.WriteLine("Restored keys from database.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to restore keys from database: Exception {e}");
                privateKey = null;
                certificate = null;
            }
        }
        
        if (null==privateKey || null==certificate) {
            var selfSigned = Locator.Current.GetService<SelfSignedCert>();
            privateKey = selfSigned.PrivateKey;
            certificate = selfSigned.Certificate;
            storeKeys = true;
        }

        if (null == privateKey || null == certificate)
        {
            throw new InvalidOperationException("Unable to obtain keys.");
        }

        PrivateKey = privateKey;
        Certificate = certificate;
        if (storeKeys)
        {
            _storeCurrentKeys();
        }
    }


    public CertificateStore()
    {
        _fillDefault();
    }
}