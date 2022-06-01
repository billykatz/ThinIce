using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    private List<ScriptableUnit> _units;

    private void Awake() {
        Instance = this;
        Debug.Log("Game Manager Awake()");

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnEnemyUnit() {
        var randomPrefab =  GetRandomUnit<BaseUnit>(Faction.Enemy);
        var spawnedUnit = Instantiate(randomPrefab);
        var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

        randomSpawnTile.SetUnit(spawnedUnit);

        GameManager.Instance.EndGameState(GameState.SpawnEnemies);

    }

    public BaseHero SpawnHeroUnit() {
        var randomPrefab =  GetRandomUnit<BaseUnit>(Faction.Hero);
        var spawnedUnit = Instantiate(randomPrefab);
        return (BaseHero)spawnedUnit;
    }

    private T GetRandomUnit<T>(Faction faction) where T: BaseUnit {
        return (T)_units.Where(u=>u.Faction==faction).OrderBy(o=>Random.value).First().UnitPrefab;
    }
}
