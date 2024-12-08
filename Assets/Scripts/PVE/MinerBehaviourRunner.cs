using System;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityToolkit;
using BehaviourAPI.StateMachines;
using BehaviourAPI.UtilitySystems;

namespace Tankito.SinglePlayer
{
	public class MinerBehaviourRunner : BehaviourRunner
	{
		[SerializeField] private MinerBehaviour m_MinerBehaviour;
		
		protected override void Init()
		{
			m_MinerBehaviour = GetComponent<MinerBehaviour>();
			
			base.Init();
		}
		
		protected override BehaviourGraph CreateGraph()
		{
			FSM MinerBehaviour = new FSM();
			UtilitySystem MinerUS_1 = new UtilitySystem(1.3f);
			
			FunctionalAction Idle_action = new FunctionalAction();
			Idle_action.onUpdated = m_MinerBehaviour.IdleState;
			State Idle = MinerBehaviour.CreateState(Idle_action);
			
			FunctionalAction Dig_action = new FunctionalAction();
			Dig_action.onStarted = m_MinerBehaviour.InitDigState;
			Dig_action.onUpdated = m_MinerBehaviour.DigState;
			State Dig = MinerBehaviour.CreateState(Dig_action);
			
			FunctionalAction MinerUS_action = new FunctionalAction();
			State MinerUS = MinerBehaviour.CreateState(MinerUS_action);
			
			ConditionPerception IdleToUS_perception = new ConditionPerception();
			IdleToUS_perception.onCheck = m_MinerBehaviour.CheckIdleToMinerUS;
			FunctionalAction IdleToUS_action = new FunctionalAction();
			IdleToUS_action.onUpdated = () => Status.Running;
			StateTransition IdleToUS = MinerBehaviour.CreateTransition(Idle, MinerUS, IdleToUS_perception, IdleToUS_action);
			
			ConditionPerception USToIdle_perception = new ConditionPerception();
			USToIdle_perception.onCheck = m_MinerBehaviour.CheckMinerUSToIdle;
			FunctionalAction USToIdle_action = new FunctionalAction();
			USToIdle_action.onUpdated = m_MinerBehaviour.ActionMinerUSExit;
			StateTransition USToIdle = MinerBehaviour.CreateTransition(MinerUS, Idle, USToIdle_perception, USToIdle_action);
			
			ConditionPerception USToDig_perception = new ConditionPerception();
			USToDig_perception.onCheck = m_MinerBehaviour.CheckMinerUSToDig;
			FunctionalAction USToDig_action = new FunctionalAction();
			USToDig_action.onUpdated = m_MinerBehaviour.ActionMinerUSExit;
			StateTransition USToDig = MinerBehaviour.CreateTransition(MinerUS, Dig, USToDig_perception, USToDig_action);
			
			ConditionPerception DigToUS_perception = new ConditionPerception();
			DigToUS_perception.onCheck = m_MinerBehaviour.CheckDigToMinerUS;
			StateTransition DigToUS = MinerBehaviour.CreateTransition(Dig, MinerUS, DigToUS_perception);
			
			FunctionalAction PutMine_action = new FunctionalAction();
			PutMine_action.onUpdated = m_MinerBehaviour.PutMineState;
			State PutMine = MinerBehaviour.CreateState(PutMine_action);
			
			ConditionPerception USToMine_perception = new ConditionPerception();
			USToMine_perception.onCheck = m_MinerBehaviour.CheckMinerUSToPutMine;
			FunctionalAction USToMine_action = new FunctionalAction();
			USToMine_action.onUpdated = m_MinerBehaviour.ActionMinerUSExit;
			StateTransition USToMine = MinerBehaviour.CreateTransition(MinerUS, PutMine, USToMine_perception, USToMine_action);
			
			ConditionPerception MineToUS_perception = new ConditionPerception();
			MineToUS_perception.onCheck = m_MinerBehaviour.CheckPutMineToMinerUS;
			StateTransition MineToUS = MinerBehaviour.CreateTransition(PutMine, MinerUS, MineToUS_perception);
			
			VariableFactor HPPlayer = MinerUS_1.CreateVariable(m_MinerBehaviour.HP_Player, 0f, 1f);
			
			VariableFactor Distance = MinerUS_1.CreateVariable(m_MinerBehaviour.Distance, 0f, 1f);
			
			VariableFactor Cover = MinerUS_1.CreateVariable(m_MinerBehaviour.Cover, 0f, 1f);
			
			VariableFactor CanDig = MinerUS_1.CreateVariable(m_MinerBehaviour.CanDig, 0f, 1f);
			
			VariableFactor CanPutMIne = MinerUS_1.CreateVariable(m_MinerBehaviour.CanPutMine, 0f, 1f);
			
			VariableFactor NAllies = MinerUS_1.CreateVariable(m_MinerBehaviour.NAllies, 0f, 1f);
			
			VariableFactor NMines = MinerUS_1.CreateVariable(m_MinerBehaviour.NMines, 0f, 1f);
			
			CustomCurveFactor AggroHPP = MinerUS_1.CreateCurve<CustomCurveFactor>(HPPlayer);
			
			CustomCurveFactor AggroNA = MinerUS_1.CreateCurve<CustomCurveFactor>(NAllies);
			
			ExponentialCurveFactor unnamed = MinerUS_1.CreateCurve<ExponentialCurveFactor>(Cover);
			
			LinearCurveFactor _1_Distance = MinerUS_1.CreateCurve<LinearCurveFactor>(Distance);
			
			LinearCurveFactor _1_NMines = MinerUS_1.CreateCurve<LinearCurveFactor>(NMines);
			
			WeightedFusionFactor UGoAggro = MinerUS_1.CreateFusion<WeightedFusionFactor>(AggroHPP, _1_Distance, _1_NMines);
			
			FunctionalAction GoAggro_action = new FunctionalAction();
			GoAggro_action.onUpdated = () => Status.Running;
			UtilityAction GoAggro = MinerUS_1.CreateAction(UGoAggro, GoAggro_action);
			
			LinearCurveFactor _1_AggroHPP = MinerUS_1.CreateCurve<LinearCurveFactor>(AggroHPP);
			
			WeightedFusionFactor UGoDef = MinerUS_1.CreateFusion<WeightedFusionFactor>(_1_AggroHPP, Distance, _1_NMines);
			
			FunctionalAction GoDef_action = new FunctionalAction();
			GoDef_action.onUpdated = () => Status.Running;
			UtilityAction GoDef = MinerUS_1.CreateAction(UGoDef, GoDef_action);
			
			WeightedFusionFactor WeightDig = MinerUS_1.CreateFusion<WeightedFusionFactor>(AggroNA, _1_Distance, _1_AggroHPP);
			
			MinFusionFactor UDig = MinerUS_1.CreateFusion<MinFusionFactor>(WeightDig, CanDig);
			
			FunctionalAction Dig_1_action = new FunctionalAction();
			Dig_1_action.onUpdated = m_MinerBehaviour.Dig;
			UtilityAction Dig_1 = MinerUS_1.CreateAction(UDig, Dig_1_action);
			
			WeightedFusionFactor WeightPutMine = MinerUS_1.CreateFusion<WeightedFusionFactor>(AggroHPP, Distance, unnamed, AggroNA, NMines);
			
			MinFusionFactor UPutMine = MinerUS_1.CreateFusion<MinFusionFactor>(WeightPutMine, CanPutMIne);
			
			FunctionalAction PutMine_1_action = new FunctionalAction();
			PutMine_1_action.onUpdated = m_MinerBehaviour.PutMine;
			UtilityAction PutMine_1 = MinerUS_1.CreateAction(UPutMine, PutMine_1_action);
			
			return MinerBehaviour;
		}
	}
}
