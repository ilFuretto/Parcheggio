using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace ProjParcheggioThread
{
    public partial class Form1 : Form
    {
        const int maxPosti = 8;
        Semaphore parcheggio;
        Semaphore uscita;
        Random random;
        int nMacchine;
        List<Thread> macchine;
        List<PictureBox> immagini;
        List<bool> postiOccupati;
        SoundPlayer audioIngresso;
        SoundPlayer audioUscita;

        public Form1()
        {
            InitializeComponent();
            parcheggio = new Semaphore(maxPosti, maxPosti);
            uscita = new Semaphore(1, 1);
            random = new Random();
            macchine = new List<Thread>();
            immagini = new List<PictureBox>()
            {
                pictureBox1, pictureBox2, pictureBox3, pictureBox4,
                pictureBox5, pictureBox6, pictureBox7, pictureBox8
            };
            postiOccupati = new List<bool>(new bool[maxPosti]);
            label1.Text = "Numero di macchine: " + macchine.Count;
            audioIngresso = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", "audioEntrata.wav"));
            audioUscita = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", "audioUscitaParcheggio.wav"));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (macchine.Count >= maxPosti)
            {
                MessageBox.Show("Non è possibile entrare. Il parcheggio è pieno.");
                return;
            }
            int postoLibero = TrovaPostoLiberoCasuale();

            if (postoLibero == -1)
            {
                MessageBox.Show("Non ci sono posti liberi.");
                return;
            }

            Thread macchina = new Thread(MacchinaEntra);
            macchine.Add(macchina);

            postiOccupati[postoLibero] = true;
            nMacchine = macchine.Count;

            immagini[postoLibero].Visible = true;

            macchina.Start(postoLibero); 
        }

        private int TrovaPostoLiberoCasuale()
        {
            List<int> postiLibero = new List<int>();

            for (int i = 0; i < maxPosti; i++)
            {
                if (!postiOccupati[i])
                {
                    postiLibero.Add(i);
                }
            }

            if (postiLibero.Count > 0)
            {
                return postiLibero[random.Next(postiLibero.Count)];
            }

            return -1;
        }

        private void MacchinaEntra(object postoId)
        {
            int posto = (int)postoId;
            try
            {
                audioIngresso.Play();
                parcheggio.WaitOne();

                AggiornaLabelMacchine();

                int tempoPermanenza = random.Next(2500, 4500);
                Thread.Sleep(tempoPermanenza);

                EsciDalParcheggio(posto);
            }
            finally
            {
                parcheggio.Release();
            }
        }

        private void EsciDalParcheggio(int posto)
        {
            bool uscitaCompletata = false;

            while (!uscitaCompletata)
            {
                try
                {
                    uscita.WaitOne();

                    int tempoUscita = random.Next(2000, 3000);
                    Thread.Sleep(tempoUscita);

                    macchine.RemoveAll(m => m.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
                    AggiornaLabelMacchine();
                    audioUscita.Play();
                    MessageBox.Show("Macchina al posto " + (posto + 1) + " sta lasciando il parcheggio.");
                    uscitaCompletata = true;
                }
                catch (ThreadInterruptedException)
                {
                    int attesa = random.Next(1500, 4000);
                    Thread.Sleep(attesa);
                }
                finally
                {
                    uscita.Release();
                    AggiornaImmagineMacchine(posto);
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

        private void AggiornaImmagineMacchine(int posto)
        {
            immagini[posto].Invoke(new MethodInvoker(delegate
            {
                immagini[posto].Visible = false;
            }));
            postiOccupati[posto] = false;
        }
    }
}
