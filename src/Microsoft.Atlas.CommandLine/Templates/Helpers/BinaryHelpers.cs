// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class BinaryHelpers : TemplateHelperProvider
    {
        private readonly ISecretTracker _secretTracker;

        public BinaryHelpers(ISecretTracker secretTracker)
        {
            _secretTracker = secretTracker;
        }

        [Description("binary")]
        public void SshKeyGen(TextWriter output, dynamic context, params object[] arguments)
        {
            var options = arguments.Last() as IDictionary<string, object>;
            if (options != null)
            {
                if (options.TryGetValue("file", out var file))
                {
                    // TODO: file reading service, and restricted source locations
                    var bytes = File.ReadAllBytes(file.ToString());
                    output.Write($"!!binary {Convert.ToBase64String(bytes)}");
                    return;
                }
            }

            throw new Exception("'binary' helper requires a file='path' option");
        }
    }
}
