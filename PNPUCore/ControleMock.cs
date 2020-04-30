using PNPUCore.Controle;
using PNPUTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUCore.Controle
{
    class ControleMock : PControle, IControle
    {

        public ControleMock()
        {
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
            random = rnd.Next(1, 101);
            if (random < 98)
                return ParamAppli.StatutCompleted;
            else if (random >=98 && random < 99)
            {
                this.GetProcessControle().AjouteRapport("Warning sur le controle MOCK");
                return ParamAppli.StatutWarning;
            }
            else{
                this.GetProcessControle().AjouteRapport("Erreur sur le controle MOCK");
                return ParamAppli.StatutError;
            }
        }
    }
}
