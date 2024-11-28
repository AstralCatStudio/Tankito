using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBulletCannon 
{
    public void Shoot(Vector2 aimVector);
    public void ShootBullet(Vector2 position, Vector2 direction, int spawnN);
}
