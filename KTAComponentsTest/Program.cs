using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KTAComponents;





namespace KTAComponentsTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            string errMessage = "";


            Utils utl = new Utils();
            bool isnumber = utl.isNumber("", out errMessage);

            string EnvironnementName = utl.getComputerName();
            Console.WriteLine(isnumber);
            Console.WriteLine(EnvironnementName);

            Console.Read();































            /*BordereauSuppressionGenerator bGen = new BordereauSuppressionGenerator();
            bGen.GenerateBordereauSuppression("Data Source=localhost;Initial Catalog=SIRIUS_CustomDB;Integrated Security=True",
                  "D:\\Export\\SIRIUS\\", "test", ".htm", "APP_EFF77777");*/



            /*BordereauGeneralGenerator bGenGen = new BordereauGeneralGenerator();
            bGenGen.GenerateBordereauGeneralCheques("Data Source=localhost;Initial Catalog=SIRIUS_CustomDB;Integrated Security=True", "D:\\Export\\SIRIUS\\testBDRGEN.html", "", "", 3); 
            */
            
            //SIELWrapper siel = new SIELWrapper();
            //bool resultat;
            //string emetteur, commentaire, numContrat, numTiers, gestionnaire, message, numECH;
            //decimal montant;
            //siel.ReadSIEL("", "", out resultat, out commentaire, out emetteur, out montant, out numECH, out numContrat, out numTiers, out gestionnaire, out message);

            //if (resultat)
            //{
            //    Console.WriteLine("APPEL SIEL OK");
            //}
            //else
            //{
            //    Console.WriteLine("APPEL SIEL KO : "+message);
            //}

            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }



     
    }
}

