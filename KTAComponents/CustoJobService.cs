using System;
using System.IO;
using System.Threading;
using Agility.Sdk.Services;
using Agility.Sdk.Model.CaseProcesses;
using Agility.Sdk.Model.Jobs;
using Agility.Sdk.Model.Variables;
using Agility.Sdk.Model.Activities;
using Agility.Sdk.Model.States;
using Agility.Sdk.Model.WorkQueueDefinitions;
using Agility.Sdk.Model.Processes;

namespace KTAComponents
{
    // surcouche à la gestion des jobs du sdk KTA
    public class CustoJobService
    {



        // mise à jour d'une variable KTA
        // sessionId : id de session 
        // jobIdentity : id du job actif dans KTA
        // variableName : nom de la variable à modifier
        // valeur à affecter à la variable
        public void UpdateVariableValue(string sessionId, JobIdentity jobIdentity, string variableName, object value)
        {
            try
            {

                JobService jService = new JobService();

                // ajout de la variable et de sa nouvelle valeur dans une collection
                UpdatedVariableCollection collection = new UpdatedVariableCollection();
                UpdatedVariable uVar = new UpdatedVariable();
                uVar.Id = variableName;
                uVar.Value = value;
                collection.Add(uVar);

                // mise à jour de la variable
                jService.UpdateJobVariables(sessionId, jobIdentity, collection);

            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }



        // relance un job à une étape donnée
        // sessionId : id de session
        // nodeId : activité à laquelle relancer le job
        // embeddedProcessCount : nombre de processus embarqués
        // jobId : id du job
        // subJobId : id du sous-job
     /*   public void RestartJobAt(string sessionId, Int16 nodeId, Int16 embeddedProcessCount, string jobId, string subJobId)
        {

            try
            {

                JobService jService = new JobService();
                JobActivityIdentity jobActivityIdentity = new JobActivityIdentity();
                //jobActivityIdentity.ActivityName
                jobActivityIdentity.EmbeddedProcessCount = embeddedProcessCount;
                jobActivityIdentity.JobId = jobId;
                jobActivityIdentity.NodeId = nodeId;

                JobIdentity subJobIdentity = new JobIdentity();
                subJobIdentity.Id = subJobId;

                ProcessActivityIdentityCollection embeddedProcessList = new ProcessActivityIdentityCollection();
                short restartType = 0; // redémarrrer à une activité (1 pour redémarrer après l'activité)

                jService.RestartJob(sessionId, jobActivityIdentity, embeddedProcessList, restartType, subJobIdentity);
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        } */


        // relance un job à une étape donnée seulement une fois, s'il y a déjà eu relance on ne fait rien
        // la vérification se fait sur l'historique des états du job
        // sessionId : id de session
        // nodeId : activité à laquelle relancer le job
        // embeddedProcessCount : nombre de processus embarqués
        // jobId : id du job
        // subJobId : id du sous-job
        // delayInMinutes : délai d'attente en minutes avant de rejouer l'étape
        // return vrai si on a rejoue le processus, faux sinon
        public bool RestartJobOnlyOnceAt(string sessionId, Int16 nodeId, Int16 embeddedProcessCount, string jobId, string subJobId, Int16 delayInMinutes)
        {
            try
            {


                bool restart = true;

                // récupération de l'historique des statuts
                JobService jService = new JobService();
                JobIdentity jobIdentity = new JobIdentity();
                jobIdentity.Id = jobId;
                StateHistoryCollection collect = jService.GetStateChangeHistory(sessionId, jobIdentity);

                // récupération des deux derniers status s'ils existent
                int length = collect.Count;
                if (length >= 2)
                {
                    StateHistory[] states = collect.ToArray();
                    StateHistory state1 = states[length - 1];
                    StateHistory state2 = states[length - 2];

                    if (state1 != null && state2 != null)
                    {

                        if (state1.StateIdentity != null && state2.StateIdentity != null)
                        {

                            if (state1.StateIdentity.Name != null && state2.StateIdentity.Name != null)
                            {
                                // comparaison des deux statuts
                                if (state1.StateIdentity.Name.Equals(state2.StateIdentity.Name))
                                {
                                    // s'ils sont égaux on a déjà rejoué cette étape, on ne la rejoue pas
                                    restart = false;
                                }
                            }
                        }


                    }
                }


                // si on n'a pas encore rejoué l'étape
                if (restart)
                {
                    // s'il y a un délai d'attente
                    if (delayInMinutes > 0)
                    {
                        // on attend avant de rejouer
                        int time = delayInMinutes * 1000 * 60;
                        Thread.Sleep(time);
                    }

                    // on rejoue l'étape
                    CustoJobService cJobService = new CustoJobService();
                    // on renvoie le résultat du redémarrage, si ko on considère que le processus a déjà été rejoué afin dans l'envoyer dans la corbeille des exceptions
                    restart = cJobService.RestartSuspendedJobAt(sessionId, nodeId, embeddedProcessCount, jobId, subJobId, 0);

                }

                return restart;
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        // relance un job à une étape donnée s'il est suspendu
        // sessionId : id de session
        // nodeId : activité à laquelle relancer le job
        // embeddedProcessCount : nombre de processus embarqués
        // jobId : id du job
        // subJobId : id du sous-job
        // restartType : 0 pour redémarrer à cette étape, 1 pour redémarrer à la suivante
        // return trie si succès, false sinon
        private bool RestartSuspendedJobAt(string sessionId, Int16 nodeId, Int16 embeddedProcessCount, string jobId, string subJobId, short restartType)
        {

            try
            {
                JobService jService = new JobService();
                JobActivityIdentity jobActivityIdentity = new JobActivityIdentity();

                //jobActivityIdentity.ActivityName
                jobActivityIdentity.EmbeddedProcessCount = embeddedProcessCount;
                jobActivityIdentity.JobId = jobId;
                jobActivityIdentity.NodeId = nodeId;

                JobIdentity subJobIdentity = new JobIdentity();
                subJobIdentity.Id = subJobId;

                // si le job à redémarrer est suspendu (en erreur)
                JobIdentity jobIdentity = new JobIdentity();
                jobIdentity.Id = jobId;
                JobInfo info = jService.GetJobInfo(sessionId, jobIdentity, new JobHistoryFilter());
                if (info.Status.Value == 3)
                {
                    // on le redémarre
                    ProcessActivityIdentityCollection embeddedProcessList = new ProcessActivityIdentityCollection();                 
                    jService.RestartJob(sessionId, jobActivityIdentity, embeddedProcessList, restartType, subJobIdentity);
                }

                return true;
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : " + e.Message + e.StackTrace);
                return false;
            }
        }



        // relance un job à une étape donnée s'il est suspendu
        // sessionId : id de session
        // nodeId : activité à laquelle relancer le job
        // embeddedProcessCount : nombre de processus embarqués
        // jobId : id du job
        // subJobId : id du sous-job
        public void RestartSuspendedJobAt(string sessionId, Int16 nodeId, Int16 embeddedProcessCount, string jobId, string subJobId)
        {

            
            try
            {
               bool restartOK =  RestartSuspendedJobAt(sessionId, nodeId, embeddedProcessCount, jobId, subJobId,0);
               if (!restartOK)
               {

                   JobService jService = new JobService();
                   JobIdentity identity = new JobIdentity();
                   identity.Id = jobId;
                   JobHistoryCollection histo = jService.GetJobHistory(sessionId, identity, true);

                   for (int i = 0; i < histo.Count; i++)
                   {
                       if (i == 0 || i==1)
                       {
                           continue;
                       }
                       restartOK = RestartSuspendedJobAt(sessionId, histo[i].Node.NodeId, embeddedProcessCount, jobId, subJobId, 1);
                       if(restartOK)
                       {
                            break;
                       }
                   }

                   if (!restartOK)
                   {
                       throw new Exception("Impossible de redémarrer le processus en erreur");
                   }
               }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : " + e.Message + e.StackTrace);
                throw e;
            }


        }

        /**
         * Fonction permettant de récupérer le dernier état du job (l'état en cours).
         * 
         **/
        public string getLastState(string sessionId, string jobId) {
            JobIdentity jobIdentity = new JobIdentity();
            jobIdentity.Id = jobId;
            JobService jobService = new JobService();
            StateHistoryCollection collect = jobService.GetStateChangeHistory(sessionId, jobIdentity);
            
            int length = collect.Count;
            if (collect.Count >= 1) {
                StateHistory[] states = collect.ToArray();
                StateHistory state = states[length - 1];

                return state.StateIdentity.Name;
            }

            LoggerConfiguration.GetLogger().TraceError("Erreur CustoJobService.cs : Tentative de récupération de l'état d'un job (résultat vide). JobId: '"+jobId+"'");

            throw new Exception("Aucun processus ou état n'a été trouvé pour le jobId:'" + jobId + "'");
        }
    }
}
