using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._Sunrise.Weapons.Ranged;

public sealed class ConsumableAmmoSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ConsumableAmmoComponent, InteractUsingEvent>(OnLoadAmmo);
        SubscribeLocalEvent<ConsumableAmmoComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ConsumableAmmoComponent, TakeAmmoEvent>(OnTakeAmmo);
        SubscribeLocalEvent<ConsumableAmmoComponent, ExaminedEvent>(OnExamine);
    }

    private void OnLoadAmmo(Entity<ConsumableAmmoComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        var itemProtoId = Prototype(args.Used);

        if (itemProtoId == null)
            return;

        if (!ent.Comp.LoadableItemIds.Contains(itemProtoId) && !ent.Comp.MultiplierLoadableItemIds.Contains(itemProtoId))
            return;

        if (ent.Comp.CurrentCharges >= ent.Comp.MaxCharges)
        {
            _popup.PopupPredicted(Loc.GetString("consumable-ammo-fully-charged"), ent.Owner, args.User);
            args.Handled = true;
            return;
        }

        var chargesPerItem = 1.0f / ent.Comp.ItemsPerCharge;

        if (ent.Comp.MultiplierLoadableItemIds.Contains(itemProtoId))
            chargesPerItem *= ent.Comp.Multiplier;

        var chargesCanAdd = ent.Comp.MaxCharges - ent.Comp.CurrentCharges;
        var itemsNeeded = (int)Math.Ceiling(chargesCanAdd / chargesPerItem);
        var itemsToConsume = Math.Min(itemsNeeded, stack.Count);

        if (itemsToConsume == 0)
            return;

        if (!_stack.Use(args.Used, itemsToConsume, stack))
            return;

        var actualChargesAdded = (int)Math.Floor(itemsToConsume * chargesPerItem);

        var roomLeft = ent.Comp.MaxCharges - ent.Comp.CurrentCharges;
        actualChargesAdded = Math.Min(actualChargesAdded, roomLeft);

        ent.Comp.CurrentCharges += actualChargesAdded;

        var message = Loc.GetString("consumable-ammo-charged", ("chargesAdded", actualChargesAdded), ("currentCharges", ent.Comp.CurrentCharges), ("maxCharges", ent.Comp.MaxCharges));
        _popup.PopupPredicted(message, ent.Owner, args.User);

        args.Handled = true;
        Dirty(ent.Owner, ent.Comp);
    }

    private void OnShotAttempted(Entity<ConsumableAmmoComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.CurrentCharges < ent.Comp.ChargesPerShot)
        {
            args.Cancel();
            return;
        }
    }

    private void OnTakeAmmo(Entity<ConsumableAmmoComponent> ent, ref TakeAmmoEvent args)
    {
        if (ent.Comp.CurrentCharges < args.Shots * ent.Comp.ChargesPerShot)
        {
            args.Reason = Loc.GetString("consumable-ammo-empty");
            return;
        }
        ent.Comp.CurrentCharges -= args.Shots * ent.Comp.ChargesPerShot;
        for (var i = 0; i < args.Shots; i++)
        {
            var projectile = Spawn(ent.Comp.ProjectilePrototypeId, args.Coordinates);
            if (!TryComp<AmmoComponent>(projectile, out var ammoComp))
                continue;
            args.Ammo.Add((projectile, ammoComp));
        }
        Dirty(ent.Owner, ent.Comp);
    }

    private void OnExamine(Entity<ConsumableAmmoComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("charges-count-text", ("chargesText", ent.Comp.CurrentCharges)));
    }
}
