using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Lab_4
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=inventar.db;Version=3;";

        public Form1()
        {
            InitializeComponent();
            InitDatabase();
            LoadDataFromDb();
        }

        private void InitDatabase()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        CREATE TABLE IF NOT EXISTS person (
                            id_person INTEGER PRIMARY KEY,
                            name TEXT,
                            position TEXT,
                            code INTEGER
                        );
                        INSERT OR IGNORE INTO person (id_person, name, position, code) VALUES
                        (1, 'Олег', 'Керівник', 3),
                        (2, 'Марія', 'Секретар', 4),
                        (3, 'Василина', 'Заступник', 6),
                        (4, 'Петро', 'Бухгалтер', 7);";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка SQLite: " + ex.Message);
            }
        }

        private void LoadDataFromDb()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM person", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch { }
        }

        // Кнопка 1
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            dt.Columns.Add(new DataColumn("ID", typeof(int)));
            dt.Columns.Add(new DataColumn("Name", typeof(string)));
            dt.Columns.Add(new DataColumn("Age", typeof(int)));
            dt.Columns.Add(new DataColumn("Phone", typeof(string)));

            fillRows(1, "Олег", 27, "+380982328654", dt);
            fillRows(2, "Наталя", 45, "+380684291317", dt);
            fillRows(3, "Богдан", 24, "+380964175896", dt);
            fillRows(4, "Інна", 26, "+380979797573", dt);

            ds.Tables.Add(dt);
            ds.Tables[0].TableName = "client";
            ds.WriteXml("Client.xml");

            MessageBox.Show("Готово!!!");
        }

        private void fillRows(int PID, string pName, int pAge, string pPhone, DataTable dt)
        {
            DataRow dr = dt.NewRow();
            dr["ID"] = PID;
            dr["Name"] = pName;
            dr["Age"] = pAge;
            dr["Phone"] = pPhone;
            dt.Rows.Add(dr);
        }

        // Кнопка 2
        private void button2_Click(object sender, EventArgs e)
        {
            DataSet ds = new DataSet();
            string sql = "SELECT * FROM person";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, connection);
                    adapter.Fill(ds);
                    ds.WriteXml("Person.xml");
                    MessageBox.Show("Готово!!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Кнопка 3
        private void button3_Click(object sender, EventArgs e)
        {
            if (File.Exists("Person.xml"))
            {
                using (XmlReader xmlFile = XmlReader.Create("Person.xml", new XmlReaderSettings()))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlFile);

                    DataView dv = new DataView(ds.Tables[0], "code > 5", "name", DataViewRowState.CurrentRows);
                    dv.ToTable().WriteXml("Result.xml");
                    MessageBox.Show("Готово!!!");
                }
            }
            else
            {
                MessageBox.Show("Файл Person.xml відсутній!!! Спершу створіть його!");
            }
        }

        // Кнопка 4
        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists("Person.xml"))
            {
                using (XmlReader xmlFile = XmlReader.Create("Person.xml", new XmlReaderSettings()))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlFile);

                    DataView dv = new DataView(ds.Tables[0]);
                    dv.Sort = "name";

                    string search_name = textBox1.Text.Trim();
                    int index = dv.Find(search_name);

                    if (index == -1)
                    {
                        MessageBox.Show("Даний запис відсутній!!!");
                    }
                    else
                    {
                        MessageBox.Show(dv[index]["name"].ToString() + " - " + dv[index]["position"].ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Файл Person.xml відсутній!!! Спершу створіть його!");
            }
        }

        // Кнопка 5
        private void button5_Click(object sender, EventArgs e)
        {
            string searchPos = "";
            if (Controls.Find("pos", true).Length > 0)
                searchPos = Controls.Find("pos", true)[0].Text.Trim();
            else if (Controls.Find("textBox2", true).Length > 0)
                searchPos = Controls.Find("textBox2", true)[0].Text.Trim();
            else
                searchPos = textBox1.Text.Trim();

            string xmlFile = File.Exists("Result.xml") ? "Result.xml" : (File.Exists("Person.xml") ? "Person.xml" : null);

            if (xmlFile == null)
            {
                MessageBox.Show("Спочатку натисніть 'Створити XML на основі БД'!");
                return;
            }

            DataSet ds = new DataSet();
            ds.ReadXml(xmlFile);

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("Даний запис відсутній!!!");
                return;
            }

            DataTable dtResult = ds.Tables[0].Clone();
            string foundList = "";

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string posValue = row["position"] != null ? row["position"].ToString().Trim() : "";

                if (posValue.Equals(searchPos, StringComparison.OrdinalIgnoreCase) ||
                   (searchPos.Length > 0 && posValue.ToLower().Contains(searchPos.ToLower())))
                {
                    dtResult.ImportRow(row);
                    foundList += row["name"] + " — " + row["position"] + "\n";
                }
            }

            if (dtResult.Rows.Count > 0)
            {
                DataSet dsNew = new DataSet();
                dsNew.Tables.Add(dtResult);
                dsNew.WriteXml("Result2.xml");

                MessageBox.Show("Знайдено записів (" + dtResult.Rows.Count + "):\n\n" + foundList, "Результат вибірки");
            }
            else
            {
                MessageBox.Show("Даний запис відсутній!!!");
            }
        }

        // Кнопка 6
        private void button6_Click(object sender, EventArgs e)
        {
            if (File.Exists("NewPerson.xml"))
            {
                DataSet ds = new DataSet();
                using (XmlReader xmlFile = XmlReader.Create("NewPerson.xml", new XmlReaderSettings()))
                {
                    ds.ReadXml(xmlFile);
                }

                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            int id_person = Convert.ToInt32(ds.Tables[0].Rows[i]["id_person"]);
                            string name = ds.Tables[0].Rows[i]["name"].ToString();
                            string position = ds.Tables[0].Rows[i]["position"].ToString();
                            int code = Convert.ToInt32(ds.Tables[0].Rows[i]["code"]);

                            string sql = "INSERT OR REPLACE INTO person (id_person, name, position, code) VALUES (@id, @name, @pos, @code)";
                            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@id", id_person);
                                command.Parameters.AddWithValue("@name", name);
                                command.Parameters.AddWithValue("@pos", position);
                                command.Parameters.AddWithValue("@code", code);
                                command.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Готово !!! Записи додано !!! Оновіть таблицю !");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при додаванні в БД: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Файл NewPerson.xml відсутній!!! Спершу створіть його!");
            }
        }

        // Кнопка 7
        private void button7_Click(object sender, EventArgs e)
        {
            LoadDataFromDb();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}
