using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransJakartaLocator.Model
{
    class Shelter
    {
        public string Name { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }

        public double DoubleLat
        {
            get { return (double)Latitude / 1E6; }
        }

        public double DoubleLon
        {
            get { return (double)Longitude / 1E6; }
        }

        public Shelter()
        {
            this.Name = "";
            this.Latitude = 0;
            this.Longitude = 0;
        }
    }
}
