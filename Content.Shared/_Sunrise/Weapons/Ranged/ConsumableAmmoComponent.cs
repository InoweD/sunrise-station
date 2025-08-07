using Robust.Shared.GameStates;

namespace Content.Shared._Sunrise.Weapons.Ranged
{
    [RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
    public sealed partial class ConsumableAmmoComponent : Component
    {
        [AutoNetworkedField]
        [DataField("currentCharges")]
        public int CurrentCharges = 0; 

        [DataField("maxCharges")] 
        public int MaxCharges = 30; 

        // обязательно
        [DataField("loadableItemIds", required: true)]
        public List<string> LoadableItemIds = new() {};

        // предметы с мультипликатором, то есть при использовании их
        // количество полученных с них зарядов умножается на Multipler
        [DataField("multiplerLoadableItemIds")]
        public List<string> MultiplerLoadableItemIds = new() {};

        [DataField("multipler")]
        public int Multipler = 2;
        // предметы с мультипликатором

        [DataField("chargesPerShot")] 
        public int ChargesPerShot = 1; 

        // отношение потребляемых материалов к полученным зарядам
        // к прмиеру: 0.2 значит, что нужно пять предметов для одного заряда
        [DataField("itemsPerCharge")] 
        public float ItemsPerCharge = 1f;

        // обязательно
        [DataField("projectilePrototypeId", required: true)]
        public string ProjectilePrototypeId;
    }
}
