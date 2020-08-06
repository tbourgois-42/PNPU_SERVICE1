using PNPUTools;
using System;

namespace PNPUCore.Controle
{
    class ControleMock : PControle, IControle
    {

        public ControleMock()
        {
            ToolTipControle = "Contrôle utilisé pour les tests";
            LibControle = "ContrôleMock";
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>

        public new string MakeControl()
        {
            Random rnd = new Random();
            int random = rnd.Next(0, 5000);
            System.Threading.Thread.Sleep(random);
            random = rnd.Next(1, 100);
            if (random < 99)
                return ParamAppli.StatutOk;
            else if (random >= 99 && random < 99.5)
            {
                GetProcessControle().AjouteRapport("Warning sur le controle MOCK");
                return ParamAppli.StatutWarning;
            }
            else
            {
                GetProcessControle().AjouteRapport("Erreur sur le controle MOCK");
                return ParamAppli.StatutError;
            }
        }
    }
}
