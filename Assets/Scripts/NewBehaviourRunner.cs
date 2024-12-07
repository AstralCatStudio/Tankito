/*using System;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityToolkit;
using BehaviourAPI.StateMachines;
using BehaviourAPI.UtilitySystems;

using Tankito.SinglePlayer;
using BehaviourAPI.StateMachines.StackFSMs;
public class NewBehaviourRunner : BehaviourRunner
{
	[SerializeField] private AttackerBehaviour m_AttackerBehaviour;
	
	protected override void Init()
	{
		m_AttackerBehaviour = GetComponent<AttackerBehaviour>();
		
		base.Init();
	}
	
	protected override BehaviourGraph CreateGraph()
	{
		StackFSM AttackerBehaviour = new StackFSM();
		UtilitySystem CombatUS = new UtilitySystem(1.3f);
		
		FunctionalAction Idle_action = new FunctionalAction();
		Idle_action.onUpdated = m_AttackerBehaviour.IdleState;
		State Idle = AttackerBehaviour.CreateState(Idle_action);
		
		FunctionalAction ChaseBodyGuard_action = new FunctionalAction();
		ChaseBodyGuard_action.onUpdated = m_AttackerBehaviour.ChaseState;
		State ChaseBodyGuard = AttackerBehaviour.CreateState(ChaseBodyGuard_action);
		
		FunctionalAction Cover_action = new FunctionalAction();
		Cover_action.onUpdated = m_AttackerBehaviour.CoverState;
		State Cover = AttackerBehaviour.CreateState(Cover_action);
		
		FunctionalAction Aim_action = new FunctionalAction();
		Aim_action.onUpdated = m_AttackerBehaviour.AimState;
		State Aim = AttackerBehaviour.CreateState(Aim_action);
		
		FunctionalAction Shoot_action = new FunctionalAction();
		Shoot_action.onUpdated = m_AttackerBehaviour.ShootState;
		State Shoot = AttackerBehaviour.CreateState(Shoot_action);
		
		ConditionPerception ChaseToIdle_perception = new ConditionPerception();
		ChaseToIdle_perception.onCheck = m_AttackerBehaviour.CheckChaseToIdle;
		StateTransition ChaseToIdle = AttackerBehaviour.CreateTransition(ChaseBodyGuard, Idle, ChaseToIdle_perception);
		
		ConditionPerception IdleToChase_perception = new ConditionPerception();
		IdleToChase_perception.onCheck = m_AttackerBehaviour.CheckIdleToChase;
		SimpleAction IdleToChase_action = new SimpleAction();
		IdleToChase_action.onStarted = m_AttackerBehaviour.ActionIdleToChase;
		StateTransition IdleToChase = AttackerBehaviour.CreateTransition(Idle, ChaseBodyGuard, IdleToChase_perception, IdleToChase_action);
		
		ConditionPerception CoverToChase_perception = new ConditionPerception();
		CoverToChase_perception.onCheck = m_AttackerBehaviour.CheckCoverToChase;
		StateTransition CoverToChase = AttackerBehaviour.CreateTransition(Cover, ChaseBodyGuard, CoverToChase_perception);
		
		ConditionPerception ChaseToCover_perception = new ConditionPerception();
		ChaseToCover_perception.onCheck = m_AttackerBehaviour.CheckChaseToCover;
		StateTransition ChaseToCover = AttackerBehaviour.CreateTransition(ChaseBodyGuard, Cover, ChaseToCover_perception);
		
		ConditionPerception AimToShoot_perception = new ConditionPerception();
		AimToShoot_perception.onCheck = m_AttackerBehaviour.CheckAimToShoot;
		SimpleAction AimToShoot_action = new SimpleAction();
		AimToShoot_action.onStarted = m_AttackerBehaviour.ActionAimToShoot;
		StateTransition AimToShoot = AttackerBehaviour.CreateTransition(Aim, Shoot, AimToShoot_perception, AimToShoot_action);
		
		ConditionPerception POPAim_perception = new ConditionPerception();
		POPAim_perception.onCheck = m_AttackerBehaviour.CheckAimPOP;
		PopTransition POPAim = AttackerBehaviour.CreatePopTransition(Aim, POPAim_perception);
		
		SubsystemAction AttackUSystem_action = new SubsystemAction(CombatUS);
		State AttackUSystem = AttackerBehaviour.CreateState(AttackUSystem_action);
		
		ConditionPerception USToCover_perception = new ConditionPerception();
		USToCover_perception.onCheck = m_AttackerBehaviour.CheckUSToCover;
		StateTransition USToCover = AttackerBehaviour.CreateTransition(AttackUSystem, Cover, USToCover_perception);
		
		ConditionPerception CoverToUS_perception = new ConditionPerception();
		CoverToUS_perception.onCheck = m_AttackerBehaviour.CheckChaseAndCoverToUS;
		StateTransition CoverToUS = AttackerBehaviour.CreateTransition(Cover, AttackUSystem, CoverToUS_perception);
		
		ConditionPerception ChaseToUS_perception = new ConditionPerception();
		ChaseToUS_perception.onCheck = m_AttackerBehaviour.CheckChaseAndCoverToUS;
		SimpleAction ChaseToUS_action = new SimpleAction();
		ChaseToUS_action.onStarted = m_AttackerBehaviour.ActionChaseToAim;
		StateTransition ChaseToUS = AttackerBehaviour.CreateTransition(ChaseBodyGuard, AttackUSystem, ChaseToUS_perception, ChaseToUS_action);
		
		ConditionPerception USToChase_perception = new ConditionPerception();
		USToChase_perception.onCheck = m_AttackerBehaviour.CheckUSToChase;
		StateTransition USToChase = AttackerBehaviour.CreateTransition(AttackUSystem, ChaseBodyGuard, USToChase_perception);
		
		ConditionPerception IdleToUS_perception = new ConditionPerception();
		IdleToUS_perception.onCheck = m_AttackerBehaviour.CheckIdleToUS;
		SimpleAction IdleToUS_action = new SimpleAction();
		IdleToUS_action.onStarted = m_AttackerBehaviour.ActionIdleToChase;
		StateTransition IdleToUS = AttackerBehaviour.CreateTransition(Idle, AttackUSystem, IdleToUS_perception, IdleToUS_action);
		
		ConditionPerception USToIdle_perception = new ConditionPerception();
		USToIdle_perception.onCheck = m_AttackerBehaviour.CheckUSToIdle;
		StateTransition USToIdle = AttackerBehaviour.CreateTransition(AttackUSystem, Idle, USToIdle_perception);
		
		ConditionPerception USToAim_perception = new ConditionPerception();
		USToAim_perception.onCheck = m_AttackerBehaviour.CheckChaseToAim;
		SimpleAction USToAim_action = new SimpleAction();
		USToAim_action.onStarted = m_AttackerBehaviour.ActionAttackerUSToAim;
		PushTransition USToAim = AttackerBehaviour.CreatePushTransition(AttackUSystem, Aim, USToAim_perception, USToAim_action);
		
		ConditionPerception POPShoot_perception = new ConditionPerception();
		POPShoot_perception.onCheck = m_AttackerBehaviour.CheckShootPOP;
		SimpleAction POPShoot_action = new SimpleAction();
		POPShoot_action.onStarted = m_AttackerBehaviour.ActionShootPOP;
		PopTransition POPShoot = AttackerBehaviour.CreatePopTransition(Shoot, POPShoot_perception, POPShoot_action);
		
		VariableFactor NAFullAggro = CombatUS.CreateVariable(m_AttackerBehaviour.NAFullAggro, 0f, 5f);
		
		VariableFactor HP_Player = CombatUS.CreateVariable(m_AttackerBehaviour.HP_Player, 0f, 1f);
		
		VariableFactor HP_Attacker = CombatUS.CreateVariable(m_AttackerBehaviour.HP_Attacker, 0f, 1f);
		
		VariableFactor Healer = CombatUS.CreateVariable(m_AttackerBehaviour.Healer, 0f, 1f);
		
		CustomCurveFactor AggroNA = CombatUS.CreateCurve<CustomCurveFactor>(NAFullAggro);
		
		CustomCurveFactor AggroHPA = CombatUS.CreateCurve<CustomCurveFactor>(HP_Attacker);
		
		CustomCurveFactor AggroHPP = CombatUS.CreateCurve<CustomCurveFactor>(HP_Player);
		
		WeightedFusionFactor UGoAggro = CombatUS.CreateFusion<WeightedFusionFactor>(AggroNA, AggroHPA, AggroHPP);
		
		LinearCurveFactor _1_AggroHPA = CombatUS.CreateCurve<LinearCurveFactor>(AggroHPA);
		
		LinearCurveFactor _1_AggroHPP = CombatUS.CreateCurve<LinearCurveFactor>(AggroHPP);
		
		WeightedFusionFactor UGoIdealDis = CombatUS.CreateFusion<WeightedFusionFactor>(AggroHPA, AggroHPP);
		
		WeightedFusionFactor UGoDef = CombatUS.CreateFusion<WeightedFusionFactor>(_1_AggroHPP, _1_AggroHPA);
		
		FunctionalAction GoAggro_action = new FunctionalAction();
		GoAggro_action.onUpdated = m_AttackerBehaviour.GoAggro;
		UtilityAction GoAggro = CombatUS.CreateAction(UGoAggro, GoAggro_action);
		
		FunctionalAction GoIdealDis_action = new FunctionalAction();
		GoIdealDis_action.onUpdated = m_AttackerBehaviour.GoIdealDis;
		UtilityAction GoIdealDis = CombatUS.CreateAction(UGoIdealDis, GoIdealDis_action);
		
		FunctionalAction GoDef_action = new FunctionalAction();
		GoDef_action.onUpdated = m_AttackerBehaviour.GoDef;
		UtilityAction GoDef = CombatUS.CreateAction(UGoDef, GoDef_action);
		
		MinFusionFactor UGoHeal = CombatUS.CreateFusion<MinFusionFactor>(_1_AggroHPA, Healer);
		
		FunctionalAction GoHeal_action = new FunctionalAction();
		GoHeal_action.onUpdated = m_AttackerBehaviour.GoHeal;
		UtilityAction GoHeal = CombatUS.CreateAction(UGoHeal, GoHeal_action);
		
		return AttackerBehaviour;
	}
}
*/