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
            int random = rnd.Next(5000, 15000);
            System.Threading.Thread.Sleep(random);
            random = rnd.Next(1, 101);
            if (random < 80)
                return true;
            else
            {
                this.GetProcessControle().AjouteRapport("Flûte une erreur...");
                return false;
            }
        }
    }
}
