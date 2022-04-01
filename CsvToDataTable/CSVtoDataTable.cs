using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvToDataTable
{
    public class Fournisseur
    {
        public string NumFournisseur { get; set; }
        public string NomFournisseur { get; set; }
        public string CodeSiege { get; set; }
        public string CodePostal { get; set; }
        public string Ville { get; set; }
        public string Status { get; set; }
    }
    public class CSVtoDataTable
    {
        
        public Fournisseur GetFournisseur(string numFournisseur, string PathFile)
        {
            Fournisseur four = new Fournisseur();
            
            DataTable dt = GetDataTableFromCSVFile(PathFile).Result;
            
            try
            {
                if (File.Exists(PathFile) && !string.IsNullOrEmpty(numFournisseur))
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (dt.Rows[i][0].ToString() == numFournisseur)
                        {
                            four.NumFournisseur = dt.Rows[i][0].ToString();
                            four.NomFournisseur = dt.Rows[i][1].ToString();
                            four.CodeSiege = dt.Rows[i][12].ToString();
                            four.CodePostal = dt.Rows[i][4].ToString();
                            four.Ville = dt.Rows[i][5].ToString();
                            four.Status = "";

                            return four;
                        }
                        
                        

                    }

                    

                }
                else if (string.IsNullOrEmpty(numFournisseur))
                {
                    four.Status = "Le code fournisseur ne devrais pas etres null";
                    return four;
                }

                else
                    four.Status = "Le chemin du fichier n'existe pas";
                return four;

            }
            catch(Exception ex)
            {
                four.Status = ex.Message;
                return four;
            }     
            


        }

        
        
        //Retourne le contenue du csv dans un  Datatable
        private async Task<DataTable> GetDataTableFromCSVFile(string csvFilePath)
        {
            DataTable csvData = new DataTable();
            try
            {
                await Task.Delay(20000);
                {
                    using (TextFieldParser csvReader = new TextFieldParser(csvFilePath))
                    {
                        csvReader.SetDelimiters(new string[] { ";" });
                        csvReader.HasFieldsEnclosedInQuotes = true;
                        string[] colFields = csvReader.ReadFields();
                        foreach (string column in colFields)
                        {
                            DataColumn datecolumn = new DataColumn(column);
                            datecolumn.AllowDBNull = true;
                            csvData.Columns.Add(datecolumn);
                        }

                        while (!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();
                            //Making empty value as null
                            for (int i = 0; i < fieldData.Length; i++)
                            {
                                if (fieldData[i] == "")
                                {
                                    fieldData[i] = null;
                                }
                            }
                            csvData.Rows.Add(fieldData);
                        }

                    }
                }
                
                
            }
            catch (Exception ex)
            {
                
            }
            
            return  csvData;

        }
    }
}
