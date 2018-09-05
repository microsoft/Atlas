// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Secrets
{
    public class SecretTracker : ISecretTracker
    {
        private List<string> _secrets = new List<string>();
        private object _secretsLock = new object();

        public void AddSecret(string secret)
        {
            if (!string.IsNullOrEmpty(secret))
            {
                lock (_secretsLock)
                {
                    var allSecrets = new List<string>();
                    allSecrets.AddRange(_secrets);
                    allSecrets.Add(secret);
                    allSecrets.AddRange(secret.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));

                    var orderedSecrets = new List<string>();

                    MoveAllSecrets(allSecrets, orderedSecrets);

                    _secrets = orderedSecrets;
                }
            }
        }

        public string FilterString(string text)
        {
            return _secrets.Aggregate(text, (redacted, secret) => redacted.Replace(secret, "xxxxxxxx"));
        }

        public TextWriter FilterTextWriter(TextWriter writer)
        {
            return new SecretTextWriter(this, writer);
        }

        private void MoveAllSecrets(List<string> remainingSecrets, List<string> orderedSecrets)
        {
            while (remainingSecrets.Any())
            {
                var secret = remainingSecrets[0];
                remainingSecrets.RemoveAt(0);
                MoveLongerSecrets(secret, remainingSecrets, 0, orderedSecrets);
                orderedSecrets.Add(secret);
            }
        }

        private void MoveLongerSecrets(string shorterSecret, List<string> remainingSecrets, int remainingIndex, List<string> orderedSecrets)
        {
            var index = remainingIndex;
            while (index < remainingSecrets.Count)
            {
                var secret = remainingSecrets[index];
                if (secret.IndexOf(shorterSecret, StringComparison.Ordinal) < 0)
                {
                    index += 1;
                }
                else
                {
                    remainingSecrets.RemoveAt(index);
                    MoveLongerSecrets(secret, remainingSecrets, index, orderedSecrets);
                    orderedSecrets.Add(secret);
                }
            }
        }
    }
}
