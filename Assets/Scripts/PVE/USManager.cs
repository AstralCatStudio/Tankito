using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USManager : Singleton<USManager>
{
   int nAttackerFullAggro = 0;

    public int NAttackerFullAggro { get => nAttackerFullAggro; set => nAttackerFullAggro = value; }
}
