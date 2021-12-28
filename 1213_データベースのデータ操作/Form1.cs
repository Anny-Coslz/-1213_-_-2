using System;
using System.Data;
using System.Data.Odbc;
using System.Windows.Forms;

namespace _1213_データベースのデータ操作
{
    //部分クラス
    public partial class Form1 : Form
    {
        //DataSet クラスの新しいインスタンスを初期化
        DataSet dtSet = new DataSet();

        OdbcDataAdapter dataAdapter;

        //接続文字列の作成
        OdbcConnection conn = new OdbcConnection(
            "Driver={PostgreSQL ANSI};database=postgres;Server=192.168.1.45;Port=5432;" +
            "Uid=postgres;Pwd=12345;CommandTimeOut=20;TimeOut=5");

        public Form1()
        {
            //コンポーネント初期化
            InitializeComponent();
        }

        //データの抽出
        private void btn_Select_Click_1(object sender, EventArgs e)
        {
            //データのみをクリアして、列情報は残ったまま
            //dtSet.Clear();

            if (dtSet.Tables.Count == 1)
            {
                //指定されたDataTableオブジェクトをコレクションから削除
                dtSet.Tables.Remove(dtSet.Tables[0]);
            }

            try
            {
                //クラスの新しいインスタンスを初期化
                dataAdapter = new OdbcDataAdapter(@"Select * from " + textBox1.Text + " order by 1", conn);

                //データソースに関連付けられたDataSetへの変更を調整するための単一テーブルコマンドを自動的に生成します。
                OdbcCommandBuilder builder = new OdbcCommandBuilder(dataAdapter);

                // SQLの実行した結果をdtSetの中に格納します。
                dataAdapter.Fill(dtSet);
                // DataTableをDataSourceとして使用、DataGridViewに表示
                dataGridView1.DataSource = dtSet.Tables[0];




                



                //---------------2021/12/27　チェックボックスを追加---------不要?------
                //DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();

                //column.HeaderText = "flag";
                //column.Name = "flag";

                //column.CellTemplate = new DataGridViewCheckBoxCell(false);
                //column.TrueValue = 1;
                //column.FalseValue = 0;

                //column.Width = 50;
                //column.IndeterminateValue = 1;
                //this.dataGridView1.Columns.AddRange(new DataGridViewColumn[] { column });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //データベースに反映
        private void button1_Click(object sender, EventArgs e)
        {
            dataAdapter.Update(dtSet);

            //出力メッセージ
            MessageBox.Show("データベースに反映済み");
        }

        //データ削除
        private void btn_Delete_Click_1(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection src = dataGridView1.SelectedRows;

            //行選択されない場合は、削除しない
            //src.Count:選択された行数
            for (int i = src.Count - 1; i >= 0; i--)
            {
                dataGridView1.Rows.RemoveAt(src[i].Index);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            int i = 1;

            //データテーブルをループ、行ステータスを判定
            foreach (DataRow row in dtSet.Tables[0].Rows)
            {
                switch (row.RowState)
                {
                    case DataRowState.Modified:
                        //UpdateCommand文を編集、実行
                        string sql1 = "Update " + textBox1.Text +" Set 社員番号 = 10011,名前= '佐藤11' Where ";

                        ExecuteNonQuery(sql1);

                        break;
                    
                    case DataRowState.Deleted:
                        //DeleteCommand文を編集、実行
                        string sql2 = "Delete from " + textBox1.Text + "";

                        ExecuteNonQuery(sql2);

                        break;
                    
                    case DataRowState.Added:
                        //InsertCommand文を編集、実行
                        string sql3 = "Insert into " + textBox1.Text + "(社員番号,名前,性別,血液型) values (10008,'佐藤8','男','B') ";

                        ExecuteNonQuery(sql3);

                        break;
                    case DataRowState.Unchanged:
                        MessageBox.Show(i + "行目のrow.RowState：" + row.RowState);
                        break;
                }

                i++;
            }
        }

        //共通メソッド：SQLを実行
        public void ExecuteNonQuery(string sql)
        {
            using (OdbcCommand command = new OdbcCommand())
            {
                //トランザクション開始
                conn.Open();
                OdbcTransaction transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    command.CommandText = sql;
                    command.Connection = conn;

                    command.Transaction = transaction;
                    command.ExecuteNonQuery();

                    //トランザクションをコミットします。
                    transaction.Commit();
                }
                catch (System.Exception e)
                {
                    //トランザクションをロールバック
                    transaction.Rollback();
                    MessageBox.Show(e.Message);
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}
