using UnityEngine;

namespace core.zqc.bombs
{
    public class AmmunitionDepot : MonoBehaviour
    {
        public BombGenerator linkedGenerator;

        public void FillBomb(Bomb bomb)
        {
            Destroy(bomb.gameObject);
            linkedGenerator.AddBomb();
        }
    }
}

