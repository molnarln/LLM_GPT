using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minta_ZH1_Bankbetet
{
    internal class BefektetesiJegy : Bankbetet
    {
        private static Random rnd = new Random();
        private string reszvenyAlap;

        public string ReszvenyAlap
        {
            get { return reszvenyAlap; }
            private set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("ReszvenyAlap nem lehet null!");
                }
                reszvenyAlap = value;
            }
        }
        private double minimumHozam;

        public double MinimumHozam
        {
            get { return minimumHozam; }
            set
            {
                if (value < 0.01 || value > 0.5)
                {
                    throw new Exception("Nem megfelelő minimum hozam érték");
                }
                minimumHozam = value;
            }
        }

        private double maximumHozam;

        public double MaximumHozam
        {
            get { return maximumHozam; }
            set
            {
                if (value < MinimumHozam || value > 0.99)
                {
                    throw new Exception("Nem megfelelő maximum hozam érték");
                }
                maximumHozam = value;
            }
        }

        public Kockazat Kockazat { get; set; }

        public BefektetesiJegy(string azonosito, int kezdotoke, double kamatlab, int lekotesIdeje, string reszvenyAlap, double minimumHozam, double maximumHozam, Kockazat kockazat) : base(azonosito, kezdotoke, 0.01, lekotesIdeje)
        {
            ReszvenyAlap = reszvenyAlap;
            MinimumHozam = minimumHozam;
            MaximumHozam = maximumHozam;
            Kockazat = kockazat;
        }

        public BefektetesiJegy(string azonosito, int kezdotoke, double kamatlab, int lekotesIdeje, string reszvenyAlap, double minimumHozam, double maximumHozam) : this(azonosito, kezdotoke, kamatlab, lekotesIdeje, reszvenyAlap, minimumHozam, maximumHozam, Kockazat.alacsony)
        {

        }

        public override int Hozam(int elteltEvek)
        {
            int hozam = Convert.ToInt32(Kezdotoke * Math.Pow(1 + (Kamatlab / 100), elteltEvek));

            for (int i = 0; i < elteltEvek; i++)
            {
                hozam = Convert.ToInt32(Kezdotoke * (1 + (0.01 + rnd.NextDouble() * (0.5 - 0.01))));
            }

            return hozam;
        }

        public override string ToString()
        {
            return base.ToString() + $" részvényalap: {ReszvenyAlap}, minimum hozam: {MinimumHozam}, maximum hozam: {MaximumHozam}, kockázat: {Kockazat}";
        }

    }
}
