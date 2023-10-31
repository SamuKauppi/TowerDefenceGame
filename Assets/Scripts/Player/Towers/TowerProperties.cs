using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TowerProperties
{
    // Tower name
    public string name;
    public string description;
    public string[] upgradePaths;
    // Tower sprites
    public Sprite barrelSprite;
    public Sprite towerSprite;
    // Bullets it shoots
    public string bullet = "bullet";
    // How often it shoots 
    public float attackSpeed;
    // How fast does the turret track enemies
    public float rotationSpeed;
    // How accurate it is (+-deg added to barrel rotation) 
    public int accuracy;
    // How far it shoots
    public float attackRange;
    // How long tower has to charge between bursts (if 0 = ignored)
    public float chargeTime;
    // How many bullets it shoots in bursts (has to be at least 1)
    public int burstShots = 1;
    // How many spread shots it fires (has to be at least 1)
    public int spreadShots = 1;
    // Degree per spread shot fired (ignored if burstShots is less than 2)
    public int degreePerShot;
    // Cost to upgrade
    public int cost;
}
