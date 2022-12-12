using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireClone
{
    [DefaultExecutionOrder(-1)]
    public class UnitManager : Etienne.Singleton<UnitManager>
    {
        public List<Unit> Units => units;
        [SerializeField] private List<Unit> units;

        private WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
        }

        private IEnumerator Start()
        {
            while (enabled)
            {
                Vector3 playerPosition = Player.Instance.transform.position;
                units.Sort((u1, u2) => Vector3.Distance(u1.transform.position, playerPosition).CompareTo(Vector3.Distance(u2.transform.position, playerPosition)));
                yield return waitForSeconds;
            }
        }
    }
}
