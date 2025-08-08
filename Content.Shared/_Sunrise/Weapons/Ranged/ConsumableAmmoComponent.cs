using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Shared._Sunrise.Weapons.Ranged;
[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class ConsumableAmmoComponent : Component
{
    [DataField, ViewVariables, AutoNetworkedField]
    public int CurrentCharges;
    [DataField, ViewVariables]
    public int MaxCharges = 30;
    [DataField, ViewVariables]
    public List<EntProtoId> LoadableItemIds = [];
    /// <summary>
    /// предметы с мультипликатором, то есть при использовании их количество полученных с них зарядов умножается на Multipler
    /// </summary>
    [DataField, ViewVariables]
    public List<EntProtoId> MultiplierLoadableItemIds = [];
    [DataField, ViewVariables]
    public int Multiplier = 2;
    [DataField, ViewVariables, AutoNetworkedField]
    public int ChargesPerShot = 1;
    /// <summary>
    /// отношение затрат материала на один заряд
    /// </summary>
    [DataField, ViewVariables]
    public float ItemsPerCharge = 1f;
    [DataField, ViewVariables]
    public EntProtoId ProjectilePrototypeId;
    [DataField, ViewVariables]
    public SoundSpecifier? LoadSound;
    [DataField, ViewVariables]
    public SoundSpecifier? EmptySoundl;
    [DataField, ViewVariables]
    public bool PopupShownOnEmpty = false;
}
