using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementEngine;

namespace VagabondRL
{
    public class Timer
    {
        float TimeSoFar;
        float TicRate;
        public bool TicThisUpdate;

        public Timer(float ticRate)
        {
            TimeSoFar = 0.0f;
            TicRate = ticRate;
        }

        public void Update(GameTimer gameTimer)
        {
            TicThisUpdate = false;
            TimeSoFar += gameTimer.DeltaS;
            if (TimeSoFar >= TicRate)
            {
                TicThisUpdate = true;
                TimeSoFar -= gameTimer.DeltaS;
            }
        }
    }
}
