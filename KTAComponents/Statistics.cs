using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using log4net;
using System.Reflection;

namespace KTAComponents
{
    public class Statistics
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int getTotalPaiements(string connectionString, string starttime, string endtime, string id_societe, string type_paiement)
        {
            int nb_paiements = 0;
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                string sql_statement = "SELECT count(distinct kta_jobid) As TOTAL from vPaiement ";
                bool start_with_and = false;

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;


                if(starttime !=null && starttime.Length >= 8)
                {
                    sql_statement += "where date_numerisation>= @start ";
                    cmd.Parameters.AddWithValue("@start", starttime);
                    start_with_and = true;
                }

                if(endtime!=null && endtime.Length >= 8)
                {
                    sql_statement += start_with_and ?  " AND ":" where ";
                    sql_statement += " date_numerisation <= @end";
                    cmd.Parameters.AddWithValue("@end", endtime);
                    start_with_and = true;
                }

                if (id_societe != null && id_societe.Length>0)
                {
                    sql_statement += start_with_and ? " AND " : " where ";
                    sql_statement += " numero_societe = @societe ";
                    cmd.Parameters.AddWithValue("@societe", id_societe);
                    start_with_and = true;
                }

                if (type_paiement != null && type_paiement.Length > 0)
                {
                    sql_statement += start_with_and ? " AND " : " where ";
                    sql_statement += " type_paiement = @type ";
                    cmd.Parameters.AddWithValue("@type", type_paiement);
                    start_with_and = true;
                }
                
                cmd.CommandText = sql_statement;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        if (reader.Read())
                        {
                            nb_paiements = Convert.ToInt32(reader["TOTAL"]);
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur Statistics.cs : " + e.Message + e.StackTrace);
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

            
            return nb_paiements;
        }

        public int getTotalPaiementsTerminesEnMoinsDe(int jours, string dbConnectionString, string startdate, string enddate, string processname, string societe)
        {
            int nbpaiements = 0;

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(dbConnectionString);
                conn.Open();
                string sql_statement = "SELECT count(distinct [FINISHED_JOB].JOB_ID) as TOTAL "
                                     + "FROM [FINISHED_JOB], [FINISHED_JOB_HISTORY], [FINISHED_JOB_VARIABLE] "
                                     + " where [FINISHED_JOB].JOB_ID=[FINISHED_JOB_HISTORY].JOB_ID "
                                     + " and Owner_ID = [FINISHED_JOB].JOB_ID "
                                     + " and DATEDIFF(day, [FINISHED_JOB].CREATION_TIME, [FINISHED_JOB].FINISH_TIME)<@nbjours"
                                     + " and [FINISHED_JOB].Process_name in ('Cheque Process', 'Effet Process', 'Mandat Facture Process', 'Mandatement Process')";
                
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@nbjours", jours);

                if(startdate!=null )
                {
                    sql_statement += " and [CREATION_TIME] > CONVERT(datetime,@start, 127)";
                    cmd.Parameters.AddWithValue("start", startdate);
                }

                if (enddate != null)
                {
                    sql_statement += " and [CREATION_TIME] <= CONVERT(datetime,@end, 127)";
                    cmd.Parameters.AddWithValue("end", enddate);
                }

                //@TODO à faire via la valeur de l'index
                if (processname != null && processname.Length>0)
                {
                    sql_statement += " and OWNER_ID IN (select distinct OWNER_ID from [FINISHED_JOB_VARIABLE] WHERE DISPLAY_NAME = 'TYPE_PAIEMENT' AND VAR_VALUE=@processname)";
                    cmd.Parameters.AddWithValue("processname", processname);
                }

                if (societe != null && societe.Length == 10)
                {
                    sql_statement += " and OWNER_ID OWNER_ID IN (select distinct OWNER_ID from [FINISHED_JOB_VARIABLE] WHERE DISPLAY_NAME = 'CHQ_SOCIETE' AND VAR_VALUE=@societe)";
                    cmd.Parameters.AddWithValue("societe", societe);
                }

                cmd.CommandText = sql_statement;
                //LoggerConfiguration.GetLogger().TraceError("Reporting - " + sql_statement + "jours : " + jours + " start : " + startdate + " end : " + enddate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if(reader.Read())
                            nbpaiements = Convert.ToInt32(reader["TOTAL"]);
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch(Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Reporting - " + e.Message + " \ndetails : "+e.StackTrace);
            }

            return nbpaiements;
        }

        public int getTotalPaiementsEtrangers(string connectionString, string starttime, string endtime, string id_societe, string type_paiement)
        {
            int nb_paiements = 0;
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                string sql_statement = "SELECT count(distinct kta_jobid) As TOTAL from vPaiement where etranger=1 ";
                

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;


                if (starttime != null && starttime.Length >= 8)
                {
                    sql_statement += "and date_numerisation>= @start ";
                    cmd.Parameters.AddWithValue("@start", starttime);
                    
                }

                if (endtime != null && endtime.Length >= 8)
                {
                    sql_statement += "and date_numerisation <= @end ";
                    cmd.Parameters.AddWithValue("@end", endtime);
                    
                }

                if (id_societe != null && id_societe.Length > 0)
                {
                    sql_statement += "and numero_societe = @societe ";
                    cmd.Parameters.AddWithValue("@societe", id_societe);
                    
                }

                if (type_paiement != null && type_paiement.Length > 0)
                {
                    sql_statement += "and type_paiement = @type ";
                    cmd.Parameters.AddWithValue("@type", type_paiement);
                }
                
                cmd.CommandText = sql_statement;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {

                        if (reader.Read())
                        {
                            nb_paiements = Convert.ToInt32(reader["TOTAL"]);
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur Statistics.cs : " + e.Message + e.StackTrace);
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


            return nb_paiements;
        }

        private int getNbStepsInLiveProcesses(string stepname, string dbConnectionString, string startdate, string enddate, string processname, string societe)
        {
            int nbpaiements = 0;

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(dbConnectionString);
                conn.Open();
                string sql_statement = "SELECT count(distinct [JOB].JOB_ID) as TOTAL "
                                     + "FROM [JOB], [JOB_HISTORY], [VARIABLE] "
                                     + " where [JOB].JOB_ID=[JOB_HISTORY].JOB_ID "
                                     + " and Owner_ID = [JOB].JOB_ID "
                                     + " and NODE_NAME like @stepname"
                                     + " and [JOB].Process_name in ('Cheque Process', 'Effet Process', 'Mandat Facture Process', 'Mandatement Process')";

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@stepname", stepname);

                if (startdate != null)
                {
                    sql_statement += " and [CREATION_TIME] > CONVERT(datetime,@start, 127)";
                    cmd.Parameters.AddWithValue("start", startdate);
                }

                if (enddate != null)
                {
                    sql_statement += " and [CREATION_TIME] <= CONVERT(datetime,@end, 127)";
                    cmd.Parameters.AddWithValue("end", enddate);
                }


                if (processname != null && processname.Length > 0)
                {
                    sql_statement += " and OWNER_ID IN (select distinct OWNER_ID from [JOB_VARIABLE] WHERE DISPLAY_NAME = 'TYPE_PAIEMENT' AND VAR_VALUE=@processname)";
                    cmd.Parameters.AddWithValue("processname", processname);
                }

                if (societe != null && societe.Length == 10)
                {
                    sql_statement += " and OWNER_ID OWNER_ID IN (select distinct OWNER_ID from [JOB_VARIABLE] WHERE DISPLAY_NAME = 'CHQ_SOCIETE' AND VAR_VALUE=@societe)";
                    cmd.Parameters.AddWithValue("societe", societe);
                }

                cmd.CommandText = sql_statement;
                //LoggerConfiguration.GetLogger().TraceError("Reporting - " + sql_statement + "jours : " + jours + " start : " + startdate + " end : " + enddate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                            nbpaiements = Convert.ToInt32(reader["TOTAL"]);
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Reporting - " + e.Message + " \ndetails : " + e.StackTrace);
            }

            return nbpaiements;
        }


        private int getNbStepsInArchivedProcesses(string stepname, string dbConnectionString, string startdate, string enddate, string processname, string societe)
        {
            int nbpaiements = 0;

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(dbConnectionString);
                conn.Open();
                string sql_statement = "SELECT count(distinct [FINISHED_JOB].JOB_ID) as TOTAL "
                                     + "FROM [FINISHED_JOB], [FINISHED_JOB_HISTORY], [FINISHED_JOB_VARIABLE] "
                                     + " where [FINISHED_JOB].JOB_ID=[FINISHED_JOB_HISTORY].JOB_ID "
                                     + " and Owner_ID = [FINISHED_JOB].JOB_ID "
                                     + " and NODE_NAME like @stepname"
                                     + " and OWNER_ID IN (select distinct OWNER_ID from [FINISHED_JOB_VARIABLE] WHERE DISPLAY_NAME = 'ETRANGER' AND VAR_VALUE = 1)"
                                     + " and [FINISHED_JOB].Process_name in ('Cheque Process', 'Effet Process', 'Mandat Facture Process', 'Mandatement Process')";

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@stepname", stepname);

                if (startdate != null)
                {
                    sql_statement += " and [CREATION_TIME] > CONVERT(datetime,@start, 127)";
                    cmd.Parameters.AddWithValue("start", startdate);
                }

                if (enddate != null)
                {
                    sql_statement += " and [CREATION_TIME] <= CONVERT(datetime,@end, 127)";
                    cmd.Parameters.AddWithValue("end", enddate);
                }


                if (processname != null && processname.Length > 0)
                {
                    sql_statement += " and OWNER_ID IN (select distinct OWNER_ID from [FINISHED_JOB_VARIABLE] WHERE DISPLAY_NAME = 'TYPE_PAIEMENT' AND VAR_VALUE=@processname)";
                    cmd.Parameters.AddWithValue("processname", processname);
                }

                if (societe != null && societe.Length == 10)
                {
                    sql_statement += " and OWNER_ID OWNER_ID IN (select distinct OWNER_ID from [FINISHED_JOB_VARIABLE] WHERE DISPLAY_NAME = 'CHQ_SOCIETE' AND VAR_VALUE=@societe)";
                    cmd.Parameters.AddWithValue("societe", societe);
                }

                cmd.CommandText = sql_statement;
                //LoggerConfiguration.GetLogger().TraceError("Reporting - " + sql_statement + "jours : " + jours + " start : " + startdate + " end : " + enddate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                            nbpaiements = Convert.ToInt32(reader["TOTAL"]);
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Reporting - " + e.Message + " \ndetails : " + e.StackTrace);
            }

            return nbpaiements;
        }


        public int getTotaNbStepsOccurred(string stepname, string dbKTAMainConnectionString, string dbKTAArchiveConnectionString, string startdate, string enddate, string processname, string societe)
        {
            int nbpaiements = 0;
            nbpaiements = getNbStepsInArchivedProcesses(stepname, dbKTAArchiveConnectionString, startdate, enddate, processname, societe);
            nbpaiements += getNbStepsInLiveProcesses(stepname, dbKTAMainConnectionString, startdate, enddate, processname, societe);


            return nbpaiements;
        }
    }
}
