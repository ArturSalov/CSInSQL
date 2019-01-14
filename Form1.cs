using System;
using System.Collections.Generic;
using System.Drawing;
using System.Configuration;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WindowsFormsApp6
{

    public partial class Form1 : Form
    {
        List<CheckBox> listCB;
        Model model;

        public Form1() { InitializeComponent(); }

        private void Form1_Load(object sender, EventArgs e)
        {
            model = new Model();
            listCB = new List<CheckBox>();
            toDGV();
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int index = listCB.IndexOf(checkBox);
            if (!checkBox.Checked)
                model.AddIndex(index);
            else if (model.ContainsIndex(index)) model.RemoveIndex(index);
        }


        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Name != "Update")
                listCB.ForEach(x => x.Checked = true);
            toDGV();
        }

        void AddCheckBoxOnLB(Column[] Columns, List<CheckBox> list, Panel panel, EventHandler hendler)
        //Метод, що додає CheckBox в Panel з текстом назви стовбця
        {
            if (list.Count != 0) { list.Clear(); panel.Controls.Clear(); }
            if (Columns != null)
            {
                int hy = panel.Height / Columns.Length;

                for (int i = 0; i < Columns.Length; i++)
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Text = Columns[i].Name,
                        Location = new Point(10, i * hy),
                        Checked = true,
                    };
                    checkBox.CheckedChanged += hendler;
                    panel.Controls.Add(checkBox);
                    list.Add(checkBox);
                }
            }
        }

        void AddCheckBoxOnLB(Column[] Columns)
        //Метод, що додає CheckBox в Panel з текстом назви стовбця
        {
            if (listCB.Count != 0) { listCB.Clear(); panel2.Controls.Clear(); }
            if (Columns != null)
            {
                int hy = panel2.Height / Columns.Length;

                for (int i = 0; i < Columns.Length; i++)
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Text = Columns[i].Name,
                        Location = new Point(10, i * hy),
                        Checked = true,
                    };
                    checkBox.CheckedChanged += CheckBox_CheckedChanged;
                    panel2.Controls.Add(checkBox);
                    listCB.Add(checkBox);
                }
            }
        }


        private async void toDGV()
            //Запис даних з БД в таблицю
        {
            Task t = model.GetDataAsync();
            await Task.Run(() => t);
            Task.WaitAll(t);
            data Data = model.Data;

            if (Data.Columns != null)
            {
                if (panel2.Controls.Count == 0)
                    AddCheckBoxOnLB(Data.Columns);//, listCB, panel2, CheckBox_CheckedChanged);
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                foreach (Column c in Data.Columns)
                    dataGridView1.Columns.Add(c.Name, c.Name);
                foreach (object[] objArr in Data.Rows)
                    dataGridView1.Rows.Add(objArr);
            }
        }
    }
}
