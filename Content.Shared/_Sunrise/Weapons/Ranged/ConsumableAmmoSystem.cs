using Robust.Shared.GameObjects;
using Content.Shared._Sunrise.Weapons.Ranged;
using Content.Shared.Interaction.Events;
using Content.Shared.Stacks;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System;
using Content.Shared.Projectiles;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Examine;

namespace Content.Shared._Sunrise.Weapons.Ranged
{
    public sealed class ConsumableAmmoSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedStackSystem _stackSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedProjectileSystem _projectileSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ConsumableAmmoComponent, InteractUsingEvent>(OnLoadAmmo);
            SubscribeLocalEvent<ConsumableAmmoComponent, ShotAttemptedEvent>(OnShotAttempted);
            SubscribeLocalEvent<ConsumableAmmoComponent, TakeAmmoEvent>(OnTakeAmmo);
            SubscribeLocalEvent<ConsumableAmmoComponent, ExaminedEvent>(OnExamine);
        }

        private void OnLoadAmmo(EntityUid uid, ConsumableAmmoComponent ammo, InteractUsingEvent args)
        {
            if (args.Handled)
            {
                return;
            }

            var itemInHand = args.Used;

            if (!TryComp<StackComponent>(itemInHand, out var stack))
            {
                return;
            }

            var itemProtoId = _entityManager.GetComponent<MetaDataComponent>(itemInHand).EntityPrototype?.ID ?? string.Empty;

            if (!ammo.LoadableItemIds.Contains(itemProtoId) && !ammo.MultiplerLoadableItemIds.Contains(itemProtoId))
            {
                return;
            }

            if (ammo.CurrentCharges >= ammo.MaxCharges)
            {
                _popupSystem.PopupPredicted(Loc.GetString("consumable-ammo-fully-charged"), uid, args.User);
                args.Handled = true;
                return;
            }

            var chargesPerItem = 1.0f / ammo.ItemsPerCharge;
            if (ammo.MultiplerLoadableItemIds.Contains(itemProtoId))
            {
                chargesPerItem *= ammo.Multipler;
            }

            var chargesCanAdd = ammo.MaxCharges - ammo.CurrentCharges;
            var itemsNeeded = (int)Math.Ceiling(chargesCanAdd / chargesPerItem);
            var itemsToConsume = Math.Min(itemsNeeded, stack.Count);

            if (itemsToConsume == 0)
            {
                return;
            }

            if (!_stackSystem.Use(itemInHand, itemsToConsume, stack))
            {
                return;
            }

            var actualChargesAdded = (int)Math.Floor(itemsToConsume * chargesPerItem);

            var roomLeft = ammo.MaxCharges - ammo.CurrentCharges;
            actualChargesAdded = Math.Min(actualChargesAdded, roomLeft);

            ammo.CurrentCharges += actualChargesAdded;

            _popupSystem.PopupPredicted(Loc.GetString("consumable-ammo-charged", ("chargesAdded", actualChargesAdded), ("currentCharges", ammo.CurrentCharges), ("maxCharges", ammo.MaxCharges)), uid, args.User);

            args.Handled = true;
            Dirty(uid, ammo);
        }

        private void OnShotAttempted(EntityUid uid, ConsumableAmmoComponent ammo, ref ShotAttemptedEvent args)
        {
            if (args.Cancelled)
            {
                return;
            }

            if (ammo.CurrentCharges < ammo.ChargesPerShot)
            {
                args.Cancel();
                return;
            }
        }

        private void OnTakeAmmo(EntityUid uid, ConsumableAmmoComponent ammo, ref TakeAmmoEvent args)
        {
            if (ammo.CurrentCharges < args.Shots * ammo.ChargesPerShot)
            {
                args.Reason = Loc.GetString("plasma-cutter-empty");
                return;
            }
            ammo.CurrentCharges -= args.Shots * ammo.ChargesPerShot;
            for (var i = 0; i < args.Shots; i++)
            {
                var projectile = Spawn(ammo.ProjectilePrototypeId, args.Coordinates);
                if (!TryComp<AmmoComponent>(projectile, out var ammoComp))
                {
                    continue;
                }
                args.Ammo.Add((projectile, ammoComp));
            }
            Dirty(uid, ammo);
        }

        private void OnExamine(EntityUid uid, ConsumableAmmoComponent ammo, ref ExaminedEvent args)
        {
            args.PushMarkup(Loc.GetString("charges-count-text", ("chargesText", ammo.CurrentCharges)));
        }
    }
}
