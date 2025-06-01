using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem_Main
{
    public static class WebServiceClient
    {
        private static readonly Lazy<LibraryWebServiceRef.WebService> _instance =
            new Lazy<LibraryWebServiceRef.WebService>(() =>
            {
                var client = new LibraryWebServiceRef.WebService();
            return client;
            });

        public static LibraryWebServiceRef.WebService Instance => _instance.Value;
    }
}