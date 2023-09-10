using DataToolKit.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataToolKit.DataAccess
{
    public class DTKDB
    {
        private SqlConnection sqlcon;
        public DTKDB(IConfiguration configuration)
        { 
            sqlcon = new(configuration.GetConnectionString("DataToolKitDbContextConnection"));
        }
        
        public int InsertBatchControl(BatchControl BC)
        {
            int BatchId = 0;

            try
            {
                SqlCommand cmd = new SqlCommand("Insert_Batch_Control_File", sqlcon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BC.BatchId);
                cmd.Parameters.AddWithValue("@Status_Description", "In Progress");
                cmd.Parameters.AddWithValue("@Submit_Date", BC.SubmitDate);
                cmd.Parameters.AddWithValue("@Submit_Name", BC.SubmitName); //Logged in user
                cmd.Parameters.AddWithValue("@Vendor_Name", BC.VendorName); //blank or null 
                cmd.Parameters.AddWithValue("@Customer_Name", BC.CustomerName);
                cmd.Parameters.AddWithValue("@Request_Type_Code", BC.RequestTypeCode);
                cmd.Parameters.AddWithValue("@Description_Tile", BC.DescriptionTitle); //blank or null
                cmd.Parameters.AddWithValue("@Report_Title", BC.ReportTitle);
                cmd.Parameters.AddWithValue("@Project_Code", BC.ProjectCode);
                cmd.Parameters.AddWithValue("@Input_file_Name", BC.InputFileName);
                cmd.Parameters.AddWithValue("@input_record_count", BC.InputRecordCount);
                cmd.Parameters.AddWithValue("@Results_Email_1", BC.ResultEmail1);//blank or null
                cmd.Parameters.AddWithValue("@Results_Email_2", BC.ResultEmail2);//blank or null
                cmd.Parameters.AddWithValue("@Results_Email_3", BC.ResultEmail3);//blank or null
                cmd.Parameters.AddWithValue("@Results_Email_4", BC.ResultEmail4);//blank or null
                cmd.Parameters.AddWithValue("@Results_Email_5", BC.ResultEmail5);//blank or null
                cmd.Parameters.AddWithValue("@Batch_ID_Original", BC.OrginalBatchId);
                cmd.Parameters.Add("@Output_BatchID", SqlDbType.Int);
                cmd.Parameters["@Output_BatchID"].Direction = ParameterDirection.Output;


                sqlcon.Open();
                int i = cmd.ExecuteNonQuery();

                BatchId = Convert.ToInt32(cmd.Parameters["@Output_BatchID"].Value);

                sqlcon.Close();

                return BatchId;

            }
            catch (Exception ex)
            {
                if (sqlcon.State == ConnectionState.Open)
                {
                    sqlcon.Close();
                }

                throw (ex);

            }
        }

        public string InsertBatchDataFile(BatchDataFile BDF)
        {


            try
            {
                SqlCommand cmd = new SqlCommand("Insert_Batch_data_File", sqlcon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BDF.BatchId);
                cmd.Parameters.AddWithValue("@Batch_NPI", BDF.NPI);
                cmd.Parameters.AddWithValue("@Batch_Segment", BDF.Segment);


                sqlcon.Open();
                cmd.ExecuteNonQuery();
                sqlcon.Close();
                return ("success");

            }
            catch (Exception ex)
            {
                if (sqlcon.State == ConnectionState.Open)
                {
                    sqlcon.Close();
                }

                return (ex.Message.ToString());

            }
        }

        public List<BatchControl> GetBatchControls()
        {
            SqlCommand command = new SqlCommand("select * from [dbo].[Batch_Control_File]", sqlcon);
            command.CommandType = CommandType.Text;
            sqlcon.Open();
            SqlDataReader reader = command.ExecuteReader();

            List<BatchControl> BatchControls = new List<BatchControl>();
            BatchControl batch = null;

            while (reader.Read())
            {
                batch = new BatchControl();
                batch.BatchId = Convert.ToInt32(reader["BatchID"]);
                batch.StatusDescription = reader["Status_Description"].ToString();
                batch.VendorName = reader["Vendor_Name"].ToString();
                batch.CustomerName = reader["Customer_Name"].ToString();
                batch.RequestTypeCode = reader["Request_Type_Code"].ToString();
                batch.DescriptionTitle = reader["Description_Tile"].ToString();
                batch.ReportTitle = reader["Report_Title"].ToString();
                batch.ProjectCode = reader["Project_Code"].ToString();
                batch.InputFileName = reader["Input_file_Name"].ToString();
                batch.InputRecordCount = reader["input_record_count"].ToString();
                batch.ResultEmail1 = reader["Results_Email_1"].ToString();
                batch.SubmitName = reader["Submit_Name"].ToString();
                DateTime SubmitDate = Convert.ToDateTime(reader["Submit_Date"].ToString());
                batch.SubmitDate = SubmitDate.ToString("dd/MM/yyyy");
                BatchControls.Add(batch);
            }
            return BatchControls;
        }

        public List<BatchDataFile> GetBatchData(int batchId)
        {
            SqlCommand command = new SqlCommand("select * from [Uncovered].[Batch_data_File] where BatchID = " + batchId, sqlcon);
            command.CommandType = CommandType.Text;
            sqlcon.Open();
            SqlDataReader reader = command.ExecuteReader();

            List<BatchDataFile> batchDataFile = new List<BatchDataFile>();
            BatchDataFile batchdata = null;

            while (reader.Read())
            {
                batchdata = new BatchDataFile();
                batchdata.BatchId = int.Parse(reader["BatchID"].ToString());
                batchdata.NPI = int.Parse(reader["Batch_NPI"].ToString());
                batchdata.Segment = reader["Batch_Segment"].ToString();
                batchDataFile.Add(batchdata);
            }
            return batchDataFile;
        }
    }
}
