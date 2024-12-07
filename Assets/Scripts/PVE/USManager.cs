using System.Collections;
using System.Collections.Generic;
using Tankito.SinglePlayer;
using UnityEngine;

public class USManager : Singleton<USManager>
{
   List<AttackerBehaviour> attackersFullAggro = new List<AttackerBehaviour>();

    public List<AttackerBehaviour> AttackerFullAggro { get => attackersFullAggro; }
}
