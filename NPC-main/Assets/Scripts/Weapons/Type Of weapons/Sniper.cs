using UnityEngine;


public class Sniper : Weapon
{
    [Header("Sniper Specific")]
    [SerializeField] private bool canPenetrateTargets = false;
    [SerializeField] private int maxPenetrations = 2;
    
    protected override void PerformShot()
    {
        Vector3 shootDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        
        if (!canPenetrateTargets)
        {
            
            if (FireRaycast(out RaycastHit hit, shootDirection))
            {
                CreateBulletTracer(origin, hit.point);
                CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                ApplyDamage(hit.collider.gameObject, weaponData.damage);
                Debug.DrawLine(origin, hit.point, Color.green, 0.5f);
            }
            else
            {
                Vector3 endPoint = origin + shootDirection * weaponData.range;
                CreateBulletTracer(origin, endPoint);
            }
        }
        else
        {
          
            RaycastHit[] hits = Physics.RaycastAll(origin, shootDirection, weaponData.range, hitLayers);
            
            if (hits.Length > 0)
            {
      
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                
                int penetrationCount = 0;
                Vector3 lastPoint = origin;
                
                foreach (RaycastHit hit in hits)
                {
                    if (penetrationCount >= maxPenetrations)
                        break;
                    
                
                    CreateBulletTracer(lastPoint, hit.point);
                    
                    CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                    
                 
                    float damageMultiplier = 1f - (penetrationCount * 0.3f);
                    ApplyDamage(hit.collider.gameObject, weaponData.damage * damageMultiplier);
                    
                    Debug.DrawLine(lastPoint, hit.point, Color.green, 0.5f);
                    
                    lastPoint = hit.point;
                    penetrationCount++;
                }
                
               
                if (lastPoint != origin)
                {
                    Vector3 finalEnd = origin + shootDirection * weaponData.range;
                    CreateBulletTracer(lastPoint, finalEnd);
                }
            }
            else
            {
                
                Vector3 endPoint = origin + shootDirection * weaponData.range;
                CreateBulletTracer(origin, endPoint);
            }
        }
    }
}