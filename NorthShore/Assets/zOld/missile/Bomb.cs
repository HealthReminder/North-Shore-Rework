using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    // launch variables
    [SerializeField] private Transform TargetObject;
    [Range(1.0f, 6.0f)] public float TargetRadius;
    [Range(20.0f, 75.0f)] public float LaunchAngle;

    // state
    private bool bTouchingGround;

    // cache
    private Rigidbody rigid;
    //private Vector3 initialPosition;
    private Quaternion initialRotation;

    public ParticleSystem[] explosionParticles;
    public bool  wasLaunched = false;
   public  bool canExplode = false;
    public int initialHeight = 100;

	//Launch variables
	[Range(0.0f, 10.0f)] public float TargetHeightOffsetFromGround;
    

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = true;
        //initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // launches the object towards the TargetObject with a given LaunchAngle
    void Launch(){
         wasLaunched = true;
        initialHeight = (int)transform.position.y;
         rigid.isKinematic = false;
       // think of it as top-down view of vectors: 
    //   we don't care about the y-component(height) of the initial and target position.
    Vector3 projectileXZPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    Vector3 targetXZPos = new Vector3(TargetObject.position.x, transform.position.y, TargetObject.position.z);
    
    // rotate the object to face the target
    transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = TargetObject.position.y - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)) );
        float Vy = tanAlpha * Vz;

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity;
        
        }

    // Update is called once per frame
    void Update ()
    {
        if(transform.position.y > initialHeight +5 && !canExplode && wasLaunched) {
            transform.GetComponent<CapsuleCollider>().enabled = true;
            canExplode = true;
        }
		
	 
        if (Input.GetKeyDown(KeyCode.U))
        {
            print("Pressed U");
                Launch();
            
        }

       
          if (!bTouchingGround)
        {
            // updatje the rotation of the projectile during trajectory motion
            transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
        }
    }
     void OnCollisionEnter()
    {
        bTouchingGround = true;
        if( wasLaunched)
        if(canExplode){
        transform.DetachChildren();
        foreach(ParticleSystem p in explosionParticles) {
            p.Emit(1);
        }
        Destroy(TargetObject.gameObject);
        Destroy(gameObject);
        }
    }

    void OnCollisionExit()
    {
        bTouchingGround = false;
    }
}