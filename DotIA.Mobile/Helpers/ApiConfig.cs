using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotIA_Mobile.Helpers
{
    public static class ApiConfig
    {
        // ✅ URL DA SUA API (do arquivo .http)
        public static string BaseUrl = "http://localhost:5029";
        public static TimeSpan Timeout = TimeSpan.FromSeconds(30);

        // Endpoints da sua API
        public static class Endpoints
        {
            public static string Login = "auth/login";
            public static string Registrar = "usuarios/cadastrar";
            public static string Chat = "chat/mensagens";
        }
    }
}
