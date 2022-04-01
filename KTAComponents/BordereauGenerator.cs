using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace KTAComponents
{
    // génération de bordereaux
    public class BordereauGenerator
    {

        /**
         *Génération bordereau quotidien pour les chèques 
         *@param connectionString connexion BDD
         *@param path chemin de génération du bordereau
         *@param fileName nom de fichier du bordereau
         *@param extension extension fichier de bordereau
         *@param adresseBordereau adresse figurant en bas du bordereau
         */
        public void GenerateBordereauQuotidienCheques(string connectionString, string path, string fileName, string extension, string adresseBordereau)
        {
            GenerateBordereauQuotidien(connectionString, path, fileName, extension, "vBordereauQuotidienChequesNonEtrangers", adresseBordereau, "ch&egrave;ques");
        }


        /**
         *Génération bordereau quotidien pour les effets 
         *@param connectionString connexion BDD
         *@param path chemin de génération du bordereau
         *@param fileName nom de fichier du bordereau
         *@param extension extension fichier de bordereau
         *@param adresseBordereau adresse figurant en bas du bordereau
         */
        public void GenerateBordereauQuotidienEffets(string connectionString, string path, string fileName, string extension, string adresseBordereau)
        {
            GenerateBordereauQuotidien(connectionString, path, fileName, extension, "vBordereauQuotidienEffets", adresseBordereau, "effets");
        }


        /**
         *Génération bordereau quotidien pour les mandats cash 
         *@param connectionString connexion BDD
         *@param path chemin de génération du bordereau
         *@param fileName nom de fichier du bordereau
         *@param extension extension fichier de bordereau
         *@param adresseBordereau adresse figurant en bas du bordereau
         */
        public void GenerateBordereauQuotidienMandatsCash(string connectionString, string path, string fileName, string extension, string adresseBordereau)
        {
            GenerateBordereauQuotidien(connectionString, path, fileName, extension, "vBordereauQuotidienMandatsCash", adresseBordereau, "mandats cash");
        }





        private void GenerateBordereauQuotidien(string connectionString, string path, string fileName, string extension, string view, string adresseBordereau, string typePaiement)
        {

            StreamWriter writer = null;

            try
            {

                writer = File.CreateText(path + fileName + extension);
                writer.Write("<html><head></head><body>");

                List<Societe> societes = GetSocietes(connectionString, view);
                foreach (Societe societe in societes)
                {
                    writer.Write("<table border='1px solid black'>");
                    writer.Write("<tr>");
                    writer.Write("<td colspan='3'>Soci&eacute;t&eacute; " + societe.libelle + " - " + societe.rib + "</td>");
                    writer.Write("</tr>");

                    writer.Write("<tr>");
                    writer.Write("<td></td>");
                    writer.Write("<td>Montant remise</td>");
                    writer.Write("<td>Nombre " + typePaiement + "</td>");
                    writer.Write("</tr>");

                    List<Remise> remises = GetRemisesFromCompany(connectionString, societe.id, view);
                    foreach(Remise remise in remises)
                    {
                        writer.Write("<tr>");
                        writer.Write("<td></td>");
                        writer.Write("<td>"+ remise.montantTotal +"</td>");
                        writer.Write("<td>"+ remise.nbPaiements +"</td>");
                        writer.Write("</tr>");
                    }

                    writer.Write("<tr>");
                    writer.Write("<td>Total</td>");
                    writer.Write("<td>" + societe.total + "</td>");
                    writer.Write("<td>" + societe.nbPaiements + "</td>");
                    writer.Write("</tr>");

                    writer.Write("</table>");
                    writer.Write("<br/><br/>");
                }

                Journee journee = GetTotaux(connectionString, view);

                writer.Write("<div style='page-break-before:always'>");

                writer.Write("<table border='1px solid black'>");
                writer.Write("<tr><td colspan='3'>BORDEREAU D'ENVOI DES VALEURS</td></tr>");
                writer.Write("<tr><td>Date</td><td>Exp&eacute;diteur</td><td>Destinataire</td></tr>");
                writer.Write("<tr><td>" + DateTime.Now.ToString("dd/MM/yyyy") + "</td><td>Sous-Participant : BPLG</td><td>" + adresseBordereau + "</td></tr>");
                writer.Write("<tr><td colspan='2'>Valeurs</td><td>" + journee.total + "</td></tr>");
                writer.Write("<tr><td colspan='2'>Nombre de " + typePaiement + "</td><td>" + journee.nbPaiements + "</td></tr>");
                writer.Write("<tr><td style='text-decoration:underline;vertical-align:top'>Visa BPLG</td><td><span style='text-decoration:underline;vertical-align:top'>R&eacute;ception au "
                    + adresseBordereau + "</span><br /><br />Date :<br /><br />Visa :<br /><br /></td><td style='text-decoration:underline;vertical-align:top' >Observations</td></tr>");
                writer.Write("</table>");

                writer.Write("</div>");

                writer.Write("</body></html>");
                writer.Flush();
                writer.Close();

            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur BordereauGenerator.cs : " + e.Message +  e.StackTrace);
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


        // récupération chiffres globaux du bordereau
        private Journee GetTotaux(string connectionString, string view)
        {
            Journee journee = new Journee();
            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT sum(total_paiements) as total, sum(nb_paiements) as nb FROM " + view;

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

                            journee.nbPaiements = Convert.ToInt32(reader["nb"]);
                            journee.total = Convert.ToDouble(reader["total"]); ;
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
                journee.nbPaiements = 0;
                journee.total = 0;
            }

            return journee;
        }


        // récupération des remises d'un société du bordereau
        private List<Remise> GetRemisesFromCompany(string connectionString, Int32 idSociete, string view)
        { 
            List<Remise> remises = new List<Remise>();

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT id_remise, total_paiements as total, nb_paiements as nb FROM "+ view +" WHERE id_societe = " + idSociete;

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
                            Remise remise = new Remise();
                            remise.id = Convert.ToInt32(reader["id_remise"]);
                            remise.nbPaiements = Convert.ToInt32(reader["nb"]);
                            remise.montantTotal = Convert.ToDouble(reader["total"]); ;
                            remises.Add(remise);
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

            return remises;
        }
        


        // récupération societes du bordereau
        private List<Societe> GetSocietes(string connectionString, string view)
        {

            List<Societe> societes = new List<Societe>();

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT distinct id_societe, code_organisme, libelle_societe, rib_societe, sum(total_paiements) as total, sum(nb_paiements) as nb FROM " + view;
            sql_request += " group by id_societe, code_organisme, libelle_societe, rib_societe";

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
                            Societe soc = new Societe();
                            soc.id = Convert.ToInt32(reader["id_societe"]);
                            soc.codeOrganisme = Convert.ToString(reader["code_organisme"]);
                            soc.rib = Convert.ToString(reader["rib_societe"]);
                            soc.libelle = Convert.ToString(reader["libelle_societe"]);
                            soc.nbPaiements = Convert.ToInt32(reader["nb"]);
                            soc.total = Convert.ToDouble(reader["total"]); ;
                            societes.Add(soc);
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

            return societes;
        }

        private struct Societe
        {
            public Int32 id;
            public string codeOrganisme;
            public string rib;
            public string libelle;
            public double total;
            public Int32 nbPaiements;
        }

        private struct Remise
        {
            public Int32 id;
            public double montantTotal;
            public Int32 nbPaiements;
        }

        private struct Journee
        {
            public double total;
            public Int32 nbPaiements;
        }
    }

}
