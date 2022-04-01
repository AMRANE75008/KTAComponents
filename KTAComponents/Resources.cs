using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TotalAgility.Sdk;
using Agility.Sdk.Model.Resources;
using System.IO;
using System.Data.SqlClient;
using System.Data;



namespace KTAComponents
{
    public class Resources
    {

        // met à jour la table des utilisateurs depuis les ressources KTA membres du groupe indiqué
        // sessionId : id de session de connexion
        // resourceType : type de ressource KTA
        // groupName : seuls les membres de ce groupe seront récupérés
        // connectionString : infos connexion base de données
        public void UpdateDatabaseUtilisateursFromKTAResources(String sessionId, Int32 resourceType, string groupName, string connectionString, bool superviseurs)
        {

            SqlConnection conn = null;

            try
            {

                // récupération des ressources
                ResourceService service = new ResourceService();

                ResourceIdentity groupId = new ResourceIdentity();
                groupId.Name = groupName;

                ResourceSummaryCollection coll = service.GetMembersOfGroup(sessionId, groupId, false);
                
                // connexion à la base de données
                conn = new SqlConnection(connectionString);
                conn.Open();


                List<string> list = new List<string>();
                if (coll != null)
                {
                    // parcours des ressources récupérées
                    foreach (ResourceSummary rSumm in coll)
                    {

                        // filtre sur le type
                        if (resourceType == rSumm.Identity.ResourceType)
                        {
                            // appel procédure stockée addOrUpdateUser
                            SqlCommand command = new SqlCommand("addOrUpdateUser", conn);
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            // préparation des paramètres de la procédure stockée
                            SqlParameter paramId = new SqlParameter("@userId", SqlDbType.VarChar, 50);
                            paramId.Value = rSumm.Identity.Id;
                            command.Parameters.Add(paramId);

                            SqlParameter paramDisplayName = new SqlParameter("@userDisplayName", SqlDbType.VarChar, 50);
                            paramDisplayName.Value = rSumm.Identity.Name;
                            command.Parameters.Add(paramDisplayName);

                            SqlParameter paramLogin = new SqlParameter("@userLogin", SqlDbType.VarChar, 50);
                            paramLogin.Value = service.GetWorkerResource(sessionId, rSumm.Identity).NTName;
                            command.Parameters.Add(paramLogin);

                            SqlParameter paramEMail = new SqlParameter("@email", SqlDbType.VarChar, 50);
                            paramEMail.Value = service.GetWorkerResource(sessionId, rSumm.Identity).EmailAddress;
                            command.Parameters.Add(paramEMail);


                            SqlParameter paramSuperviseur = new SqlParameter("@superviseur", SqlDbType.Int, 1);
                            if (superviseurs)
                            {
                                paramSuperviseur.Value = 1;
                            }
                            else
                            {
                                paramSuperviseur.Value = 0;
                            }
                            command.Parameters.Add(paramSuperviseur);

                            // appel procédure pour une ressource
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur Resources.cs : " + e.Message + e.StackTrace);
                throw e;
            }
            finally
            {
                try
                {
                    conn.Dispose();
                    conn.Close();
                }
                catch (Exception)
                {
                }
            }
        }








        // met à jour la table des gestionnaire depuis les ressources KTA membres du groupe indiqué
        // sessionId : id de session de connexion
        // resourceType : type de ressource KTA
        // groupName : seuls les membres de ce groupe seront récupérés
        // connectionString : infos connexion base de données
        // superviseur : indique si on charge les utilisateurs du groupe superviseurs
        public void UpdateDatabaseGestionnairesFromKTAResources(String sessionId, Int32 resourceType, string groupName, string connectionString)
        {

            SqlConnection conn = null;

            try
            {

                // récupération des ressources
                ResourceService service = new ResourceService();

                ResourceIdentity groupId = new ResourceIdentity();
                groupId.Name = groupName;

                ResourceSummaryCollection coll = service.GetMembersOfGroup(sessionId, groupId, false);

                // connexion à la base de données
                conn = new SqlConnection(connectionString);
                conn.Open();

                List<string> list = new List<string>();

                // parcours des ressources récupérées
                foreach (ResourceSummary rSumm in coll)
                {

                    // filtre sur le type
                    if (resourceType == rSumm.Identity.ResourceType)
                    {

                        // appel procédure stockée addOrUpdateUser
                        SqlCommand command = new SqlCommand("addOrUpdateGestionnaire", conn);
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // préparation des paramètres de la procédure stockée
                        SqlParameter paramId = new SqlParameter("@userId", SqlDbType.VarChar, 50);
                        paramId.Value = rSumm.Identity.Id;
                        command.Parameters.Add(paramId);

                        SqlParameter paramDisplayName = new SqlParameter("@userDisplayName", SqlDbType.VarChar, 50);
                        paramDisplayName.Value = rSumm.Identity.Name;
                        command.Parameters.Add(paramDisplayName);

                        SqlParameter paramEMail = new SqlParameter("@email", SqlDbType.VarChar, 50);
                        paramEMail.Value = service.GetWorkerResource(sessionId, rSumm.Identity).EmailAddress;
                        command.Parameters.Add(paramEMail);


                        // appel procédure pour une ressource
                        command.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur Resources.cs : " + e.Message + e.StackTrace);
                throw e;
            }
            finally
            {
                try
                {
                    conn.Dispose();
                    conn.Close();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

