using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTInstaller.Services
{
    internal static class ServiceFactory
    {
        private static object _codeService = null;

        public static T GetService<T>()
        {
            if (typeof(T) == typeof(ICodeService))
            {
                if (_codeService == null)
                    _codeService = new CodeService();

                return (T)_codeService;
            }

            return default;
        }
    }
}
