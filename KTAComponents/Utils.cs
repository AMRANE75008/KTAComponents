using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Agility.Sdk.Model.Capture;
using System.Data;
using Agility.Sdk.Services;

namespace KTAComponents
{
    public class Utils
    {
        public void UpdateEffetInfos(string db_conn_str, string kta_jobid, Decimal amount)
        {
            SqlConnection connection = new SqlConnection(db_conn_str);
            String sql_request = "UPDATE EFFET SET eff_montant=@montant where kta_job_id=@jobid";

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@montant", amount/100);
                cmd.Parameters.AddWithValue("@jobid", kta_jobid);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex){
                LoggerConfiguration.GetLogger().TraceError("Erreur lors de la mise à jour DB de l'effet:" + ex.Message + " \n " + ex.StackTrace);
            }
        }

        public void UpdateChequeInfos(string db_conn_str, string kta_jobid, Decimal amount)
        {
            SqlConnection connection = new SqlConnection(db_conn_str);
            String sql_request = "UPDATE Cheque SET chq_montant=@montant where kta_job_id=@jobid";

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@montant", amount / 100);
                cmd.Parameters.AddWithValue("@jobid", kta_jobid);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur lors de la mise à jour DB de l'effet:" + ex.Message + " \n " + ex.StackTrace);
            }


        }

        public void UpdateMandatInfos(string db_conn_str, string kta_jobid, Decimal amount)
        {
            SqlConnection connection = new SqlConnection(db_conn_str);
            String sql_request = "UPDATE EFFET SET mdt_montant=@montant where kta_job_id=@jobid";

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql_request;
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@montant", amount / 100);
                cmd.Parameters.AddWithValue("@jobid", kta_jobid);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur lors de la mise à jour DB de l'effet:" + ex.Message + " \n " + ex.StackTrace);
            }


        }

        public string getComputerName()
        {
            return Environment.MachineName;
        }

        public string FormatDecimal(Decimal d, string format, string culture)
        {
            return d.ToString(format, new System.Globalization.CultureInfo(culture));
        }


        public Decimal ParseToDecimal(string value)
        {
            Decimal retour = 0;

            try
            {

                Decimal.TryParse(value, System.Globalization.NumberStyles.Currency, new System.Globalization.CultureInfo("fr-FR"), out retour);
            }
            catch {
                try
                {
                    Decimal.TryParse(value, System.Globalization.NumberStyles.Currency, new System.Globalization.CultureInfo("en-US"), out retour);
                }
                catch { }
            }
            return retour;
        }

        public DateTime Parse(string value, string format)
        {
            DateTime d = DateTime.Now; 
            
            if(!DateTime.TryParse(value, new System.Globalization.CultureInfo("fr-FR"), System.Globalization.DateTimeStyles.AdjustToUniversal,  out d))
            {
                d = DateTime.Now;
                LoggerConfiguration.GetLogger().TraceError("Erreur de conversion de date : " + value);
            }

            return d;
        }
        
        public DateTime Convert(string day, string month, string year)
        {
            return Parse(day + "/" + month + "/" + year, "dd/MM/yyyy");

        }


        public string Now(string DateFormat)
        {
            return DateTime.Now.ToString(DateFormat);
        }

        public DateTime AddDays(DateTime d, int nb)
        {
            return d.AddDays(nb);
        }

        public bool Not(bool input)
        {
            return !input;
        }
        

        // retourne un booléen en fonction d'un entier
        public bool ConvertIntToBool(decimal val)
        {
            if (val == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string FormatDate(DateTime d, string format)
        {
            return d.ToString(format);
        }

        // vérifie si la date passée en paramètre est une date future
        // date : date à vérifier
        public bool IsFutureDate(DateTime date)
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                int res = DateTime.Compare(tomorrow, date);

                if (res <= 0)
                {

                    return true;
                }
                else
                {

                    return false;
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur utils.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        // supprime le dossier indiqué et son contenu éventuel (si le dossier existe)
        // path : dossier à supprimer avec son contenu éventuel
        public void Purge(string path)
        {
            try
            {
                bool exists = Directory.Exists(path);
                if (exists)
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur utils.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }




        // supprime une partie d'arborescences du type <année>/<mois>/<jour>
        // la suppression se fait sur les périodes datant de plus de X jours
        // path : racine de l'arborescences <année>/<mois>/<jour>
        // days : période en jours avant laquelle supprimer une portion d'arborescence
        public void RemovePathCompletePeriod(string path, int days)
        {
            try
            {
                // construction date seuil
                DateTime threshold = DateTime.Now;
                TimeSpan timeSpan = new TimeSpan(days, 0, 0, 0);
                threshold = threshold.Subtract(timeSpan);


                // vérification et purge des années avant seuil de suppression          
                RemovePathPeriod(path, threshold.Year);
                // vérification et purge des mois pour l'année à cheval sur la suppression et la conservation de documents
                RemovePathPeriod(path + "\\" + threshold.Year, threshold.Month);
                // vérification et purge des jours pour l'année et le mois à cheval sur la suppression et la conservation de documents
                RemovePathPeriod(path + "\\" + threshold.Year + "\\" + threshold.Month, threshold.Day);
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur utils.cs : " + e.Message + e.StackTrace);
                throw e;
            }


        }


        // supprime les dossiers portant un nom du type entier jusqu'à un numéro donné
        // path : dossier racine
        // threshold : nombre en dessous duquel supprimer les dossiers
        private void RemovePathPeriod(string path, int threshold)
        {
            try
            {

                // si le dossier racine n'existe pas, on ne fait rien
                bool exists = Directory.Exists(path);
                if (exists)
                {
                    // on liste les dossiers contenus dans le dossier racine
                    string[] dirs = Directory.GetDirectories(path);

                    // pour chacun des dossiers contenus
                    for (int i = 0; i < dirs.Length; i++)
                    {

                        try
                        {
                            // on récupère le nom du dossier
                            char[] delim = { '\\' };
                            string[] tmp = dirs[i].Split(delim);
                            string dayDir = tmp[tmp.Length - 1];
                            Int32 intDayhDir;

                            bool success = Int32.TryParse(dayDir, out intDayhDir);
                            // si le dossier fait partie de ceux à supprimer
                            if (success && intDayhDir < threshold)
                            {
                                // on le supprime avec son contenu
                                Purge(dirs[i]);
                            }
                        }
                        catch
                        {
                            // si plantage sur un des dossiers, on poursuit le traitement pour les autres
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur utils.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        public void CopyFile(string source, string dest)
        {
            try
            {
                File.Copy(source,dest,true);
            }
            catch (Exception e)
            { 
                LoggerConfiguration.GetLogger().TraceError("Erreur utils.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }

        public bool FileName(string path, out string fileName,out string errMessage)
        {
            bool cheminCorrect = true;
            fileName = string.Empty;
            errMessage = null;
            try
            {
                string[] listPath = null;
               
                if (!string.IsNullOrEmpty(path))
                {
                    path = path.Replace("\\", " ");
                    listPath = path.Split(' ');
                    for (int i = 0; i < listPath.Length; i++)
                    {
                        if (i + 1 == listPath.Length)
                        {
                            fileName = listPath[i];
                            return cheminCorrect;
                        }
                    }
                }
                else
                {
                    cheminCorrect = false;
                }
                return cheminCorrect;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return cheminCorrect;
            }
            
        }

        public void concatTwoString(string sInput1, string sInput2, out string sOutPut)
        {
            sOutPut = sInput1 + sInput2;
            
        }

        public bool UpdateSQLTableFromCSV(string PathFile,  string connString, string tableName, out string errMessage)
        {
            

            int nobColumn = 0;
            string columns = null;
            errMessage = "";
            try
            {
                //lire le fichier csv
                var csvRow = File.ReadAllLines(PathFile, Encoding.Default).ToList();
                int rowCount = 0;
                
                SqlConnection conn = new SqlConnection(connString);

                using (conn )
                {
                  


                    #region Ouvrir la tableName et récuperer les colonnes dans une seule ligne séparer par une virgule
                    conn.Open();

                    var sql = "SELECT * FROM " + tableName;
                    var cmd = new SqlCommand(sql, conn);

                    SqlDataReader rdr = cmd.ExecuteReader();
                    cmd.Connection = conn;

                    //récupere les colonne de la table et les stocké dans columns
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        nobColumn = rdr.FieldCount;
                        columns += rdr.GetName(i) + ",";
                    }

                    conn.Close();
                    #endregion fermer la requette

                    conn.Open();

                    //enlevé  la derniere (,) 
                    columns = columns.Substring(0, columns.Length - 1);

                    //ignioré la premiere ligne
                    foreach (var row in csvRow.Skip(1))
                    {

                        // initialiser a chaque fois la ligne 
                        string Vcolumn = "";

                        var rowClean = row.ToString().Replace("'", "''");
                        var column = rowClean.Split(';');
                        cmd.Connection = conn;

                        for (int j = 0; j < nobColumn; j++)
                        {
                            Vcolumn += "'" + column[j] + "'" + ",";

                        }
                        Vcolumn = Vcolumn.Substring(0, Vcolumn.Length - 1);

                        string sqlInsert = "insert into  [dbo].[" + tableName + "] (" + columns + ") values (" + Vcolumn + ")";
                        cmd.CommandText = sqlInsert;
                        rowCount = cmd.ExecuteNonQuery() + rowCount;

                    }

                    errMessage = "Mise a jours de la table effectué avec succès";

                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message.ToString();
                return false;
            }

        }
        /// <summary>
        /// Suppresion des données d'une table
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="tableName"></param>
        /// <param name="errMessage"></param>
        /// <returns></returns>

        public bool deleteTable(string connString ,string tableName, out string errMessage)
        {
            errMessage = "";
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                int rowCount = 0;
                using ( conn )
                {

                    conn.Open();
                    var sql = "truncate table " + tableName;
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        rowCount = cmd.ExecuteNonQuery() + rowCount;
                        conn.Close();
                        conn.Dispose();
                    }
                    errMessage = "Les données de la tables sont bien tranqués";


                }
                return true;
            }
            catch (Exception e)
            {
                errMessage = "Erreur : " + e.Message;
                return false;
            }
        }

        public bool isNumber(string s, out string errMessage)
        {
            errMessage = "";
            bool isnumber = false;
            try
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (s.All(char.IsDigit))
                    {
                        isnumber = true;
                        errMessage = "Cette chaine est bien un nombre";
                        return isnumber;

                    }

                    else
                    {
                        isnumber = false;
                        errMessage = "Cette chaine n'est pas un nombre veuillez vérifier";
                        return isnumber;
                    }
                    
                }
                else
                {
                    errMessage = "La chaine ne peut pas être vide";
                    return false;
                }
                    
                   
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                isnumber = false;
                return isnumber;

            }
            



        }


        public void UpdateDocumentFieldValue(string sessionId, string docId,string tableName,int indexColumn,string newValue)
        {
            var cds = new CaptureDocumentService();

            RuntimeFieldCollection rfc = new RuntimeFieldCollection();
            TableFieldIdentity tfi = new TableFieldIdentity();
            tfi.Name = tableName;
            DataSet ds = cds.GetDocumentTableFieldValue(sessionId, null, docId, tfi);
            DataTable dt = ds.Tables[0];

            int lines = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lines++;
                rfc.Add(new RuntimeField() { Name = tableName, TableColumn = indexColumn, TableRow = i, Value = newValue });
            }

            cds.UpdateDocumentFieldValues(sessionId, null, docId, rfc);

        }



    }
}
