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

    /// <summary>
    /// предметы(EntProtoId) с мультипликатором(int), то есть при использовании их количество полученных с них(EntProtoId) зарядов умножается на int
    /// </summary>
    [DataField, ViewVariables]
    public Dictionary<EntProtoId, int> LoadableItems = new Dictionary<EntProtoId, int>();

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
    public SoundSpecifier? EmptySound;

    [DataField]
    public bool PopupShownOnEmpty = false;
}
