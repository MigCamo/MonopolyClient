using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using UIGameClientTourist.Properties;

namespace UIGameClientTourist.GameLogic
{
    public class ResourceHandler
    {
        public ResourceHandler() { }

        public string GetResourceString(string resourceName)
        {
            string resourceValue = "No paso nada";
            ResourceManager resourceManager = new ResourceManager("UIGameClientTourist.Properties.Resources", Assembly.GetExecutingAssembly());

            try
            {
                resourceValue = resourceManager.GetString(Resources.AcceptPurchase_Button);
                if (resourceValue == null)
                {
                    Console.WriteLine($"La cadena de recursos '{resourceName}' no se encontró.");
                }
            }
            catch (MissingManifestResourceException)
            {
                Console.WriteLine($"Archivo de recursos no encontrado para el espacio de nombres '{resourceName}'.");
            }
            return resourceValue;
        }

    }
}
