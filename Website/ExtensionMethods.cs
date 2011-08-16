﻿using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.Web.Routing;
using System.Web.WebPages;

namespace NuGetGallery {
    public static class ExtensionMethods {
        public static void MapServiceRoute(
            this RouteCollection routes,
            string routeName,
            string routeUrl,
            Type serviceType) {
            var serviceRoute = new ServiceRoute(routeUrl, new DataServiceHostFactory(), serviceType);
            serviceRoute.Defaults = new RouteValueDictionary { { "serviceType", "odata" } };
            serviceRoute.Constraints = new RouteValueDictionary { { "serviceType", "odata" } };
            routes.Add(routeName, serviceRoute);
        }

        public static string ToStringSafe(this object obj) {
            if (obj != null) {
                return obj.ToString();
            }
            return String.Empty;
        }

        public static string Flatten(this ICollection<PackageAuthor> authors) {
            return string.Join(",", authors.Select(a => a.Name).ToArray());
        }

        public static string Flatten(this ICollection<PackageDependency> dependencies) {
            return FlattenDependencies(dependencies.Select(d => new Tuple<string, string>(d.Id, d.VersionRange.ToStringSafe())));
        }

        public static string Flatten(this IEnumerable<NuGet.PackageDependency> dependencies) {
            return FlattenDependencies(dependencies.Select(d => new Tuple<string, string>(d.Id, d.VersionSpec.ToStringSafe())));
        }

        static string FlattenDependencies(IEnumerable<Tuple<string, string>> dependencies) {
            return string.Join("|", dependencies.Select(d => string.Format("{0}:{1}", d.Item1, d.Item2)).ToArray());
        }

        public static HelperResult Flatten<T>(this IEnumerable<T> items, Func<T, HelperResult> template) {
            if (items == null) {
                return null;
            }
            var formattedItems = items.Select(item => template(item).ToHtmlString());

            return new HelperResult(writer => {
                writer.Write(string.Join(", ", formattedItems.ToArray()));
            });
        }

        public static bool AnySafe<T>(this IEnumerable<T> items) {
            if (items == null) {
                return false;
            }
            return items.Any();
        }

        public static bool IsOwner(this Package package, IPrincipal user) {
            return package.PackageRegistration.IsOwner(user);
        }

        public static bool IsOwner(this PackageRegistration package, IPrincipal user) {
            if (package == null) {
                throw new ArgumentNullException("package");
            }
            if (user == null || user.Identity == null) {
                return false;
            }
            return package.Owners.Any(u => u.Username == user.Identity.Name);
        }
    }
}