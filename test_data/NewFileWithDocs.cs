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

        /// <summary>
        /// Updates an existing record in the repository with the provided entity data.
        /// </summary>
        /// <param name="data">The entity instance containing updated values.</param>
        /// <returns>void</returns>
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

        /// <summary>
        /// Calculates the minimum hozam.
        /// </summary>
        /// <param name="minimumHozam">The initial minimum hozam.</param>
        /// <returns>The final calculated minimum hozam as a double.</returns>
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

        /// <summary>
        /// Calculates the maximum allowable hozam.
        /// </summary>
        /// <param name="minimumHozam">The minimum allowed hozam.</param>
        /// <param name="maximumHozam">The maximum allowed hozam (0.99 by default).</param>
        /// <returns>The calculated maximum hozam as a double.</returns>
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

        /// <summary>
        /// Updates an existing record in the repository with the provided entity data.
        /// </summary>
        /// <param name="data">The entity instance containing updated values.</param>
        /// <returns>void</returns>
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

        /// <summary>
        /// Calculates the total price including tax for a list of items.
        /// </summary>
        /// <param name="prices">A list of integer prices.</param>
        /// <param name="taxRate">The tax rate to apply (e.g., 1.27 for 27%).</param>
        /// <returns>The final calculated total as an integer.</returns>
        public override int Hozam(int elteltEvek)
        {
            int hozam = Convert.ToInt32(Kezdotoke * Math.Pow(1 + (Kamatlab / 100), elteltEvek));

            for (int i = 0; i < elteltEvek; i++)
            {
                hozam = Convert.ToInt32(Kezdotoke * (1 + (0.01 + rnd.NextDouble() * (0.5 - 0.01))));
            }

            return hozam;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string containing the object's properties.</returns>
        public override string ToString()
        {
            return base.ToString() + $" részvényalap: {ReszvenyAlap}, minimum hozam: {MinimumHozam}, maximum hozam: {MaximumHozam}, kockázat: {Kockazat}";
        }

    }
}
