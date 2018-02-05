﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Upgrade;
using Abilities;
using System;
using Ship;
using System.Linq;
using SubPhases;
using Bombs;

namespace UpgradesList
{
    public class Genius : GenericUpgrade
    {
        public Genius() : base()
        {
            Type = UpgradeType.SalvagedAstromech;
            Name = "\"Genius\"";
            Cost = 0;

            isUnique = true;

            UpgradeAbilities.Add(new GeniusAbility());
        }
    }
}

namespace Abilities
{
    public class GeniusAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnMovementFinish += CheckGeniusAbility;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnMovementFinish -= CheckGeniusAbility;
        }

        private void CheckGeniusAbility(GenericShip ship)
        {
            if (!HostShip.IsBumped && !HostShip.IsBombAlreadyDropped && HasTimedBombs())
            {
                RegisterAbilityTrigger(TriggerTypes.OnMovementActivation, AskUseGeniusAbility);
            }
        }

        private List<GenericUpgrade> GetTimedBombsInstalled()
        {
            return HostShip.UpgradeBar.GetUpgradesOnlyFaceup().Where(n => n.GetType().BaseType == typeof(GenericTimedBomb)).ToList();
        }

        private bool HasTimedBombs()
        {
            int timedBombsInstalledCount = GetTimedBombsInstalled().Count;
            return timedBombsInstalledCount > 0;
        }

        private void AskUseGeniusAbility(object sender, EventArgs e)
        {
            AskToUseAbility(NeverUseByDefault, UseGeniusAbility);
        }

        private void UseGeniusAbility(object sender, EventArgs e)
        {
            List<GenericUpgrade> timedBombsInstalled = GetTimedBombsInstalled();
            DecisionSubPhase.ConfirmDecisionNoCallback();

            if (timedBombsInstalled.Count == 1)
            {
                BombsManager.CurrentBomb = timedBombsInstalled[0] as GenericBomb;
                StartDropBombSubphase();
            }
            else
            {
                AskToSelectTimedBomb(StartDropBombSubphase);
            }
        }

        private void AskToSelectTimedBomb(Action callback)
        {
            GeniusBombDecisionSubPhase selectBombToDrop = (GeniusBombDecisionSubPhase)Phases.StartTemporarySubPhaseNew(
                "Select bomb to drop",
                typeof(GeniusBombDecisionSubPhase),
                callback
            );

            foreach (var timedBombInstalled in GetTimedBombsInstalled())
            {
                selectBombToDrop.AddDecision(
                    timedBombInstalled.Name,
                    delegate { SelectBomb(timedBombInstalled); }
                );
            }

            selectBombToDrop.InfoText = "Select bomb to drop";

            selectBombToDrop.DefaultDecision = GetTimedBombsInstalled().First().Name;

            selectBombToDrop.RequiredPlayer = HostShip.Owner.PlayerNo;

            selectBombToDrop.Start();
        }

        private void SelectBomb(GenericUpgrade timedBombUpgrade)
        {
            BombsManager.CurrentBomb = timedBombUpgrade as GenericTimedBomb;
            DecisionSubPhase.ConfirmDecision();
        }

        private class GeniusBombDecisionSubPhase : DecisionSubPhase { }

        private void DiscardBombAndDrop() { 
}

        private void StartDropBombSubphase()
        {
            Phases.StartTemporarySubPhaseOld(
                "Bomb drop planning",
                typeof(BombDropPlanningSubPhase),
                CheckThatBombIsDiscarded
            );
        }

        private void CheckThatBombIsDiscarded()
        {
            if (BombsManager.CurrentBomb == null || BombsManager.CurrentBomb.IsDiscardedAfterDropped)
            {
                Triggers.FinishTrigger();
            }
            else
            {
                BombsManager.CurrentBomb.TryDiscard(Triggers.FinishTrigger);
            }
        }
    }
}