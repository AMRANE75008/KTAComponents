using System;
using System.IO;
using Agility.Sdk.Services;
using Agility.Sdk.Model.CaseProcesses;
using Agility.Sdk.Model.Jobs;
using Agility.Sdk.Model.Variables;


namespace KTAComponents
{
    
    

    // surcouche à la gestion des cases du sdk KTA
    public class CustoCaseService
    {
        
        // mise en attente d'un case
        // sessionId : id de session
        // caseId : identifiant du case actif dans KTA
        // caseReference : référence du case dans KTA
        // activationDate : date à laquelle mettre fin à l'attente
        // reasonForHold : motif de la mise en attente
        public void PutCaseOnHold(string sessionId, string caseId, string caseReference, DateTime activationDate, string reasonForHold)
        {

            try
            {
                // préparation paramètres
                CaseService cService = new CaseService();
                CaseIdentity caseIdentity = new CaseIdentity();
                caseIdentity.CaseId = caseId;
                caseIdentity.CaseReference = caseReference;
                OnHoldOptions holdOptions = new OnHoldOptions();
                holdOptions.ActivationDate = activationDate; // date de réactivation
                holdOptions.OnHoldType = 0; // type de réactivation : à une date donnée
                holdOptions.ReasonForHold = reasonForHold; // motif

                // mise en attente du case, si la date n'est pas plus d'une heure dans le futur la mise en attente ne peut pas se faire et une exception est renvoyée
                cService.PutCaseOnHold(sessionId, caseIdentity, holdOptions);
            }
            catch(System.ServiceModel.FaultException e)
            {
                // si exception car date n'est pas de plus d'une heure dans le futur, on ignore la mise en attente
                if(!e.ToString().Contains("Activation date should be at least an hour greater than the current date.")) {

                    LoggerConfiguration.GetLogger().TraceError("Erreur CaseService.cs : " + e.Message + e.StackTrace);
                    throw e; // renvoi de l'exception si autre cas d'erreur
                }
            }
        }

        // mise en attente d'un case et réinitialisation de la date de mise en attente avec la valeur donnée
        // sessionId : id de session
        // jobId : id du job actif dans KTE
        // caseId : identifiant du case actif dans KTA
        // caseReference : référence du case dans KTA
        // variableNameActivationDate : nom de la variable correspondant à la date d'activation
        // newDateValue : nouvelle valeur à affecter à la date
        // reasonForHold : motif de la mise en attente
        public void PutCaseOnHoldOnce(string sessionId, JobIdentity jobIdentity, string caseId, string caseReference, string variableNameActivationDate, DateTime activationDate, DateTime newDateValue, string reasonForHold)
        {

            try
            {
                // préparation paramètres d'entrée
                CaseService cService = new CaseService();
                CaseIdentity caseIdentity = new CaseIdentity();
                caseIdentity.CaseId = caseId;
                caseIdentity.CaseReference = caseReference;
                OnHoldOptions holdOptions = new OnHoldOptions();
                holdOptions.ActivationDate = activationDate; // date de réveil
                holdOptions.OnHoldType = 0; // type de réactivaition : échéance date de réveil
                holdOptions.ReasonForHold = reasonForHold; // motif

               // mise en attente du case ; si la date n'est pas plus d'une heure dans le futur la mise en attente ne peut pas se faire et une exception est renvoyée
               cService.PutCaseOnHold(sessionId, caseIdentity, holdOptions);

                // mise à jour de la date de réveil
               JobService jService = new JobService();
               UpdatedVariableCollection collection = new UpdatedVariableCollection();
               UpdatedVariable uVar = new UpdatedVariable();
               uVar.Id = variableNameActivationDate;
               uVar.Value = newDateValue;
               collection.Add(uVar);
               jService.UpdateJobVariables(sessionId, jobIdentity, collection);

            }
            catch (System.ServiceModel.FaultException e)
            {
                // si exception car date n'est pas de plus d'une heure dans le futur, on ignore la mise en attente
                if (!e.ToString().Contains("Activation date should be at least an hour greater than the current date."))
                {
                    LoggerConfiguration.GetLogger().TraceError("Erreur CaseService.cs : " + e.Message + e.StackTrace);
                    throw e; // renvoi de l'exception si autre cas d'erreur
                }
            }
        }


        // réveil d'un case
        // sessionId : id de session
        // caseId : identifiant du case actif dans KTA
        // caseReference : référence du case dans KTA
        public void UnholdCase(string sessionId, string caseId, string caseReference) 
        {
            try
            {

            CaseService cService = new CaseService();
            CaseIdentity caseIdentity = new CaseIdentity();
            caseIdentity.CaseId = caseId;
            caseIdentity.CaseReference = caseReference;
            cService.TakeCaseOffHold(sessionId, caseIdentity, false); // réveil
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CaseService.cs : " + e.Message +  e.StackTrace);
                throw e;
            }
        }
    }
}
