using System;
using System.Data;
using System.Data.Odbc;
using System.Windows.Forms;

namespace _1213_データベースのデータ操作
{
    //部分クラス
    public partial class Form1 : Form
    {
        int int_i;

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
        {
            DataGridViewSelectedRowCollection src = dataGridView1.SelectedRows;

            //行選択されない場合は、削除しない
            //src.Count:選択された行数
            for (int i = src.Count - 1; i >= 0; i--)
            {
                dataGridView1.Rows.RemoveAt(src[i].Index);

                //int_i = (int)dataGridView1.Rows[src[i].Index].Cells[0].Value;
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

                        // Update文を編集、実行
                        // 列ループ、すべての値を再設定、値をシングルクオーテーションを付け、条件が1列目の値を一致すること！
                        string sql1 = "UPDATE " + textBox1.Text + " SET ";

                        for (int k = 0; k < dataGridView1.Columns.Count; k++)
                        {
                            sql1 = sql1 + dataGridView1.Columns[k].Name + " = '" + dataGridView1.Rows[i-1].Cells[k].Value + "',";
                        }

                        sql1 = sql1.Substring(0, sql1.Length - 1);

                        sql1 = sql1 + " Where " + dataGridView1.Columns[0].Name + " = '" + dataGridView1.Rows[i-1].Cells[0].Value + "'";

                        ExecuteNonQuery(sql1);

                        break;
                    
                    case DataRowState.Deleted:

                        //Delete文を編集、実行　・・・・未完了
                        string sql2 = "Delete from " + textBox1.Text +
                            " Where " + dataGridView1.Columns[0].Name + " = '" + dataGridView1.Rows[i-1].Cells[0].Value + "'";

                        ExecuteNonQuery(sql2);

                        break;
                    
                    case DataRowState.Added:

                        //InsertCommand文を編集、実行
                        string sql3 = "INSERT INTO " + textBox1.Text + " VALUES (";

                        for (int k = 0; k < dataGridView1.Columns.Count; k++)
                        {
                            sql3 = sql3 + "'" + dataGridView1.Rows[i-1].Cells[k].Value + "',";
                        }

                        sql3 = sql3.Substring(0, sql3.Length - 1) + ")";

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
