// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text;
using HandlebarsDotNet;
using Microsoft.Atlas.CommandLine.Secrets;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Microsoft.Atlas.CommandLine.Templates.Helpers
{
    public class SshHelpers : TemplateHelperProvider
    {
        private readonly ISecretTracker _secretTracker;

        public SshHelpers(ISecretTracker secretTracker)
        {
            _secretTracker = secretTracker;
        }

        [HandlebarsDotNet.Description("sshkeygen")]
        public void SshKeyGen(TextWriter output, dynamic context, params object[] arguments)
        {
            var keyContext = GenerateSshKey();
        }

        [HandlebarsDotNet.Description("sshkeygen")]
        public void SshKeyGen(TextWriter output, HelperOptions options, dynamic context, params object[] arguments)
        {
            var keyContext = GenerateSshKey();
            options.Template(output, keyContext);
        }

        private KeyContext GenerateSshKey()
        {
            var random = new SecureRandom(new CryptoApiRandomGenerator());
            var strength = 2048;

            var parameters = new KeyGenerationParameters(random, strength);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(parameters);
            var keyPair = keyPairGenerator.GenerateKeyPair();

            var keyContext = new KeyContext();

            // var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            // var privateKey = Convert.ToBase64String(privateKeyInfo.GetEncoded(Asn1Encodable.Ber));
            using (var writer = new StringWriter())
            {
                var pem = new PemWriter(writer);
                pem.WriteObject(keyPair.Private);
                keyContext.PrivateKey = writer.ToString();
            }

            var publicKeyParameter = (RsaKeyParameters)keyPair.Public;
            using (var memory = new MemoryStream())
            {
                WriteBytes(memory, Encoding.ASCII.GetBytes("ssh-rsa"));
                WriteBytes(memory, publicKeyParameter.Exponent.ToByteArray());
                WriteBytes(memory, publicKeyParameter.Modulus.ToByteArray());

                var publicKeyBase64 = Convert.ToBase64String(memory.ToArray());

                keyContext.PublicKey = $"ssh-rsa {publicKeyBase64} generated-key";
            }

            _secretTracker.AddSecret(keyContext.PrivateKey);

            return keyContext;
        }

        private void WriteBytes(MemoryStream memory, byte[] bytes)
        {
            var length = BitConverter.GetBytes(bytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(length);
            }

            memory.Write(length, 0, length.Length);
            memory.Write(bytes, 0, bytes.Length);
        }

        public class KeyContext
        {
            public string PublicKey { get; set; }

            public string PrivateKey { get; set; }
        }
    }
}
