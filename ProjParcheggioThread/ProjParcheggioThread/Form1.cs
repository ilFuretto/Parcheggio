using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ProjParcheggioThread
{
    public partial class Form1 : Form
    {
        const int maxPosti = 5;  // Numero massimo di posti disponibili nel parcheggio
        Semaphore parcheggio;     // Per gestire il numero di posti disponibili
        Semaphore uscita;         // Per gestire l'uscita una macchina alla volta
        Random random;
        int nMacchine;
        List<Thread> macchine;    // Lista per memorizzare i thread delle macchine

        public Form1()
        {
            InitializeComponent();
            parcheggio = new Semaphore(maxPosti, maxPosti);  // Impostiamo il numero massimo di posti
            uscita = new Semaphore(1, 1);  // Solo una macchina può uscire alla volta
            random = new Random();
            macchine = new List<Thread>();  // Lista di macchine
            label1.Text = "Numero di macchine: " + macchine.Count;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (macchine.Count >= maxPosti)
            {
                MessageBox.Show("Non è possibile entrare. Il parcheggio è pieno.");
                return;
            }

            Thread macchina = new Thread(new ParameterizedThreadStart(MacchinaEntra));

            macchine.Add(macchina);

            nMacchine = macchine.Count;

            macchina.Start(nMacchine);
        }

        private void MacchinaEntra(object macchinaId)
        {
            int id = (int)macchinaId;
            try
            {

                parcheggio.WaitOne(); 

                AggiornaLabelMacchine();

                int tempoPermanenza = random.Next(500, 2000);
                Thread.Sleep(tempoPermanenza);

                EsciDalParcheggio(id);
            }
            finally
            {
                parcheggio.Release();
            }
        }

        private void EsciDalParcheggio(int id)
        {
            bool uscitaCompletata = false;

            while (!uscitaCompletata)
            {
                try
                {
                    uscita.WaitOne();


                    int tempoUscita = random.Next(1000, 3000); 
                    Thread.Sleep(tempoUscita);
                    macchine.RemoveAll(m => m.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
                    AggiornaLabelMacchine();
                    MessageBox.Show("Macchina " + id + " ha lasciato il parcheggio.");
                    uscitaCompletata = true;
                }
                catch (ThreadInterruptedException)
                {
                    int attesa = random.Next(2000, 4000);  
                    Thread.Sleep(attesa);
                }
                finally
                {
                    uscita.Release();
                }
            }

        }

        private void AggiornaLabelMacchine()
        {
            label1.Invoke(new MethodInvoker(delegate
            {
                label1.Text = "Numero di macchine: " + macchine.Count;
            }));
        }
    }
}
