using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Agility.Sdk.Services;


namespace KTAComponents
{
    // génération de bordereaux
    public class BordereauSuppressionGenerator
    {

        /**
         *Génération bordereau quotidien pour les chèques 
         *@param connectionString connexion BDD
         *@param path chemin de génération du bordereau
         *@param fileName nom de fichier du bordereau
         *@param extension extension fichier de bordereau
         *@param ids liste des batch_id des paiements à intégrer dans le bordereau (renséignés entre cotes et séparés par des ','  exemple '1', '2', '7')
         */
        public void GenerateBordereauSuppression(string connectionString, string path, string fileName, string extension, string ids)
        {
       
            StreamWriter writer = null;

            try
            {

                writer = File.CreateText(path + fileName + extension);
                writer.Write("<html><head></head><body>");

                writer.Write("****** BORDEREAU DE SUPPRESSION ******<br/><br/>");
                writer.Write("Date de g&eacute;n&eacute;ration : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                writer.Write("<br /><br /><br />");

                writer.Write("<table border='1px solid black'>");
                writer.Write("<tr>");
                writer.Write("<td>Num&eacute;ro lot origine</td>");
                writer.Write("<td>Num&eacute;ro lot</td>");
                writer.Write("<td>Type de paiement</td>");
                writer.Write("<td>Commentaire</td>");
                writer.Write("<td>Num&eacute;ro de contrat</td>");
                writer.Write("<td>Num&eacute;ro du paiement</td>");
                writer.Write("<td>Montant</td>");
                writer.Write("<td>Motif rejet</td>");
                writer.Write("</tr>");

                List<Paiement> paiements = GetPaiements(connectionString, ids);
                foreach (Paiement paiement in paiements)
                {
                    
                    writer.Write("<tr>");
                    writer.Write("<td>"+paiement.lotOrigine+"</td>");
                    writer.Write("<td>"+paiement.lot+"</td>");
                    writer.Write("<td>"+paiement.type+"</td>");
                    writer.Write("<td>"+paiement.commentaire_general+"</td>");
                    writer.Write("<td>"+paiement.contrat+"</td>");
                    writer.Write("<td>"+paiement.numero+"</td>");
                    writer.Write("<td>"+paiement.montant+"</td>");
                    writer.Write("<td>"+paiement.motifRejet+"</td>");
                    writer.Write("</tr>");
                }
                writer.Write("</table>");

                writer.Write("</body></html>");
                writer.Flush();
                writer.Close();

            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur BordereauSuppressionGenerator.cs : " + e.Message +  e.StackTrace);
                throw e;
            }
            finally
            {
                try
                {
                    writer.Dispose();
                }
                catch (Exception) { }
            }
        }


       
        // récupération des remises d'un société du bordereau
        private List<Paiement> GetPaiements(string connectionString, string ids)
        {
            List<Paiement> paiements = new List<Paiement>();

            SqlConnection connection = new SqlConnection(connectionString);

            String sql_request = "SELECT batch_id, num_lot, num_lot_origine, type_paiement, gestionnaire_de_destination, ";
            sql_request += "numero_contrat, numero_paiement, montant, commentaire_general FROM vBordereauSuppression WHERE kta_job_id in ";

            //correction pour valeurs multiples
            string[] ids_token = ids.Split(',');
            if (ids_token.Length == 0) {
                sql_request += "('" + ids + "')";
            }
            else {
                sql_request += "(";
                foreach (string id_token in ids_token) {
                    sql_request += "'" + id_token + "',";
                }
                sql_request = sql_request.Substring(0, sql_request.Length - 1);
                sql_request += ")";
            }

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                //cmd.Parameters.AddWithValue("@ids", ids);
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Paiement paiement = new Paiement();
                            string batchId = Convert.ToString(reader["batch_id"]);
                            paiement.contrat = Convert.ToString(reader["numero_contrat"]);
                            paiement.gestionnaire = Convert.ToString(reader["gestionnaire_de_destination"]);
                            paiement.lot = Convert.ToString(reader["num_lot"]); 
                            paiement.lotOrigine = Convert.ToString(reader["num_lot_origine"]); 
                            paiement.montant = Convert.ToDouble(reader["montant"]);
                            paiement.numero = Convert.ToString(reader["numero_paiement"]); 
                            paiement.type = Convert.ToString(reader["type_paiement"]);
                            paiement.commentaire_general = Convert.ToString(reader["commentaire_general"]);
                            paiement.motifRejet = GetMotifRejet(connectionString, batchId);
                            
                            paiements.Add(paiement);

                        }
                    }
                    reader.Close();
                    reader.Dispose();
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
                throw ex;
            }

            return paiements;
        }

        public void EnleverPaiementsListeSuppression(string connectionString, string ids) {

            //parse des valeurs
            String sql_in_value = String.Empty;

            string[] ids_token = ids.Split(',');
            if (ids_token.Length == 0) {
                sql_in_value += "('" + ids + "')";
            }
            else {
                sql_in_value += "(";
                foreach (string id_token in ids_token) {
                    sql_in_value += "'" + id_token + "',";
                }
                sql_in_value = sql_in_value.Substring(0, sql_in_value.Length - 1);
                sql_in_value += ")";
            }
            enleverPaiementsSuppression(connectionString, "CHEQUE", "kta_job_id IN " + sql_in_value);
            enleverPaiementsSuppression(connectionString, "MANDAT_CASH", "kta_job_id IN " + sql_in_value);
            enleverPaiementsSuppression(connectionString, "EFFET", "kta_job_id IN " + sql_in_value);

        }

        private void enleverPaiementsSuppression(string connectionString, string table, string sql_in_values) {
            SqlConnection connection = new SqlConnection(connectionString);

            //concaténation
            String sql_request = "UPDATE "+table+" SET supprime=0 WHERE "+sql_in_values;


            try {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();

                connection.Close();
                connection.Dispose();
            }
            catch (Exception ex) {
                if (connection != null) {
                    connection.Dispose();
                }
                throw ex;
            }
        }

        private string GetMotifRejet(string connectionString, string batchId)
        {
            string motif = "";

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT motif_rejet.libelle as libelle FROM motif_rejet inner join paiement_rejet_link on motif_rejet.id_rejet = paiement_rejet_link.id_rejet";
            sql_request += " WHERE paiement_rejet_link.kc_batchid = '" + batchId + "'";

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            motif = motif + " " + Convert.ToString(reader["libelle"]);
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
                throw ex;
            }


            motif = motif.Replace("à", "&agrave;");
            motif = motif.Replace("é", "&eacute;");
            motif = motif.Replace("è", "&egrave;");
            return motif;
        }



        private struct Paiement
        {
            public double montant;
            public string lot;
            public string lotOrigine;
            public string type;
            public string gestionnaire;
            public string contrat;
            public string numero;
            public string motifRejet;
            public string commentaire_general;
        }
    }

}

