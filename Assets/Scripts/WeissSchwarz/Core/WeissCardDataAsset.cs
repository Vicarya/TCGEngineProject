using UnityEngine;

namespace TCG.Weiss {
    [CreateAssetMenu(menuName = "TCG/Weiss Card Data Asset", fileName = "WeissCard_")]
    public class WeissCardDataAsset : ScriptableObject
    {
        public WeissCardData Data = new WeissCardData();
    }
}
