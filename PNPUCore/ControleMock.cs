using PNPUCore.Controle;
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

        public new bool MakeControl()
        {


            
            Random rnd = new Random();
            int random = rnd.Next(0, 5000);
            System.Threading.Thread.Sleep(random);
            random = rnd.Next(1, 101);
            if (random < 95)
                return true;
            else
            {
                this.GetProcessControle().AjouteRapport("Erreur sur le controle MOCK");
                return false;
            }
        }
    }
}
