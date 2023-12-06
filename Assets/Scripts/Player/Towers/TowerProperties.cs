using System;
using UnityEngine;

[Serializable]
public class TowerProperties
{
    // Tower name
    public string name;
    public TowerUpgrade upgradeIdent;
    [TextArea(3, 5)]
    public string description;
    public TowerUpgrade[] upgradePaths;
    // Tower sprites
    public Sprite barrelSprite;
    public Sprite towerSprite;
    // Bullets it shoots
    public GameEntity bulletIdent;
    // How often it shoots 
    public float attackSpeed;
    // How fast does the turret track enemies
    public float rotationSpeed;
    // How accurate it is (+-deg added to barrel rotation) 
    public int accuracy;
    // The max angle between barrel and enemy before tower starts firing
    public float aimThreshold = 30f;
    // How far it detects enemies
    public float attackRange;
    // How long tower has to charge between bursts (if 0 = ignored)
    public float chargeTime;
    // How many bullets it shoots in bursts (has to be at least 1)
    public int burstShots = 1;
    // Does the tower aim while shooting a burst
    public bool aimWhileFiring = true;
    // How many spread shots it fires (has to be at least 1)
    public int spreadShots = 1;
    // Degree per spread shot fired (ignored if burstShots is less than 2)
    public int degreePerShot;
    // Cost to upgrade
    public int cost;
}
