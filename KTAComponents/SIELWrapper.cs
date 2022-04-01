using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KTAComponents
{
    public class SIELWrapper
    {
        public void ReadSIEL(string userAS400, string idSIRIUS, out bool resultat, out string commentaire, out string emetteur, out decimal montant, out string numECH, out string numContrat, out string numTiers, out string gestionnaire, out string message)
        {
            //siriusPortClient siel = new siriusPortClient("siriusPicoEndpointBinding", "http://localhost:8088/service/finances/i1/v1/paymentEndpoint");

            //siriusPortClient sielClient = new siriusPortClient();
            //PICOWebReference.siriusPicoserviceserviceagent sielClient = new PICOWebReference.siriusPicoserviceserviceagent();
            //SIRIUSWrapped.siriusPortClient sielClient = new SIRIUSWrapped.siriusPortClient();
            WSDLSIELProxy.siriusPicoserviceserviceagent sielClient = new WSDLSIELProxy.siriusPicoserviceserviceagent();

            resultat = false;
            commentaire = "";
            emetteur = "";
            montant = 0.0m;
            numECH = "";
            numContrat = "";
            numTiers = "";
            gestionnaire = "";
            message = "";

            try
            {
                object[] datas = null;
                //reponse1 rep = sielClient.recupAffectation(new requete1() { idSirius = idSIRIUS, profileAS400 = userAS400 });
                //sielClient.recupAffectation(idSIRIUS, userAS400, out resultat);
                //SIRIUSWrapped.reponse1 rep = sielClient.recupAffectation(new SIRIUSWrapped.requete1() { idSirius = idSIRIUS, profileAS400 = userAS400 });
                datas = sielClient.recupAffectation(idSIRIUS, userAS400, out resultat);

                //datas = sielClient.recupAffectation(idSIRIUS, userAS400, out resultat);
                //sielClient.recupAffectation()
                
                if (resultat)
                {
                    //object[] datas = rep.Items;

                    /*reponseDonneesAffectation repA = null;
                    reponseDonneesGenerales repG = null;
                    PICOWebReference.reponseDonneesAffectation repA = null;
                    PICOWebReference.reponseDonneesGenerales repG = null;*/
                    WSDLSIELProxy.reponseDonneesAffectation repA = null;
                    WSDLSIELProxy.reponseDonneesGenerales repG = null;

                    for (int i = 0; i < datas.Length; i++)
                    {
                        if (datas[i] is WSDLSIELProxy.reponseDonneesAffectation)
                        {
                            repA = (WSDLSIELProxy.reponseDonneesAffectation)datas[i];
                        }
                        else if (datas[i] is WSDLSIELProxy.reponseDonneesGenerales)
                        {
                            repG = (WSDLSIELProxy.reponseDonneesGenerales)datas[i];
                        }
                    }
                    if (repA != null && repG != null)
                    {
                        commentaire = repA.commentaire;
                        emetteur = repA.emetteur;
                        montant = repA.montant;
                        numECH = repA.idNumPieceEncaissement;
                        if (repA.contrat != null)
                        {
                            numContrat = repA.contrat.idContrat;
                            numTiers = repA.contrat.clientContrat.ToString();
                            gestionnaire = repA.contrat.libelleGestionnaire;
                        }
                    }
                    else resultat = false;

                }
            }
            catch(Exception ex)
            {
                resultat = false;
                message = ex.Message + "\n " + ex.StackTrace;
            }
        }
    }
}
