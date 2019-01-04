using System;
using ActionsList;
using Ship;
using Tokens;
using Upgrade;

namespace UpgradesList.SecondEdition
{
    public class CloakingDevice : GenericUpgrade
    {
        public CloakingDevice() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
               "Cloaking Device",
               UpgradeType.Illicit,
               cost: 5,
                abilityType: typeof(Abilities.SecondEdition.CloakingDeviceAbility),
                seImageNumber: 57,
                isLimited: true,
                charges: 2
           );
        }
    }
}


namespace Abilities.SecondEdition
{
    // small ship or medium ship only
    // 2(charge)
    // Action: Spend 1 [charge]  to perform a [cloak] action.
    // At the start of the Planning Phase, roll 1 attack die. On a 
    // [focus] result, decloak or discard your cloak token.
    public class CloakingDeviceAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            // TODO: Change to planning phase
            HostShip.OnGenerateActions += AddActions;
            RegisterAbilityTrigger(TriggerTypes.OnPlanningSubPhaseStart, this.CheckForDecloak);
        }

        public override void DeactivateAbility()
        {
            HostShip.OnGenerateActions -= AddActions;
        }

        private void AddActions(GenericShip ship)
        {
            if (HostUpgrade.State.Charges > 0)
            {
                ship.AddAvailableAction(new CloakAction()
                {
                    ImageUrl = HostUpgrade.ImageUrl,
                    HostShip = HostShip,
                    Source = HostUpgrade,
                    Name = "Cloaking Device: Cloak"
                });
            }
        }

        private void CheckForDecloak(object sender, EventArgs e)
        {
            if (this.HostShip.Tokens.HasToken(typeof(CloakToken)))
            {
                PerformDiceCheck("Checking for Cloaking Device decloak",
                                 DiceKind.Attack, 1, PerformDecloakCheck, Triggers.FinishTrigger);

            }
        }

        private void PerformDecloakCheck()
        {
            if (DiceCheckRoll.HasResult(DieSide.Focus))
            {
                Messages.ShowInfo("Should decloak");
            }
        }

        private class CloakAction : ActionsList.CloakAction
        {
            public override void ActionTake()
            {
                if (Source.State.Charges != 0)
                {
                    Source.State.SpendCharge();
                    base.ActionTake();
                }
            }
        }

    }
}