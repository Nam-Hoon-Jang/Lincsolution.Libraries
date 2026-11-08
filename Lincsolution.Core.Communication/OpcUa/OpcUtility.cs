using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lincsolution.Core.Communication.OpcUa
{
    internal static class OpcUtility
    {
        /// <summary>
        /// NI 장치 인증서(MCP)의 경우, 인증서가 정상인데 URI불일치로 연결 안 되는 경우가 있음
        /// MCP의 인증서만 URI 불일치여도 상관없게 예외 적용하는 함수.
        /// RSA 키 크기 검사와 미신뢰 인증서 등록도 처리.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="keySize"></param>
        /// <exception cref="OpcClientException"></exception>
        public static void AttachCertificateValidationHandler(ApplicationConfiguration configuration, ITelemetryContext telemetry, ushort keySize)
        {
            configuration.CertificateValidator.CertificateValidation += (s, e) =>
            {
                var cert = e.Certificate as X509Certificate2;

                using(RSA rsaPublicKey = cert?.GetRSAPublicKey())
                {
                    int? currentKeySize = rsaPublicKey?.KeySize;

                    Debug.WriteLine(
                        $"[OPC UA Certificate Validation] " +
                        $"Status={e.Error.StatusCode} " +
                        $"Subject={cert?.Subject} " +
                        $"Thumbprint={cert?.Thumbprint} " +
                        $"KeySize={currentKeySize}");

                    if(cert == null)
                    {
                        return;
                    }
                    if(currentKeySize == null || currentKeySize < keySize)
                    {
                        Debug.WriteLine($"[OPC UA] Rejected server certificate. RSA key size policy failed. Required={keySize}, Actual={currentKeySize?.ToString() ?? "unknown"}");
                        e.Accept = false;
                        return;
                    }
                    if(e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                    {
                        Debug.WriteLine("[OPC UA] Auto-accepting untrusted server certificate for equipment provisioning/trust-on-first-use.");
                        try
                        {
                            using(var peerStore = configuration.SecurityConfiguration.TrustedPeerCertificates.OpenStore(telemetry))
                            {
                                peerStore.AddAsync(cert).GetAwaiter().GetResult();

                                Debug.WriteLine("[OPC UA] Added server certificate to TrustedPeer store.");
                            }
                        }
                        catch(Exception ex)
                        {
                            e.Accept = false;
                            throw new OpcClientException(OpcClientExceptionType.UnknownException, "[OPC UA] NI Device:: Failed to add to TrustedPeer store: ", ex);
                        }

                        e.Accept = true;
                        return;
                    }
                    if(e.Error.StatusCode == StatusCodes.BadCertificateUriInvalid)
                    {
                        if(IsKnownMcpNiUriMismatchCertificate(cert))
                        {
                            // MCP/NI 서버 인증서 URI mismatch 는 현장 장비 특성상 의도적으로 허용한다.
                            Debug.WriteLine("[OPC UA] Accepted known MCP/NI server certificate despite URI mismatch.");
                            e.Accept = true;
                            return;
                        }

                        Debug.WriteLine("[OPC UA] Rejected certificate URI mismatch. Certificate is not known MCP/NI exception target.");
                        e.Accept = false;
                        return;
                    }
                }
            };
        }
        private static bool IsKnownMcpNiUriMismatchCertificate(X509Certificate2 cert)
        {
            try
            {
                IReadOnlyList<string> applicationUris = X509Utils.GetApplicationUrisFromCertificate(cert);
                foreach(string applicationUri in applicationUris)
                {
                    if(applicationUri.IndexOf("National Instruments", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("[OPC UA] Failed to inspect certificate application URI: " + ex);
            }

            return cert.GetNameInfo(X509NameType.SimpleName, false)
                .Equals("NI OPC UA App", StringComparison.OrdinalIgnoreCase);
        }
    }
}
