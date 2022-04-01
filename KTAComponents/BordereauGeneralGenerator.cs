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
    public class BordereauGeneralGenerator
    {

        /**
         * Génère le bordereau général d'une remise de chèques
         * @param infos connexion BDD
         * @param path chemin de génération 
         * @param fileName nom fichier bordereau
         * @param extension extension fichier
         * @parma beneficiaire bénéficiaire du bordereau
         * @idRemise id de la remise associée au bordereau
         */
        public void GenerateBordereauGeneralCheques(string connectionString, string path, string fileName, string extension, decimal idRemise)
        {

            GenerateBordereauGeneral(connectionString, path, fileName, extension, idRemise, "CHEQUE");
        }

         /**
         * Génère le bordereau général d'une remise d'effets
         * @param infos connexion BDD
         * @param path chemin de génération 
         * @param fileName nom fichier bordereau
         * @param extension extension fichier
         * @parma beneficiaire bénéficiaire du bordereau
         * @idRemise id de la remise associée au bordereau
         */
        public void GenerateBordereauGeneralEffets(string connectionString, string path, string fileName, string extension, decimal idRemise)
        {
            GenerateBordereauGeneral(connectionString, path, fileName, extension, idRemise, "EFFET");
        }

        /**
         * Génère le bordereau général d'une remise de mandats cash
         * @param infos connexion BDD
         * @param path chemin de génération 
         * @param fileName nom fichier bordereau
         * @param extension extension fichier
         * @parma beneficiaire bénéficiaire du bordereau
         * @idRemise id de la remise associée au bordereau
         */
        public void GenerateBordereauGeneralMandatsCash(string connectionString, string path, string fileName, string extension, decimal idRemise)
        {
            GenerateBordereauGeneral(connectionString, path, fileName, extension, idRemise, "MANDAT_CASH");
        }


        public void GenerateBordereauGeneral(string connectionString, string path, string fileName, string extension, decimal idRemise, string type)
        {
            int id_remise_int = Decimal.ToInt32(idRemise);

            string strBordereauType = "";
            if(String.Equals(type,"CHEQUE"))
            {
                type = "CHEQUES";
                strBordereauType = "ch&egrave;que";
            }
            else if (String.Equals(type, "EFFET"))
            {
                strBordereauType = "effet";
                type = "EFFETS";
            }
            else
            {
                type = "MANDATS-CASH";
                strBordereauType = "mandat cash";
            }


            StreamWriter writer = null;
            

            try
            {

                writer = File.CreateText(path + fileName + extension);
                writer.Write("<html><head></head><body>");

                Remise remise = GetRemise(connectionString, id_remise_int, type);

                // en-tête
                writer.Write("****** BORDEREAU GENERAL ******<br/><br/>");
                writer.Write("Type de paiement : "+ strBordereauType +"<br />");
                writer.Write("Soci&eacute;t&eacute; " + remise.libelleSociete + " - " + remise.ribSociete + "<br/>");
                writer.Write("Date de remise : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "<br />");
                writer.Write("Site virtuel : " + remise.siteVirtuel + "<br />");
                writer.Write("B&eacute;n&eacute;ficiaire : " + remise.beneficiaire + "<br />");
                writer.Write("Montant total : " + remise.montantTotal + "<br />");
                writer.Write("Nombre de paiements : " + remise.nbPaiements);
                writer.Write("<br /><br /><br />");

                // par lot
                List<Lot> lots = GetLots(connectionString, id_remise_int, type);

                foreach(Lot lot in lots)
                {
                    writer.Write("Lot num&eacute;ro " + lot.numero + "<br />");

                    writer.Write("<table border='1px solid black'>");
                    writer.Write("<tr>");
                    writer.Write("<td>Position</td>");
                    writer.Write("<td>Contrat</td>");
                    writer.Write("<td>Num&eacute;ro "+ strBordereauType +"</td>");
                    writer.Write("<td>Montant " + strBordereauType + "</td>");
                    writer.Write("<td>Gestionnaire/REJET</td>");
                    
                    if (String.Equals(type, "CHEQUES")) {
                        writer.Write("<td>Etranger</td>");
                    }

                    writer.Write("</tr>");

                    foreach (Paiement paiement in lot.paiements)
                    {
                        writer.Write("<tr>");
                        writer.Write("<td>" + paiement.position + "</td>");
                        writer.Write("<td>" + paiement.contrat + "</td>");
                        writer.Write("<td>" + paiement.numero + "</td>");
                        writer.Write("<td>" + paiement.montant + "</td>");
                        writer.Write("<td>" + paiement.gestionnaireRejet + "</td>");
                        
                        if (String.Equals(type, "CHEQUES")) {
                            writer.Write("<td>");
                            if (isForeignPayment(connectionString, paiement)) {
                                writer.Write("X");
                            }
                            writer.Write("</td>");
                        }
                        
                        writer.Write("</tr>");    
                    }

                    writer.Write("</table>");
                    writer.Write("Nb paiements lot : " + lot.nbPaiements + "<br />");
                    writer.Write("Montant total lot : " + lot.montantTotal);
                    writer.Write("<br /><br /><br />");
                }

                writer.Write("</body></html>");
                writer.Flush();
                writer.Close();

            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur BordereauGeneralGenerator.cs : " + e.Message + e.StackTrace);
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



        private Boolean isForeignPayment(string connectionString, Paiement p){
            Boolean result = false;//default value is false

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT etranger FROM CHEQUE WHERE chq_numero='" + p.numero+"' and numero_contrat='"+p.contrat+"'";

            try {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader()) {
                    if (reader.HasRows) { 
                        //only reads the first result
                        reader.Read();
                        //1 will return true, 0 will return false
                        try {
                            if (reader.GetBoolean(0)) {
                                result = true;
                            }
                            else result = false;
                        }
                        catch (System.Data.SqlTypes.SqlNullValueException) {//this will handle payment before the value has been stored in DB (null value)
                            result = false;
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch(Exception ex){
                if (connection != null) {
                    connection.Dispose();
                }
                throw ex;
            }

            // si jamais on a rien trouvé ou qu'une erreur est survenue, on renvoie la valeur par défaut (false).
            return result;

        }





        private List<Lot> GetLots(string connectionString, Int32 idRemise, string type)
        {
            List<Lot> lots = new List<Lot>();
            List<String> numsLots = GetNumsLots(connectionString, idRemise, type);

            foreach (String num in numsLots)
            {
                Lot lot = GetLot(connectionString, num, type);
                lots.Add(lot);
            }

            return lots;
        }





        private Lot GetLot(string connectionString, string num, string type)
        {

            Lot lot = new Lot();
            Paiement paiement = new Paiement();
            lot.paiements = new List<Paiement>();

            SqlConnection connection = new SqlConnection(connectionString);

            string view = "";
            string paiementNum = "";
            string paiementMontant = "";
            string idPaiement = "";

            if (String.Equals(type, "CHEQUES"))
            {
                view = "vBordereauGeneralCheques";
                paiementNum = "chq_numero";
                paiementMontant = "chq_montant";
                idPaiement = "id_cheque";
            }
            else if (String.Equals(type, "EFFETS"))
            {
                view = "vBordereauGeneralEffets";
                paiementNum = "eff_numero";
                paiementMontant = "eff_montant";
                idPaiement = "id_effet";
            }
            else
            {
                view = "vBordereauGeneralMandatsCash";
                paiementNum = "mdt_numero";
                paiementMontant = "mdt_montant";
                idPaiement = "id_mandat";
            }

            String sql_request = "SELECT " + paiementNum + ", numero_contrat, gestionnaire_ou_rejet, " + paiementMontant + " FROM " + view;
            sql_request += " where numero_lot = '" + num + "'";
            sql_request += " group by "+paiementMontant+", numero_contrat, gestionnaire_ou_rejet,"+idPaiement+", "+paiementNum;
            sql_request += " order by "+idPaiement+" asc";


            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    
                    int pos = 1;

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            paiement.contrat = Convert.ToString(reader["numero_contrat"]);
                            paiement.gestionnaireRejet = Convert.ToString(reader["gestionnaire_ou_rejet"]);
                            paiement.montant = Convert.ToDouble(reader[paiementMontant]);
                            paiement.numero = Convert.ToString(reader[paiementNum]);
                            lot.numero = num;
                            paiement.position = pos;

                            pos++;
                            lot.paiements.Add(paiement);
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }



                sql_request = "SELECT sum(" + paiementMontant + ") as total, count(kta_job_id) as nb FROM " + view;
                sql_request += " where numero_lot = '" + num + "'";
                cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            lot.nbPaiements = Convert.ToInt32(reader["nb"]);
                            lot.montantTotal = Convert.ToDouble(reader["total"]);
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

            return lot;
        }




        private List<String> GetNumsLots(string connectionString, Int32 idRemise, string type)
        {
            List<String> numsLot = new List<String>();

            string view = "";

            if (String.Equals(type, "CHEQUES"))
            {
                view = "vBordereauGeneralCheques";
            }
            else if (String.Equals(type, "EFFETS"))
            {
                view = "vBordereauGeneralEffets";
            }
            else
            {
                view = "vBordereauGeneralMandatsCash";
            }

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT distinct numero_lot FROM " + view;
            sql_request += " where id_remise = " + idRemise;

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
                            string lot = Convert.ToString(reader["numero_lot"]);
                            numsLot.Add(lot);
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

            return numsLot;
        }



        // récupération infos remise
        private Remise GetRemise(string connectionString, Int32 idRemise, string type)
        {

            Remise remise = new Remise();

            string view = "";
            string view2 = "";
            string paiementMontant = "";

            if (String.Equals(type, "CHEQUES"))
            {
                view = "vBordereauGeneralCheques";
                view2 = "vBordereauRemiseCheques";
                paiementMontant = "chq_montant";
            }
            else if (String.Equals(type, "EFFETS"))
            {
                view = "vBordereauGeneralEffets";
                view2 = "vBordereauRemiseEffets";
                paiementMontant = "eff_montant";
            }
            else
            {
                view = "vBordereauGeneralMandatsCash";
                view2 = "vBordereauRemiseMandatsCash";
                paiementMontant = "mdt_montant";
            }

            SqlConnection connection = new SqlConnection(connectionString);
            String sql_request = "SELECT beneficiaire,libelle_societe, rib_societe, site_virtuel FROM "+ view;
            sql_request += " where id_remise = " + idRemise;
            sql_request += " group by site_virtuel, libelle_societe, rib_societe, beneficiaire";

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
                        if (reader.Read())
                        {
                            remise.libelleSociete = Convert.ToString(reader["libelle_societe"]);
                            remise.ribSociete = Convert.ToString(reader["rib_societe"]);
                            remise.siteVirtuel = Convert.ToString(reader["site_virtuel"]);
                            remise.beneficiaire = Convert.ToString(reader["beneficiaire"]);
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }


                sql_request = "SELECT sum("+paiementMontant+") as total, count(kta_job_id) as nb FROM " + view2;
                sql_request += " where id_remise = " + idRemise;
                cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            remise.nbPaiements = Convert.ToInt32(reader["nb"]);
                            remise.montantTotal = Convert.ToDouble(reader["total"]);
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

            return remise;
        }




        private struct Remise
        {
            public double montantTotal;
            public Int32 nbPaiements;
            public string siteVirtuel;
            public string ribSociete;
            public string beneficiaire;
            public string libelleSociete;
        }

        private struct Lot
        {
            public string numero;
            public double montantTotal;
            public int nbPaiements;
            public List<Paiement> paiements;
        }

        private struct Paiement
        {
            public int position;
            public string gestionnaireRejet;
            public string contrat;
            public string numero;
            public double montant;
        }

        
    }

}
