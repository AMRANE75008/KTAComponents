using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTAComponents
{
    public class BusinessRules
    {


        // vérifie si le chèque doit être rejeté
        // chequeDate : date du chèque
        // montantCheque : montant du chèque
        // signaturePresente : présence signature
        // beneficiaire : bénéficiaire du chèque
        // montantLettresPresent : présence du montant en lettres
        // seuilRejetMontant : si montant supérieur à ce seuil, le chèque est rejeté
        public bool ChequeIsRejected(DateTime chequeDate, decimal montantCheque, bool signaturePresente, string beneficiaire, bool montantLettresPresent, decimal seuilRejetMontant, bool enRejet)
        {
            try
            {

                DateTime now = DateTime.Now;
                int res = DateTime.Compare(now, chequeDate);

                // si chèque post daté
                if (res < 0)
                {
                    return true;
                }
                // si montant > au seuil et pas de signature
                else if (montantCheque > seuilRejetMontant && !signaturePresente)
                {
                    return true;
                }
                // si bénéficiaire absent
                else if (string.IsNullOrEmpty(beneficiaire))
                {

                    return true;
                }
                // somme en lettres non présente
                else if (!montantLettresPresent)
                {

                    return true;
                }

                return enRejet;
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur BusinessRules.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        // indique si un chèque est post daté de moins de X jours
        // chequeDate : date du chèque
        // thresholdInDays : nombre de jours pris en compte
        public bool ChequeIsPostDated(DateTime chequeDate, int thresholdInDays)
        {

            try
            {

                DateTime now = DateTime.Now;
                int res = DateTime.Compare(now, chequeDate);

                // si chèque non post daté
                if (res >= 0)
                {
                    return false;
                }
                else
                {
                    TimeSpan ts = chequeDate - now;
                    int diffInDays = ts.Days;

                    // post daté inférieur à X jours
                    if (diffInDays < thresholdInDays)
                    {

                        return true;
                    }
                    else
                    {

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur BusinessRules.cs : " + e.Message + e.StackTrace);
                throw e;
            }

        }
    }
}



