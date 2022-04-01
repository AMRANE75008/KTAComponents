using System;
using System.IO;
using KTAComponents;


namespace KTA
{
    // gestion des fichiers d'export depuis KTA
    public class ExportManager
    {

        public void GenererFichierIndexesOnDemand(string pathOnDemand, string GED_COD_ORGA, string GED_COD_PAYS, string GED_DATE_OP, string GED_DEVISE, 
            string GED_FIC_HTML, string GED_HEURE_OP, string GED_NUMCPT, string GED_TICKETLOT, string GED_TYPEBORD, string MNTTOTREM, 
            string NBRCHQREJ, string NBRTOTCHQREM, int NUM_SOCIETE, string SITE_VIRTUEL, string TYPE_LOT)
        {
            try
            {

                // génération fichiet txt
                using (StreamWriter sw = File.CreateText(pathOnDemand))
                {
                    sw.WriteLine("GED_COD_ORGA," + GED_COD_ORGA.ToUpper());
                    sw.WriteLine("GED_COD_PAYS," + GED_COD_PAYS.ToUpper());
                    sw.WriteLine("GED_DATE_OP," + GED_DATE_OP.ToUpper());
                    sw.WriteLine("GED_DEVISE," + GED_DEVISE.ToUpper());
                    sw.WriteLine("GED_FIC_HTML," + GED_FIC_HTML);
                    sw.WriteLine("GED_HEURE_OP," + GED_HEURE_OP); // génerer au moment de l'export?
                    sw.WriteLine("GED_NUMCPT," + GED_NUMCPT.ToUpper());
                    sw.WriteLine("GED_TICKETLOT," + GED_TICKETLOT.ToUpper());
                    sw.WriteLine("GED_TYPEBORD," + GED_TYPEBORD.ToUpper());
                    sw.WriteLine("MNTTOTREM," + MNTTOTREM.Replace(',', '.'));
                    sw.WriteLine("NBRCHQREJ," + NBRCHQREJ.ToUpper());
                    sw.WriteLine("NBRTOTCHQREM," + NBRTOTCHQREM.ToUpper());
                    
                    //formattage sur 2 digits minimum
                    sw.WriteLine("NUM_SOC," + NUM_SOCIETE.ToString("D2"));
                    

                    sw.WriteLine("SITE_VIRTUEL," + SITE_VIRTUEL.ToUpper());
                    sw.WriteLine("TYP_LOT," + TYPE_LOT.ToUpper());
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                throw e;
            }

        }

            
        // génération de la remise aux formats txt et html pour OnDemand
        public void GenerateRemiseForOnDemand(string pathOnDemand,
           string typePaiement,
           string typeBordereau,
           string siteVirtuel,
           string refTicketLot,
           string numCompte,
           decimal numSociete,
           decimal nbRejets,
           string montantTotal,
           string heureOperation,
           string devise,
           DateTime dateOperation,
           string codePays,
           string codeOrganisme,
           string suffix,
           string txtExtension,
           string htmlExtension,
           string batchId,
           decimal nbCheques)
        {


            try
            {

                string formatedDateOperation = "";
                DateTime nullDate = new DateTime(1900, 1, 2);

                // si la date d'opération n'est pas nulle, on la formate correctement, sinon on la laisse à vide
                if (DateTime.Compare(nullDate, dateOperation) < 0)
                {
                    formatedDateOperation = dateOperation.ToString("yyyyMMdd");
                }

                string txtPath = pathOnDemand + batchId.ToUpper() + suffix + txtExtension;
                string htmlPath = pathOnDemand + batchId.ToUpper() + suffix + htmlExtension;

                // génération fichiet txt
                using (StreamWriter sw = File.CreateText(txtPath))
                {
                    sw.WriteLine("GED_COD_ORGA," + codeOrganisme.ToUpper());
                    sw.WriteLine("GED_COD_PAYS," + codePays.ToUpper());
                    sw.WriteLine("GED_DATE_OP," + formatedDateOperation);
                    sw.WriteLine("GED_DEVISE," + devise.ToUpper());
                    sw.WriteLine("GED_FIC_HTML," + batchId.ToUpper() + suffix + htmlExtension);
                    sw.WriteLine("GED_HEURE_OP," + heureOperation);
                    sw.WriteLine("GED_NUMCPT," + numCompte.ToUpper());
                    sw.WriteLine("GED_TICKETLOT," + refTicketLot.ToUpper());
                    sw.WriteLine("GED_TYPEBORD," + typeBordereau.ToUpper());
                    sw.WriteLine("MNTTOTREM," + montantTotal.Replace(',', '.'));
                    sw.WriteLine("NBRCHQREJ," + nbRejets);
                    sw.WriteLine("NBRTOTCHQREM," + nbCheques);
                    if (numSociete == -1)
                    {
                        sw.WriteLine("NUM_SOC,");
                    }
                    else
                    {
                        //formattage sur 2 digits minimum
                        sw.WriteLine("NUM_SOC," + Convert.ToInt32(numSociete).ToString("D2"));
                    }
                   
                    sw.WriteLine("SITE_VIRTUEL," + siteVirtuel.ToUpper());
                    sw.WriteLine("TYP_LOT," + typePaiement.ToUpper());
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }

        // génération de la remise aux formats txt et html pour OnDemand
        public void GenerateRemiseForOnDemand2(string pathOnDemand,
           string typePaiement,
           string typeBordereau,
           string siteVirtuel,
           string refTicketLot,
           string numCompte,
           decimal numSociete,
           decimal nbRejets,
           string montantTotal,
           string heureOperation,
           string devise,
           String formatedDateOperation,
           string codePays,
           string codeOrganisme,
           string suffix,
           string txtExtension,
           string htmlExtension,
           string batchId,
           decimal nbCheques)
        {

            try{

                string txtPath = pathOnDemand + batchId.ToUpper() + suffix + txtExtension;
                string htmlPath = pathOnDemand + batchId.ToUpper() + suffix + htmlExtension;

                // génération fichiet txt
                using (StreamWriter sw = File.CreateText(txtPath))
                {
                    sw.WriteLine("GED_COD_ORGA," + codeOrganisme.ToUpper());
                    sw.WriteLine("GED_COD_PAYS," + codePays.ToUpper());
                    sw.WriteLine("GED_DATE_OP," + formatedDateOperation);
                    sw.WriteLine("GED_DEVISE," + devise.ToUpper());
                    sw.WriteLine("GED_FIC_HTML," + batchId.ToUpper() + suffix + htmlExtension);
                    sw.WriteLine("GED_HEURE_OP," + heureOperation);
                    sw.WriteLine("GED_NUMCPT," + numCompte.ToUpper());
                    sw.WriteLine("GED_TICKETLOT," + refTicketLot.ToUpper());
                    sw.WriteLine("GED_TYPEBORD," + typeBordereau.ToUpper());
                    sw.WriteLine("MNTTOTREM," + montantTotal.Replace(',', '.'));
                    sw.WriteLine("NBRCHQREJ," + nbRejets);
                    sw.WriteLine("NBRTOTCHQREM," + nbCheques);
                    if (numSociete == -1)
                    {
                        sw.WriteLine("NUM_SOC,");
                    }
                    else
                    {
                        //formattage sur 2 digits minimum
                        sw.WriteLine("NUM_SOC," + Convert.ToInt32(numSociete).ToString("D2"));
                    }
                   
                    sw.WriteLine("SITE_VIRTUEL," + siteVirtuel.ToUpper());
                    sw.WriteLine("TYP_LOT," + typePaiement.ToUpper());
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }

        // génération du paiement au format txt pour OnDemand
        public void GeneratePaiementForOnDemand(string pathOnDemand,
            string suffix,
            string tiffExtension,
            string txtExtension,
            string batchId,
            decimal archMt,
            DateTime dateEcheanceEffet,
            DateTime dateNumerisation,
            string codePays,
            string codeOrganisme,
            string devise,
            string chequeCMC7,
            string typePaiement,
            string numContrat,
            string numTiers,
            string numFacture,
            string numLot,
            string numLotOrigine,
            decimal numSociete,
            DateTime dateRemiseMandat,
            string sousTypeLot)
        {

            try
            {


                string formatedDateEcheanceEffet = "";
                string formatedDateNumerisation = "";
                string formatedDateRemiseMandat = "";



                // formatage des dates si renseignées, sinon on laisse à vide
                DateTime nullDate = new DateTime(1900, 1, 2);
                if (DateTime.Compare(nullDate, dateEcheanceEffet) < 0)
                {
                    formatedDateEcheanceEffet = dateEcheanceEffet.ToString("yyyyMMdd");
                }
                if (DateTime.Compare(nullDate, dateNumerisation) < 0)
                {
                    formatedDateNumerisation = dateNumerisation.ToString("yyyyMMdd");
                }
                if (DateTime.Compare(nullDate, dateRemiseMandat) < 0)
                {
                    formatedDateRemiseMandat = dateRemiseMandat.ToString("yyyyMMdd");
                }


                string txtPath = pathOnDemand + batchId.ToUpper() + suffix + txtExtension;
                int formatNumSoc = Convert.ToInt32(numSociete);

                // génération fichiet txt
                using (StreamWriter sw = File.CreateText(txtPath))
                {
                    sw.WriteLine("ARCH_MT," + (Convert.ToString(archMt)).Replace(',', '.'));
                    sw.WriteLine("DAT_ECH_EFTR," + formatedDateEcheanceEffet);
                    sw.WriteLine("DAT_SCANR," + formatedDateNumerisation);
                    sw.WriteLine("FICHIERTIF_REST," + batchId.ToUpper() + suffix + tiffExtension);
                    sw.WriteLine("GED_COD_ORGA," + codeOrganisme.ToUpper());
                    sw.WriteLine("GED_COD_PAYS," + codePays.ToUpper());
                    sw.WriteLine("GED_DEVISE," + devise.ToUpper());
                    sw.WriteLine("IDX_CHQ_CMC7," + chequeCMC7.ToUpper());
                    sw.WriteLine("NO_CONTRAT," + numContrat.ToUpper());
                    sw.WriteLine("NO_TIERS," + numTiers.ToUpper());
                    sw.WriteLine("NUM_FACTURE," + numFacture.ToUpper());
                    sw.WriteLine("NUM_LOT," + numLot.ToUpper());
                    sw.WriteLine("NUM_LOT_ORIG," + numLotOrigine.ToUpper());

                    //formattage du numero societe sur 2 caractères (au moins): 1 -> 01
                    sw.WriteLine("NUM_SOC," + formatNumSoc.ToString("D2"));

                    sw.WriteLine("REM_DAT_MANDATR," + formatedDateRemiseMandat);
                    sw.WriteLine("SS_TYP_LOT," + sousTypeLot.ToUpper());
                    sw.WriteLine("TYP_LOT," + typePaiement.ToUpper());
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        // déplacement fichier avec construction du chemin et du nom de destination
        // sourceFile : fichier à déplacer
        // destPath : chemin de destination
        // batchId : identifiant du processus actif à intégrer dans le nom du fichier de destination
        // suffix : suffixe à ajouter au nom du fichier des destination
        // extension : extension du fichier des destination
        public void moveFile(string sourceFile, string destPath, string batchId, string suffix, string extension)
        {

            try
            {

                string targetFile = destPath + batchId + suffix + extension;
                File.Move(sourceFile, targetFile);
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                throw e;
            }
        }


        public bool FileExists(string filePath)
        {

            try
            {

                return File.Exists(filePath);
            }
            catch (Exception e)
            {
                LoggerConfiguration.GetLogger().TraceError("Erreur ExportManager.cs : " + e.Message + e.StackTrace);
                return false;
            }
        }


        public void AppendToFile(string row, string filepath)
        {
            try
            {
                System.IO.File.AppendAllText(filepath, row + Environment.NewLine);

            }
            catch
            {

            }
        }
        
        public void createFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void createContainingFolder(string filepath)
        {
            createFolder(filepath.Substring(0, filepath.LastIndexOf("\\")));
        }
    }
}
