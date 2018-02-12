﻿using Upgrade;
using Ship;
using SubPhases;
using Abilities;
using System;

namespace UpgradesList
{
    public class Gunner : GenericUpgrade
    {
        public Gunner() : base()
        {
            Types.Add(UpgradeType.Crew);
            Name = "Gunner";
            Cost = 5;

            UpgradeAbilities.Add(new GunnerAbility());
        }
    }
}

namespace Abilities
{
    public class GunnerAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnAttackMissedAsAttacker += CheckGunnerAbility;
            Phases.OnRoundEnd += ClearIsAbilityUsedFlag;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnAttackMissedAsAttacker -= CheckGunnerAbility;
            Phases.OnRoundEnd -= ClearIsAbilityUsedFlag;
        }

        private void CheckGunnerAbility()
        {
            if (!IsAbilityUsed && !HostShip.IsCannotAttackSecondTime)
            {
                IsAbilityUsed = true;

                // Trigger must be registered just before it's resolution
                HostShip.OnCombatCheckExtraAttack += RegisterSecondAttackTrigger;
            }
        }

        private void RegisterSecondAttackTrigger()
        {
            HostShip.OnCombatCheckExtraAttack -= RegisterSecondAttackTrigger;

            RegisterAbilityTrigger(TriggerTypes.OnCombatCheckExtraAttack, UseGunnerAbility);
        }

        private void UseGunnerAbility(object sender, System.EventArgs e)
        {
            if (!HostShip.IsCannotAttackSecondTime)
            {
                Messages.ShowInfo(HostShip.PilotName + " can perform second attack\nfrom primary weapon");

                HostShip.IsCannotAttackSecondTime = true;

                Combat.StartAdditionalAttack(
                    HostShip,
                    FinishAdditionalAttack,
                    IsPrimaryWeaponShot
                );
            }
            else
            {
                Messages.ShowErrorToHuman(string.Format("{0} cannot attack one more time", HostShip.PilotName));
                Triggers.FinishTrigger();
            }
        }

        private void FinishAdditionalAttack()
        {
            // If attack is skipped, set this flag, otherwise regular attack can be performed second time
            HostShip.IsAttackPerformed = true;

            Triggers.FinishTrigger();
        }

        private bool IsPrimaryWeaponShot(GenericShip defender, IShipWeapon weapon)
        {
            bool result = false;

            if (Combat.ChosenWeapon is PrimaryWeaponClass)
            {
                result = true;
            }
            else
            {
                Messages.ShowError("Attack must be performed from primary weapon");
            }

            return result;
        }
    } 
}