using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterTemplate.Models
{
    public class KalmanFilterData
    {
        public double BaseQ { get; set; } = 0.001;
        public double SmoothingFactor { get; set; } = 0.9;

    }
}
