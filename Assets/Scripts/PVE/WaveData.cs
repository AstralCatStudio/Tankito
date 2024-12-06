using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    [CreateAssetMenu(fileName = "NewWaveData", menuName ="Wave/WaveData", order = 3)]
    public class WaveData : ScriptableObject
    {
        public int BodyGuards;
        public int Healers;
        public int Kamikazes;
        public int Necromancers;
        public int Attackers;
        public int Miners;
    }
}

