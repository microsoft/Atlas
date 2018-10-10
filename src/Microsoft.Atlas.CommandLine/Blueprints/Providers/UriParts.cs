// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class UriParts
    {
        private static readonly HostString _github = new HostString("github.com");
        private static readonly HostString _githubusercontent = new HostString("raw.githubusercontent.com");

        private static readonly PathString _tree = new PathString("/tree");
        private static readonly PathString _blob = new PathString("/blob");
        private static readonly PathString _workflowYaml = new PathString("/workflow.yaml");

        public string Scheme { get; set; }

        public HostString Host { get; set; }

        public PathString Path { get; set; }

        public QueryString Query { get; set; }

        public FragmentString Fragment { get; set; }

        public static UriParts Parse(string uri)
        {
            UriHelper.FromAbsolute(
                uri,
                out var scheme,
                out var host,
                out var path,
                out var query,
                out var fragment);

            return new UriParts
            {
                Scheme = scheme,
                Host = host,
                Path = path,
                Query = query,
                Fragment = fragment,
            };
        }

        public UriParts Append(string path)
        {
            return new UriParts
            {
                Scheme = Scheme,
                Host = Host,
                Path = Path + new PathString("/" + path),
                Query = Query,
                Fragment = Fragment,
            };
        }

        public override string ToString()
        {
            return Scheme + "://" + Host + Path + Query + Fragment;
        }

        public bool RewriteGitHubUris()
        {
            if (!Host.Equals(_github))
            {
                return false;
            }

            var segments = SplitPathSegments(Path);
            if (segments.Count < 3)
            {
                return false;
            }

            if (!segments[2].Equals(_tree) && !segments[2].Equals(_blob))
            {
                return false;
            }

            segments.RemoveAt(2);

            Host = _githubusercontent;
            Path = segments.Aggregate(default(PathString), (a, b) => a + b);
            return true;
        }

        public bool RemoveWorkflowYaml()
        {
            var result = new List<PathString>();

            var segments = SplitPathSegments(Path);
            if (segments.Count < 1)
            {
                return false;
            }

            if (!segments.Last().Equals(_workflowYaml))
            {
                return false;
            }

            segments.RemoveAt(segments.Count() - 1);
            Path = segments.Aggregate(default(PathString), (a, b) => a + b);
            return true;
        }

        public List<PathString> SplitPathSegments(PathString path)
        {
            var segments = new List<PathString>();
            var slashIndex = 0;
            while (slashIndex < path.Value.Length)
            {
                var nextIndex = path.Value.IndexOf('/', slashIndex + 1);
                if (nextIndex < 0)
                {
                    nextIndex = path.Value.Length;
                }

                segments.Add(new PathString(path.Value.Substring(slashIndex, nextIndex - slashIndex)));
                slashIndex = nextIndex;
            }

            return segments;
        }
    }
}
