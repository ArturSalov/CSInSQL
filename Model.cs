using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    struct Column
    {
        public string Name;
        public Type type;
        public bool typeIsDig() => type.IsPrimitive || type == typeof(Decimal);
    }

    struct data { public Column[] Columns; public List<object[]> Rows; }

    class Model
    {
        Column[] AllColumns, tmpColumnName;
        List<int> Indexs;
        List<object[]> rows;
        SqlConnection sqlConnection;
        SqlDataReader dataReader;

        public data Data
        {
            get
            {
                //GetData();
                return new data
                {
                    Columns = tmpColumnName,
                    Rows = new List<object[]>(rows)
                };
            }
        }

        public Model(string ConnectionString)
        {
            sqlConnection = new SqlConnection(ConnectionString);
            Indexs = new List<int>();
            dataReader = null;
            rows = new List<object[]>();
        }
        public Model()
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["MyDBConnectionString"].ConnectionString);
            Indexs = new List<int>();
            dataReader = null;
            rows = new List<object[]>();
        }

        public void AddIndex(int index) => Indexs.Add(index);
        public void RemoveIndex(int index)=>Indexs.Remove(index); 
        public bool ContainsIndex(int index)=>Indexs.Contains(index);

        private string GetCommandString()
        {
            string CommandString = "SELECT * FROM [Table]";
            if (AllColumns != null && Indexs.Count != 0 && Indexs.Count != AllColumns.Length)
            {
                CommandString = "SELECT ";
                string Columns = "", Groupby = " GROUP BY ", OrderBy = " ORDER BY ";
                for (int i = 0; i < this.AllColumns.Length; i++)
                    if (!Indexs.Contains(i))
                    {
                        if (AllColumns[i].typeIsDig())
                            Columns = String.Concat(Columns, "Sum(", AllColumns[i].Name,
                                ") as ", AllColumns[i].Name, ", ");
                        else
                        {
                            Columns = String.Concat(Columns, AllColumns[i].Name, ", ");
                            OrderBy = String.Concat(OrderBy, AllColumns[i].Name, ", ");
                            Groupby = String.Concat(Groupby, AllColumns[i].Name, ", ");
                        }
                    }
                Columns = Columns.Substring(0, Columns.Length - 2);
                Groupby = Groupby.Substring(0, Groupby.Length - 2);
                OrderBy = OrderBy.Substring(0, OrderBy.Length - 2);
                CommandString = String.Concat(CommandString, Columns, " FROM[Table]", Groupby, OrderBy);
            }
            return CommandString;
        }


        public async Task GetDataAsync()
        {
            tmpColumnName = null;
            try
            {
                sqlConnection.Open();

                SqlCommand command = new SqlCommand(GetCommandString(), sqlConnection);
                dataReader = await command.ExecuteReaderAsync();
                if (AllColumns == null)
                {
                    AllColumns = GetColumnName(dataReader);
                    tmpColumnName = AllColumns.Clone() as Column[];
                }
                else
                    tmpColumnName = GetColumnName(dataReader);
                rows.Clear();
                for (int i = 0; await dataReader.ReadAsync(); i++)
                    rows.Add(GetStringsDB(i, dataReader));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Невозможно открыть подключение без указания источника данных или сервера \n или Подключение уже открыто.", ex.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("При открытии подключения произошла ошибка на уровне подключения.", ex.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ConfigurationErrorsException ex)
            {
                MessageBox.Show("В разделе <localdbinstances> присутствуют две записи с одинаковым именем.", ex.Source,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dataReader?.Close();
                sqlConnection?.Close();
            }
        }

        private Column[] GetColumnName(SqlDataReader dataReader)
        {
            try
            {
                Column[] res = new Column[dataReader.FieldCount];

                for (int i = 0; i < dataReader.FieldCount; i++)
                    res[i] = new Column
                    {
                        Name = dataReader.GetName(i),
                        type = dataReader.GetFieldType(i)
                    };
                return res;
            }
            catch { return null; }
        }

        private object[] GetStringsDB(int index, SqlDataReader dataReader)
        {
            object[] arr = new object[dataReader.FieldCount];
            dataReader.GetValues(arr);
            return arr;
        }

    }
}
