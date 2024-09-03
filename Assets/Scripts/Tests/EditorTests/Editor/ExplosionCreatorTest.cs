using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ExplosionCreatorTest
{
    private GameObject TestingEnemy1;
    private GameObject TestingEnemy2;
    private GameObject TestingEnemy3;
    private GameObject TestingEnemy4;

    private GameObject TestingExplosion;

    [SetUp]
    public void Setup()
    {
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EnemyPrefabs/Alien.prefab");

        TestingEnemy1 = GameObject.Instantiate(enemyPrefab, new Vector3(25, 0, 0), Quaternion.identity);
        TestingEnemy2 = GameObject.Instantiate(enemyPrefab, new Vector3(0, 0, -25), Quaternion.identity);
        TestingEnemy3 = GameObject.Instantiate(enemyPrefab, new Vector3(3, 12.674f, 1), Quaternion.identity);
        TestingEnemy4 = GameObject.Instantiate(enemyPrefab, new Vector3(0, 0, 30), Quaternion.identity);

        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ParticleEffects/ExplosionEffect.prefab");
        TestingExplosion = GameObject.Instantiate(explosionPrefab);
    }

    [Test]
    public void ExplosionTest()
    {
        Assert.IsTrue(TestingExplosion.GetComponent<ExplosionCreator>());

        ExplosionCreator creator = TestingExplosion.GetComponent<ExplosionCreator>();
        creator.InitializeExplosion(null, Vector3.zero, 25, 10, false);

        Assert.IsTrue(TestingEnemy1.GetComponent<StatManager>().GetHealth() == TestingEnemy1.GetComponent<StatManager>().GetStat(NumericalStats.Health) - 10);
        Assert.IsTrue(TestingEnemy2.GetComponent<StatManager>().GetHealth() == TestingEnemy1.GetComponent<StatManager>().GetStat(NumericalStats.Health) - 10);
        Assert.IsTrue(TestingEnemy3.GetComponent<StatManager>().GetHealth() == TestingEnemy1.GetComponent<StatManager>().GetStat(NumericalStats.Health) - 10);
        Assert.IsTrue(TestingEnemy4.GetComponent<StatManager>().GetHealth() == TestingEnemy1.GetComponent<StatManager>().GetStat(NumericalStats.Health));
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(TestingEnemy1);
        GameObject.DestroyImmediate(TestingEnemy2);
        GameObject.DestroyImmediate(TestingEnemy3);
        GameObject.DestroyImmediate(TestingEnemy4);
        GameObject.DestroyImmediate(TestingExplosion);
    }
}
