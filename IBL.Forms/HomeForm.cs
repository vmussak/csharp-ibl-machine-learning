﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using IBL.Core.Models;
using IBL.Core.Engine;

namespace IBL.Forms
{
    public partial class HomeForm : Form
    {
        public readonly ClassificadorIbl _ibl;
        public HomeForm()
        {
            _ibl = new ClassificadorIbl();
            InitializeComponent();
        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            lblImportar.Text = "";
            lblLeave.Text = "";
            openFileDialog1.Title = "Importar Excel";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Excel File|*.xlsx;*.xls";

            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var itens = LerExcel(openFileDialog1.FileName);

                _ibl.CarregarDados(itens);

                var classes = itens.GroupBy(x => x.Classe).Select(x => new
                {
                    Classe = x.Key,
                    Quantidade = x.Count()
                });

                lblImportar.Text = $"Importação OK - {itens.Count} exemplos\n";
                foreach(var item in classes)
                    lblImportar.Text += $"{item.Classe} -> {item.Quantidade} exemplos\n";
            }
            else
            {
                lblImportar.Text = "";
            }
        }

        private List<Item> LerExcel(string fileName)
        {
            var lstItens = new List<Item>();
            var xlApp = new Excel.Application();
            var xlWorkBook = xlApp.Workbooks.Open(fileName);

            var a = xlWorkBook.Worksheets;

            var xlWorkSheet = xlWorkBook.Worksheets[1];

            for (var iRow = 2; iRow <= xlWorkSheet.Rows.Count; iRow++)
            {
                if (xlWorkSheet.Cells[iRow, 1].value == null)
                    break;

                lstItens.Add(new Item
                {
                    X = new Atributo { Valor = xlWorkSheet.Cells[iRow, 1].value },
                    Y = new Atributo { Valor = xlWorkSheet.Cells[iRow, 2].value },
                    Classe = xlWorkSheet.Cells[iRow, 3].value
                });
            }

            xlWorkBook.Close();
            xlApp.Quit();

            return lstItens;
        }

        private void btnTreinar_Click(object sender, EventArgs e)
        {
            lblLeave.Text = "";
           var leaveOneOut = new LeaveOneOut();
           var (acertos, erros, total) = leaveOneOut.Fazer(_ibl._dataset.Itens);

            lblLeave.Text = "COMPLETO!\n";
            lblLeave.Text += $"Acertos: {acertos} ({(100 * acertos / total):N2}%)\n";
            lblLeave.Text += $"Erros: {erros} ({(100 * erros / total):N2}%)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var item = new Item
            {
                X = new Atributo { Valor = txtAttr1.Text },
                Y = new Atributo { Valor = txtAttr2.Text },
            };

            var novo = _ibl.Classificar(item);

            lblClassificacao.Text = novo.ItemOriginal.Classe;
        }

        private void btnFronteiras_Click(object sender, EventArgs e)
        {
            this.Hide();
            new FronteirasForm(_ibl, this).Show();
        }

        private void btnImportarTxt_Click(object sender, EventArgs e)
        {
            lblImportar.Text = "";
            lblLeave.Text = "";
            openFileDialog1.Title = "Importar TXT";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Text File|*.txt";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var itens = LerTxt(openFileDialog1.FileName);

                _ibl.CarregarDados(itens);

                var classes = itens.GroupBy(x => x.Classe).Select(x => new
                {
                    Classe = x.Key,
                    Quantidade = x.Count()
                });

                lblImportar.Text = $"Importação OK - {itens.Count} exemplos\n";
                foreach (var item in classes)
                    lblImportar.Text += $"{item.Classe} -> {item.Quantidade} exemplos\n";
            }
            else
            {
                lblImportar.Text = "";
            }
        }

        private List<Item> LerTxt(string fileName)
        {
            var lstItens = new List<Item>();
            string[] lines = System.IO.File.ReadAllLines(fileName);

            for (var iRow = 1; iRow < lines.Length; iRow++)
            {
                var linha = lines[iRow].Split(',');
                var incremento = linha.Length == 3 ? 0 : 1;
                lstItens.Add(new Item
                {
                    X = new Atributo { Valor = linha[0 + incremento].Replace('.', ',') },
                    Y = new Atributo { Valor = linha[1 + incremento].Replace('.', ',') },
                    Classe = linha[2 + incremento].Replace("\"", "")
                }); ;
            }

            return lstItens;
        }
    }
}
