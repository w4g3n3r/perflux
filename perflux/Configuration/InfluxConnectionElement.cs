using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class InfluxConnectionElement : ConfigurationElement
    {
        [ConfigurationProperty("hostName", IsRequired=true, IsKey=false)]
        public string HostName { get { return (string)this["hostName"]; } }

        [ConfigurationProperty("databaseName", IsRequired = true, IsKey = true)]
        public string DatabaseName { get { return (string)this["databaseName"]; } }

        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 8086)]
        public int Port { get { return (int)this["port"]; } }

        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName { get { return (string)this["userName"]; } }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password { get { return (string)this["password"]; } }

        public Uri GetConnectionUri()
        {
            string url = string.Format("http://{0}:{1}/write?db={2}&u={3}&p={4}",
                HostName,
                Port,
                DatabaseName,
                UserName,
                Password);

            Uri connectionUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out connectionUri))
                throw new ConfigurationErrorsException("Connection information could not be translated into a valid URI.");
            
            return connectionUri;
        }
    }
}
